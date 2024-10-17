using ShellExtensions.ComInterfaces;
using ShellExtensions.ComObjects;
using ShellExtensions.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.System.Com;

namespace ShellExtensions
{
    public struct ShellExtensionsInProcessCookie : IDisposable
    {
        private Guid clsid;

        internal ShellExtensionsInProcessCookie(Guid clsid)
        {
            this.clsid = clsid;
        }

        public readonly bool IsNull => clsid == Guid.Empty;

        public void Dispose()
        {
            if (clsid == Guid.Empty)
            {
                ShellExtensionsClassFactory.UnregisterFactory(clsid);
                clsid = Guid.Empty;
            }
        }
    }

    public struct ShellExtensionsLocalServerCookie : IDisposable
    {
        private uint cookie;

        internal ShellExtensionsLocalServerCookie(uint cookie)
        {
            this.cookie = cookie;
        }

        public readonly bool IsNull => cookie == 0;

        public void Dispose()
        {
            if (cookie != 0)
            {
                var hr = Windows.Win32.PInvoke.CoRevokeClassObject(cookie);
                if (hr.Succeeded)
                {
                    cookie = 0;
                }
                hr.ThrowOnFailure();
            }
        }
    }

    public static class ShellExtensionsClassFactory
    {
        private static Dictionary<Guid, ComObjectInfo> factories = new Dictionary<Guid, ComObjectInfo>();
        private static long g_cRefModule = 0;

        public static int DllCanUnloadNow()
        {
            return g_cRefModule >= 1 ? 1 : 0;
        }

        internal static void DllAddRef()
        {
            Interlocked.Increment(ref g_cRefModule);
        }

        internal static void DllRelease()
        {
            Interlocked.Decrement(ref g_cRefModule);
        }

        public unsafe static int DllGetClassObject(Guid* clsid, Guid* riid, void** ppv)
        {
            lock (factories)
            {
                if (factories.TryGetValue(*clsid, out var objInfo))
                {
                    var factory = new CClassFactory(objInfo.CreateManagedObject, objInfo.CreateComObject);

                    var pFactory = ComHelpers.ComWrappers.GetOrCreateComInterfaceForObject(factory, CreateComInterfaceFlags.None);

                    var result = ((Windows.Win32.System.Com.IUnknown*)pFactory)->QueryInterface(riid, ppv);
                    ((Windows.Win32.System.Com.IUnknown*)pFactory)->Release();

                    return result.Value;
                }
            }

            return HResults.CLASS_E_CLASSNOTAVAILABLE;
        }

        public static ShellExtensionsInProcessCookie RegisterInProcess<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Guid clsid)
            where T : IShellExtensions, new() => RegisterInProcess<T>(clsid, () => FastActivator.CreateInstance<T>());

