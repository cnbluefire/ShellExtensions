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
    [Guid(ComInterfaceIds.IID_IServiceProvider)]
    internal partial interface IServiceProvider
    {
        [PreserveSig]
        unsafe int QueryService(Guid* guidService, Guid* riid, void** ppvObject);
    }
}
