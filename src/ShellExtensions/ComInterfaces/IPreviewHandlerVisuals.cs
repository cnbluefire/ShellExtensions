using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IPreviewHandlerVisuals)]
    internal partial interface IPreviewHandlerVisuals
    {
        [PreserveSig()]
        unsafe int SetBackgroundColor(uint color);

        [PreserveSig()]
        unsafe int SetFont(Windows.Win32.Graphics.Gdi.LOGFONTW* plf);

        [PreserveSig()]
        unsafe int SetTextColor(uint color);
    }
}
