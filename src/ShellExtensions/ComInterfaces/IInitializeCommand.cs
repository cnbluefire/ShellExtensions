using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IInitializeCommand)]
    internal partial interface IInitializeCommand
    {
        [PreserveSig]
        unsafe int Initialize([MarshalAs(UnmanagedType.LPWStr)] string pszCommandName, Windows.Win32.System.Com.StructuredStorage.IPropertyBag* ppb);
    }
}
