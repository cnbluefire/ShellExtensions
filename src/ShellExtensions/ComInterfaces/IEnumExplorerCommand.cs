using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IEnumExplorerCommand)]
    internal partial interface IEnumExplorerCommand
    {
        [PreserveSig]
        unsafe int Next(uint celt, void** pUICommand, uint* pceltFetched);

        [PreserveSig]
        int Skip(uint celt);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        unsafe int Clone(out IEnumExplorerCommand? ppenum);
    }
}
