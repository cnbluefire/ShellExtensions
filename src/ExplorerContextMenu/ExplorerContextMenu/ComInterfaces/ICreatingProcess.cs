using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerContextMenu.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_ICreatingProcess)]
    internal partial interface ICreatingProcess
    {
        [PreserveSig]
        unsafe int OnCreating(void* pcpi);        
    }
}
