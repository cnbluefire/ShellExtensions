using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IExplorerCommandState)]
    internal partial interface IExplorerCommandState
    {
        [PreserveSig]
        unsafe int GetState(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, [MarshalAs(UnmanagedType.Bool)] bool fOkToBeSlow, uint* pCmdState);
    }
}
