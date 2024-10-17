using ShellExtensions.ComInterfaces;
using ShellExtensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions.ComObjects
{
    [GeneratedComClass]
    internal partial class CClassFactory : IClassFactory
    {
        private readonly Func<object>? createManagedObject;
        private readonly Func<object, object>? createComObject;

        public CClassFactory(
            Func<object>? createManagedObject,
            Func<object, object>? createComObject)
        {
            this.createManagedObject = createManagedObject;
            this.createComObject = createComObject;
        }

        public unsafe int CreateInstance([Optional] void* pUnkOuter, Guid* riid, void** ppvObject)
        {
            if (pUnkOuter != null) return HResults.CLASS_E_NOAGGREGATION;

            try
            {
                var managed = createManagedObject?.Invoke();
                if (managed != null)
                {
                    var comObject = createComObject?.Invoke(managed);
                    if (comObject != null)
                    {
                        var ptr = ComHelpers.ComWrappers.GetOrCreateComInterfaceForObject(comObject, CreateComInterfaceFlags.None);
                        if (ptr != IntPtr.Zero)
                        {
                            try
                            {
                                return ((Windows.Win32.System.Com.IUnknown*)ptr)->QueryInterface(riid, ppvObject);
                            }
                            finally
                            {
                                Marshal.Release(ptr);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }

            return HResults.E_NOINTERFACE;
        }

        public int LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock)
        {
            if (fLock)
            {
                ShellExtensionsClassFactory.DllAddRef();
            }
            else
            {
                ShellExtensionsClassFactory.DllRelease();
            }

            return HResults.S_OK;
        }
    }
}
