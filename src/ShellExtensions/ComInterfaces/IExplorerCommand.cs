using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IExplorerCommand)]
    internal partial interface IExplorerCommand
    {
        [PreserveSig]
        unsafe int GetTitle(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, Windows.Win32.Foundation.PWSTR* ppszName);

        [PreserveSig]
        unsafe int GetIcon(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, Windows.Win32.Foundation.PWSTR* ppszIcon);

        [PreserveSig]
        unsafe int GetToolTip(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, Windows.Win32.Foundation.PWSTR* ppszInfotip);

        [PreserveSig]
        unsafe int GetCanonicalName(global::System.Guid* pguidCommandName);

        [PreserveSig]
        unsafe int GetState(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, [MarshalAs(UnmanagedType.Bool)] bool fOkToBeSlow, uint* pCmdState);

        [PreserveSig]
        unsafe int Invoke(Windows.Win32.UI.Shell.IShellItemArray* psiItemArray, Windows.Win32.System.Com.IBindCtx* pbc);

        [PreserveSig]
        unsafe int GetFlags(uint* pFlags);

        [PreserveSig]
        unsafe int EnumSubCommands(out IEnumExplorerCommand? ppEnum);
    }
}
