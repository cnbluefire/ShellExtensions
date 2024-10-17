using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions
{
    public static class DllModule
    {
        private static nint hInstance;
        private static string? dllFilePath;
        private static char[] pathSeparatorChars = ['/', '\\'];

        public static unsafe nint HINSTANCE
        {
            get
            {
                if (hInstance == 0)
                {
                    lock (pathSeparatorChars)
                    {
                        if (hInstance == 0)
                        {
                            const uint GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004;
                            const uint GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 0x00000002;

                            void* funcPtr = (delegate* unmanaged[Stdcall]<void>)&STUB;

                            fixed (nint* pHInstance = &hInstance)
                            {
                                Windows.Win32.PInvoke.GetModuleHandleEx(
                                    GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
                                    new Windows.Win32.Foundation.PCWSTR((char*)funcPtr),
                                    (Windows.Win32.Foundation.HMODULE*)pHInstance);
                            }
                        }
                    }
                }

                return hInstance;

                [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
                static void STUB()
                { }
            }
        }

        public static string Location
        {
            get
            {
                if (string.IsNullOrEmpty(dllFilePath))
                {
                    lock (pathSeparatorChars)
                    {
                        if (string.IsNullOrEmpty(dllFilePath))
                        {
                            dllFilePath = GetModuleFileName(HINSTANCE);
                        }
                    }
                }
                return dllFilePath;
            }
        }

        public static string BaseDirectory => GetDirectory(Location);

        private static unsafe string GetModuleFileName(nint hInstance)
        {
            var buffer = new char[65536];
            fixed (char* pBuffer = buffer)
            {
                var length = Windows.Win32.PInvoke.GetModuleFileName((Windows.Win32.Foundation.HMODULE)hInstance, pBuffer, 65536);
                return new string(pBuffer, 0, (int)length);
            }
        }

        private static string GetDirectory(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;
            filePath = filePath.Trim();

            if (pathSeparatorChars.Contains(filePath[^1])) return filePath;

            var idx = filePath.LastIndexOfAny(pathSeparatorChars);
            if (idx == -1) return string.Empty;

            idx++;
            return filePath[..idx];
        }
    }

    public class ApplicationProperties
    {
        private static object locker = new object();
        private static ApplicationProperties? current;

        private ApplicationProperties(string applicationUserModelId, string relativeApplicationId)
        {
            ApplicationUserModelId = applicationUserModelId;
            RelativeApplicationId = relativeApplicationId;
        }

        public string ApplicationUserModelId { get; }

        public string RelativeApplicationId { get; }

        public override string ToString()
        {
            return $"ApplicationUserModelId: {ApplicationUserModelId}, RelativeApplicationId: {RelativeApplicationId}";
        }

        public static ApplicationProperties? Current
        {
            get
            {
                if (current == null && PackageProperties.Current != null)
                {
                    lock (locker)
                    {
                        if (current == null)
                        {
                            current = CreateApplicationProperties();
                        }
                    }
                }

                return current;
            }
        }

        private unsafe static ApplicationProperties? CreateApplicationProperties()
        {
            const uint APPLICATION_USER_MODEL_ID_MAX_LENGTH = 130;

            uint length = 0;
            var err = Windows.Win32.PInvoke.GetCurrentApplicationUserModelId(&length, default);
            if (err == Windows.Win32.Foundation.WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER
                && length > 0
                && length <= APPLICATION_USER_MODEL_ID_MAX_LENGTH)
            {
                var buffer = stackalloc char[(int)length];
                err = Windows.Win32.PInvoke.GetCurrentApplicationUserModelId(&length, buffer);
                if (length > 0 && err == Windows.Win32.Foundation.WIN32_ERROR.ERROR_SUCCESS)
                {
                    var aumid = new string(buffer, 0, (int)length - 1);
                    string relativeApplicationId = "";

                    var pfn = PackageProperties.Current!.PackageFamilyName;

                    if (!string.IsNullOrEmpty(pfn)
                        && length - pfn.Length - 2 > 0)
                    {
                        relativeApplicationId = new string(
                            buffer,
                            pfn.Length + 1,
                            (int)length - pfn.Length - 2);
                    }

                    return new ApplicationProperties(aumid, relativeApplicationId);
                }
            }

            return null;
        }
    }

    public class PackageProperties
    {
        private static bool isUnpackagedApp;
        private static object locker = new object();
        private static PackageProperties? current;

        private PackageProperties(
            string packageInstallLocation,
            string packageFullName,
            string packageFamilyName,
            PackageId packageId)
        {
            PackageInstallLocation = packageInstallLocation;
            PackageFullName = packageFullName;
            PackageFamilyName = packageFamilyName;
            PackageId = packageId;
        }

        public string PackageInstallLocation { get; }

        public string PackageFullName { get; }

        public string PackageFamilyName { get; }

        public PackageId PackageId { get; }

        public override string ToString()
        {
            return $"PackageInstallLocation: {PackageInstallLocation}, PackageFullName: {PackageFullName}, PackageFamilyName: {PackageFamilyName}, PackageId: {PackageId}";
        }

        public static PackageProperties? Current
        {
            get
            {
                if (isUnpackagedApp) return null;
                if (current == null)
                {
                    lock (locker)
                    {
                        if (current == null)
                        {
                            if (Environment.OSVersion.Version >= new Version(8, 0, 0, 0))
                            {
                                current = CreatePackageProperties();
                            }
                            else
                            {
                                isUnpackagedApp = true;
                            }
                        }
                    }
                }
                return current;
            }
        }

        private unsafe static PackageProperties? CreatePackageProperties()
        {
            uint size, count;

#pragma warning disable CA1416
            var err = Windows.Win32.PInvoke.GetCurrentPackageInfo(0x00000010, &size, null, &count);
#pragma warning restore CA1416

            if (count > 0)
            {
                var buffer = new byte[size];

                fixed (byte* pBuffer = buffer)
                {
#pragma warning disable CA1416
                    err = Windows.Win32.PInvoke.GetCurrentPackageInfo(0x00000010, &size, pBuffer, &count);
#pragma warning restore CA1416

                    var span = new Span<PACKAGE_INFO>(pBuffer, (int)count);

                    var version = span[0].packageId.version.Anonymous.Anonymous;

                    return new PackageProperties(
                        span[0].path.ToString(),
                        span[0].packageFullName.ToString(),
                        span[0].packageFamilyName.ToString(),
                    new PackageId(
                            (PackageId.ProcessorArchitecture)span[0].packageId.processorArchitecture,
                            new Version(version.Major, version.Minor, version.Build, version.Revision),
                            span[0].packageId.name.ToString(),
                            span[0].packageId.publisher.ToString(),
                            span[0].packageId.resourceId.ToString(),
                            span[0].packageId.publisherId.ToString()));
                }
            }
            else
            {
                isUnpackagedApp = true;
            }
            return null;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private partial struct PACKAGE_ID
        {
            internal uint reserved;
            internal uint processorArchitecture;
            internal Windows.Win32.Storage.Packaging.Appx.PACKAGE_VERSION version;
            internal Windows.Win32.Foundation.PWSTR name;
            internal Windows.Win32.Foundation.PWSTR publisher;
            internal Windows.Win32.Foundation.PWSTR resourceId;
            internal Windows.Win32.Foundation.PWSTR publisherId;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private partial struct PACKAGE_INFO
        {
            internal uint reserved;
            internal uint flags;
            internal Windows.Win32.Foundation.PWSTR path;
            internal Windows.Win32.Foundation.PWSTR packageFullName;
            internal Windows.Win32.Foundation.PWSTR packageFamilyName;
            internal PACKAGE_ID packageId;
        }
    }

    public record PackageId(
        PackageId.ProcessorArchitecture Architecture,
        Version Version,
        string Name,
        string Publisher,
        string? ResourceId,
        string PublisherId)
    {
        public enum ProcessorArchitecture : uint
        {
            /// <summary>
            /// The ARM processor architecture.
            /// </summary>
            Arm = 5,

            /// <summary>
            /// The Arm64 processor architecture.
            /// </summary>
            Arm64 = 12,

            /// <summary>
            /// A neutral processor architecture.
            /// </summary>
            Neutral = 11,

            /// <summary>
            /// An unknown processor architecture.
            /// </summary>
            Unknown = 65535,

            /// <summary>
            /// The x64 processor architecture.
            /// </summary>
            X64 = 9,

            /// <summary>
            /// The x86 processor architecture.
            /// </summary>
            X86 = 0,

            /// <summary>
            /// The Arm64 processor architecture emulating the X86 architecture.
            /// </summary>
            X86OnArm64 = 14,
        }
    }
}