        public static ShellExtensionsInProcessCookie RegisterInProcess<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Guid clsid, Func<T> factory)
            where T : IShellExtensions
        {
            var objInfo = CreateObjectInfo<T>(() => factory.Invoke());

            if (RegisterFactory(clsid, objInfo))
            {
                return new ShellExtensionsInProcessCookie(clsid);
            }

            return default;
        }


        public unsafe static ShellExtensionsLocalServerCookie RegisterLocalServer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Guid clsid, REGCLS flags)
            where T : IShellExtensions, new() => RegisterLocalServer<T>(clsid, flags, () => FastActivator.CreateInstance<T>());

        public unsafe static ShellExtensionsLocalServerCookie RegisterLocalServer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Guid clsid, REGCLS flags, Func<T> factory)
            where T : IShellExtensions
        {
            var objInfo = CreateObjectInfo<T>(() => factory.Invoke());

            var classFactory = new CClassFactory(objInfo.CreateManagedObject, objInfo.CreateComObject);
            var pFactory = ComHelpers.ComWrappers.GetOrCreateComInterfaceForObject(classFactory, CreateComInterfaceFlags.None);

            var flags2 = (Windows.Win32.System.Com.REGCLS)0;
            if ((flags & REGCLS.REGCLS_MULTIPLEUSE) != 0) flags2 |= Windows.Win32.System.Com.REGCLS.REGCLS_MULTIPLEUSE;
            if ((flags & REGCLS.REGCLS_MULTI_SEPARATE) != 0) flags2 |= Windows.Win32.System.Com.REGCLS.REGCLS_MULTI_SEPARATE;
            if ((flags & REGCLS.REGCLS_SUSPENDED) != 0) flags2 |= Windows.Win32.System.Com.REGCLS.REGCLS_SUSPENDED;
            if ((flags & REGCLS.REGCLS_SURROGATE) != 0) flags2 |= Windows.Win32.System.Com.REGCLS.REGCLS_SURROGATE;
            if ((flags & REGCLS.REGCLS_AGILE) != 0) flags2 |= Windows.Win32.System.Com.REGCLS.REGCLS_AGILE;

            var hr = Windows.Win32.PInvoke.CoRegisterClassObject(
                in clsid,
                (IUnknown*)pFactory,
                CLSCTX.CLSCTX_LOCAL_SERVER,
                flags2,
                out var cookie);

            if (hr.Succeeded) return new ShellExtensionsLocalServerCookie(cookie);

            hr.ThrowOnFailure();
            return default;
        }

        internal static bool RegisterFactory(Guid clsid, ComObjectInfo objInfo)
        {
            ArgumentNullException.ThrowIfNull(objInfo);
            if (objInfo.IIDTable == null
                || objInfo.CreateComObject == null
                || objInfo.CreateManagedObject == null) return false;

            lock (factories)
            {
                return factories.TryAdd(clsid, objInfo);
            }
        }

        internal static bool UnregisterFactory(Guid clsid)
        {
            lock (factories)
            {
                return factories.Remove(clsid);
            }
        }

        internal record ComObjectInfo(
            IReadOnlyCollection<Guid> IIDTable,
            Func<object>? CreateManagedObject,
            Func<object, object>? CreateComObject);

        private static ComObjectInfo CreateObjectInfo<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(Func<object> factory)
            where T : IShellExtensions
        {
            return new ComObjectInfo(
                CreateIIDTable(),
                factory,
                ComObjectFactory());

            static Func<object, object>? ComObjectFactory()
            {
                if (typeof(T).IsAssignableTo(typeof(ExplorerCommand)))
                {
                    return managedObj => new CExplorerCommand((ExplorerCommand)managedObj);
                }
                else if (typeof(T).IsAssignableTo(typeof(PreviewHandler)))
                {
                    return managedObj => new CPreviewHandler((PreviewHandler)managedObj);
                }
                else if (typeof(T).IsAssignableTo(typeof(ThumbnailProvider)))
                {
                    return managedObj => new CThumbnailProvider((ThumbnailProvider)managedObj);
                }

                return null;
            }

            static IReadOnlyCollection<Guid> CreateIIDTable()
            {
                var hashSet = new HashSet<Guid>();

                if (typeof(T).IsAssignableTo(typeof(ExplorerCommand)))
                {
                    hashSet.Add(new Guid(ComInterfaceIds.IID_IExplorerCommand));
                }
                else if (typeof(T).IsAssignableTo(typeof(PreviewHandler)))
                {
                    hashSet.Add(new Guid(ComInterfaceIds.IID_IPreviewHandler));
                }
                else if (typeof(T).IsAssignableTo(typeof(ThumbnailProvider)))
                {
                    hashSet.Add(new Guid(ComInterfaceIds.IID_IThumbnailProvider));
                }

                if (hashSet.Count > 0)
                {
                    hashSet.Add(new Guid(ComInterfaceIds.IID_IUnknown));
                }

                return hashSet;
            }
        }

        [Flags]
        public enum REGCLS
        {
            /// <summary>After an application is connected to a class object with <a href="https://docs.microsoft.com/windows/desktop/api/combaseapi/nf-combaseapi-cogetclassobject">CoGetClassObject</a>, the class object is removed from public view so that no other applications can connect to it. This value is commonly used for single document interface (SDI) applications. Specifying this value does not affect the responsibility of the object application to call <a href="https://docs.microsoft.com/windows/desktop/api/combaseapi/nf-combaseapi-corevokeclassobject">CoRevokeClassObject</a>; it must always call <b>CoRevokeClassObject</b> when it is finished with an object class.</summary>
            REGCLS_SINGLEUSE = 0x00000000,
            /// <summary>Multiple applications can connect to the class object through calls to <a href="https://docs.microsoft.com/windows/desktop/api/combaseapi/nf-combaseapi-cogetclassobject">CoGetClassObject</a>. If both the REGCLS_MULTIPLEUSE and CLSCTX_LOCAL_SERVER are set in a call to <a href="https://docs.microsoft.com/windows/desktop/api/combaseapi/nf-combaseapi-coregisterclassobject">CoRegisterClassObject</a>, the class object is also automatically registered as an in-process server, whether CLSCTX_INPROC_SERVER is explicitly set.</summary>
            REGCLS_MULTIPLEUSE = 0x00000001,
            /// <summary>
            /// <para>Useful for registering separate CLSCTX_LOCAL_SERVER and CLSCTX_INPROC_SERVER class factories through calls to <a href="https://docs.microsoft.com/windows/desktop/api/combaseapi/nf-combaseapi-cogetclassobject">CoGetClassObject</a>. If REGCLS_MULTI_SEPARATE is set, each execution context must be set separately; <a href="https://docs.microsoft.com/windows/desktop/api/combaseapi/nf-combaseapi-coregisterclassobject">CoRegisterClassObject</a> does not automatically register an out-of-process server (for which CLSCTX_LOCAL_SERVER is set) as an in-process server. This allows the EXE to create multiple instances of the object for in-process needs, such as self embeddings, without disturbing its CLSCTX_LOCAL_SERVER registration. If an EXE registers a REGCLS_MULTI_SEPARATE class factory and a CLSCTX_INPROC_SERVER class factory, instance creation calls that specify CLSCTX_INPROC_SERVER in the <a href="https://docs.microsoft.com/windows/desktop/api/wtypesbase/ne-wtypesbase-clsctx">CLSCTX</a> parameter executed by the EXE would be satisfied locally without approaching the SCM. This mechanism is useful when the EXE uses functions such as <a href="https://docs.microsoft.com/windows/desktop/api/ole/nf-ole-olecreate">OleCreate</a> and <a href="https://docs.microsoft.com/windows/desktop/api/ole2/nf-ole2-oleload">OleLoad</a> to create embeddings, but at the same does not wish to launch a new instance of itself for the self-embedding case. The distinction is important for embeddings because the default handler aggregates the proxy manager by default and the application should override this default behavior by calling <a href="https://docs.microsoft.com/windows/desktop/api/ole2/nf-ole2-olecreateembeddinghelper">OleCreateEmbeddingHelper</a> for the self-embedding case. If your application need not distinguish between the local and inproc case, you need not register your class factory using REGCLS_MULTI_SEPARATE. In fact, the application incurs an extra network round trip to the SCM when it registers its MULTIPLEUSE class factory as MULTI_SEPARATE and does not register another class factory as INPROC_SERVER.</para>
            /// <para><see href="https://learn.microsoft.com/windows/win32/api/combaseapi/ne-combaseapi-regcls#members">Read more on docs.microsoft.com</see>.</para>
            /// </summary>
            REGCLS_MULTI_SEPARATE = 0x00000002,
            /// <summary>
            /// <para>Suspends registration and activation requests for the specified CLSID until there is a call to <a href="https://docs.microsoft.com/windows/desktop/api/combaseapi/nf-combaseapi-coresumeclassobjects">CoResumeClassObjects</a>. This is used typically to register the CLSIDs for servers that can register multiple class objects to reduce the overall registration time, and thus the server application startup time, by making a single call to the SCM, no matter how many CLSIDs are registered for the server. <div class="alert"><b>Note</b>  This flag prevents COM activation errors from a possible race condition between an application shutting down and that application attempting to register a COM class.</div> <div> </div></para>
            /// <para><see href="https://learn.microsoft.com/windows/win32/api/combaseapi/ne-combaseapi-regcls#members">Read more on docs.microsoft.com</see>.</para>
            /// </summary>
            REGCLS_SUSPENDED = 0x00000004,
            /// <summary>The class object is a surrogate process used to run DLL servers. The class factory registered by the surrogate process is not the actual class factory implemented by the DLL server, but a generic class factory implemented by the surrogate. This generic class factory delegates instance creation and marshaling to the class factory of the DLL server running in the surrogate. For further information on DLL surrogates, see the <a href="https://docs.microsoft.com/windows/desktop/com/dllsurrogate">DllSurrogate</a> registry value.</summary>
            REGCLS_SURROGATE = 0x00000008,
            /// <summary>
            /// <para>The class object aggregates the free-threaded marshaler and will be made visible to all inproc apartments. Can be used together with other flags. For example, REGCLS_AGILE | REGCLS_MULTIPLEUSE to register a class object that can be used multiple times from different apartments. Without other flags, behavior will retain REGCLS_SINGLEUSE semantics in that only one instance can be generated.</para>
            /// <para><see href="https://learn.microsoft.com/windows/win32/api/combaseapi/ne-combaseapi-regcls#members">Read more on docs.microsoft.com</see>.</para>
            /// </summary>
            REGCLS_AGILE = 0x00000010,
        }
    }
}
