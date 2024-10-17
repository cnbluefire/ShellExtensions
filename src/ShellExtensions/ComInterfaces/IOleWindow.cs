using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IOleWindow)]
    internal partial interface IOleWindow
    {
        [PreserveSig]
        unsafe int GetWindow(nint* phwnd);

        [PreserveSig]
        int ContextSensitiveHelp([MarshalAs(UnmanagedType.Bool)] bool fEnterMode);
    }
}
