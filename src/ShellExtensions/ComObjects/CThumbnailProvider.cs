using ShellExtensions.ComInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;

namespace ShellExtensions.ComObjects
{
    [GeneratedComClass]
    internal unsafe partial class CThumbnailProvider : IThumbnailProvider, IInitializeWithStream
    {
        private readonly ThumbnailProvider impl;
        private ComStreamManagedWrapper? stream;

        public CThumbnailProvider(ThumbnailProvider impl)
        {
            this.impl = impl;
        }

        public unsafe int GetThumbnail(uint cx, nint* phbmp, int* pdwAlpha)
        {
            var stream = this.stream;
            if (stream != null)
            {
                try
                {
                    impl.GetThumbnail(stream, new System.Drawing.Size((int)cx, (int)cx), out var hBitmap, out var pixelFormat);

                    if (hBitmap != 0)
                    {
                        switch (pixelFormat)
                        {
                            case ThumbnailProviderPixelFormat.Unknown:
                                *pdwAlpha = 0;
                                break;
                            
                            case ThumbnailProviderPixelFormat.Format32bppRgb:
                                *pdwAlpha = 1;
                                break;
                            
                            case ThumbnailProviderPixelFormat.Format32bppArgb:
                                *pdwAlpha = 2;
                                break;

                            default:
                                return HResults.E_UNEXPECTED;
                        }
                        *phbmp = hBitmap;

                        return HResults.S_OK;
                    }
                }
                catch (Exception ex)
                {
                    return ex.HResult;
                }
            }
            return HResults.E_UNEXPECTED;
        }

        public unsafe int Initialize(IStream* pstream, uint grfMode)
        {
            stream?.Dispose();
            stream = null;
            var hr = (HRESULT)HResults.E_UNEXPECTED;

            if (pstream != null)
            {

                try
                {
                    stream = new ComStreamManagedWrapper(pstream, grfMode);
                    hr = (HRESULT)HResults.S_OK;
                }
                catch (Exception ex)
                {
                    hr = (HRESULT)ex.HResult;
                }
            }

            return hr;
        }

        ~CThumbnailProvider()
        {
            stream?.Dispose();
            stream = null;
        }
    }
}
