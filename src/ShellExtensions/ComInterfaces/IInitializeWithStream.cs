using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IInitializeWithStream)]
    internal partial interface IInitializeWithStream
    {
        [PreserveSig]
        unsafe int Initialize(Windows.Win32.System.Com.IStream* pstream, uint grfMode);
    }
}
