using ShellExtensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions
{
    public abstract class ThumbnailProvider : IShellExtensions
    {
        public abstract void GetThumbnail(Stream stream, System.Drawing.Size size, out nint hBitmap, out ThumbnailProviderPixelFormat pixelFormat);
    }

    public enum ThumbnailProviderPixelFormat
    {
        Unknown,
        Format32bppRgb,
        Format32bppArgb
    }
}
