using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IPreviewHandler)]
    internal partial interface IPreviewHandler
    {
        [PreserveSig]
        unsafe int SetWindow(nint hwnd, Windows.Win32.Foundation.RECT* prc);

        [PreserveSig]
        unsafe int SetRect(Windows.Win32.Foundation.RECT* prc);

        [PreserveSig]
        int DoPreview();

        [PreserveSig]
        int Unload();

        [PreserveSig]
        int SetFocus();

        [PreserveSig]
        unsafe int QueryFocus(nint* phwnd);

        [PreserveSig]
        unsafe int TranslateAccelerator(Windows.Win32.UI.WindowsAndMessaging.MSG* pmsg);
    }
}
