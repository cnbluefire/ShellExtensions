using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IInitializeWithFile)]
    internal partial interface IInitializeWithFile
    {
        [PreserveSig]
        unsafe int Initialize([MarshalAs(UnmanagedType.LPWStr)] string pszCommandName, uint grfMode);
    }
}
