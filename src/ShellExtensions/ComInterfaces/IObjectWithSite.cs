using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IObjectWithSite)]
    internal partial interface IObjectWithSite
    {
        [PreserveSig]
        unsafe int SetSite(Windows.Win32.System.Com.IUnknown* pUnkSite);

        [PreserveSig]
        unsafe int GetSite(global::System.Guid* riid, void** ppvSite);
    }
}
