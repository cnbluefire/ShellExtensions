using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions.ComInterfaces
{
    [GeneratedComInterface]
    [Guid(ComInterfaceIds.IID_IThumbnailProvider)]
    internal partial interface IThumbnailProvider
    {
        [PreserveSig]
        unsafe int GetThumbnail(uint cx, nint* phbmp, int* pdwAlpha);
    }
}
