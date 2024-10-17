using ShellExtensions.ComInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.System.Com;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ShellExtensions.ComObjects
{
    [GeneratedComClass]
    internal partial class CPreviewHandler : IObjectWithSite, IPreviewHandler, IOleWindow, IInitializeWithFile, IInitializeWithStream, IPreviewHandlerVisuals
    {
        private readonly PreviewHandler impl;
        private ObjectWithSiteHelper objectWithSiteHelper;

        public CPreviewHandler(PreviewHandler impl)
        {
            this.impl = impl;
            this.objectWithSiteHelper = new ObjectWithSiteHelper();
        }


        #region IOleWindow

        public unsafe int GetWindow(nint* phwnd)
        {
            var hr = HResults.E_INVALIDARG;
            try
            {
                if (phwnd != null)
                {
                    *phwnd = impl.WindowHandle;

                    if (phwnd->Equals(0))
                    {
                        hr = HResults.E_FAIL;
                    }
                    hr = HResults.S_OK;
                }
            }
            catch (Exception ex)
            {
                hr = ex.HResult;
            }

            return hr;
        }

        public int ContextSensitiveHelp([MarshalAs(UnmanagedType.Bool)] bool fEnterMode)
        {
            return HResults.E_NOTIMPL;
        }

        #endregion IOleWindow



        #region IObjectWithSite

        public unsafe int GetSite(Guid* riid, void** ppvSite)
        {
            return objectWithSiteHelper.GetSite(riid, ppvSite);
        }

        public unsafe int SetSite(IUnknown* pUnkSite)
        {
            return objectWithSiteHelper.SetSite(pUnkSite);
        }

        #endregion IObjectWithSite



        #region IInitializeWithFile


        public int Initialize([MarshalAs(UnmanagedType.LPWStr)] string pszCommandName, uint grfMode)
        {
            var hr = (HRESULT)HResults.E_UNEXPECTED;

            if (!string.IsNullOrEmpty(pszCommandName))
            {
                try
                {
                    var access = (grfMode & 2) != 0 ? System.IO.FileAccess.ReadWrite : System.IO.FileAccess.Read;

                    if (impl.Initialize(pszCommandName, access))
                    {
                        hr = (HRESULT)HResults.S_OK;
                    }
                    else
                    {
                        hr = (HRESULT)HResults.E_NOTIMPL;
                    }
                }
                catch (Exception ex)
                {
                    hr = (HRESULT)ex.HResult;
                }
            }

            return hr;
        }


        #endregion IInitializeWithFile


        #region IInitializeWithStream

        public unsafe int Initialize(IStream* pstream, uint grfMode)
        {
            var hr = (HRESULT)HResults.E_UNEXPECTED;

            if (pstream != null)
            {
                ComStreamManagedWrapper? stream = null;
                try
                {
                    stream = new ComStreamManagedWrapper(pstream, grfMode);

                    var access = (grfMode & 2) != 0 ? System.IO.FileAccess.ReadWrite : System.IO.FileAccess.Read;

                    if (impl.Initialize(stream, access))
                    {
                        hr = (HRESULT)HResults.S_OK;
                    }
                    else
                    {
                        hr = (HRESULT)HResults.E_NOTIMPL;
                    }
                }
                catch (Exception ex)
                {
                    hr = (HRESULT)ex.HResult;
                }

                if (hr.Failed)
                {
                    stream?.Dispose();
                    stream = null;
                }
            }

            return hr;
        }

        #endregion IInitializeWithStream



        #region IPreviewHandler


        public int DoPreview()
        {
            try
            {
                impl.DoPreview();
                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        public unsafe int QueryFocus(nint* phwnd)
        {
            try
            {
                *phwnd = impl.QueryFocus();
                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        public int SetFocus()
        {
            try
            {
                impl.SetFocus();
                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        public unsafe int SetRect(RECT* prc)
        {
            try
            {
                impl.OnRectChanged(new System.Drawing.Rectangle(prc->left, prc->top, prc->Width, prc->Height));
                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        public unsafe int SetWindow(nint hwnd, RECT* prc)
        {
            try
            {
                impl.ParentInternal = hwnd;
                impl.OnParentChanged(hwnd, new System.Drawing.Rectangle(prc->left, prc->top, prc->Width, prc->Height));
                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        public unsafe int TranslateAccelerator(MSG* pmsg)
        {
            try
            {
                ref var msg = ref Unsafe.AsRef<global::ShellExtensions.PreviewHandler.MSG>(pmsg);
                var result = impl.TranslateAccelerator(ref msg);
                return result ? HResults.S_OK : HResults.S_FALSE;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        public int Unload()
        {
            try
            {
                impl.Unload();
                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        #endregion IPreviewHandler



        #region IPreviewHandlerVisuals

        public int SetBackgroundColor(uint color)
        {
            try
            {
                var r = unchecked((byte)color);
                var g = unchecked((byte)(color >> 8));
                var b = unchecked((byte)(color >> 16));

                impl.OnBackgroundChanged(System.Drawing.Color.FromArgb(255, r, g, b));

                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        public unsafe int SetFont(LOGFONTW* plf)
        {
            return HResults.S_OK;
        }

        public int SetTextColor(uint color)
        {
            try
            {
                var r = unchecked((byte)color);
                var g = unchecked((byte)(color >> 8));
                var b = unchecked((byte)(color >> 16));

                impl.OnForegroundChanged(System.Drawing.Color.FromArgb(255, r, g, b));

                return HResults.S_OK;
            }
            catch (Exception ex)
            {
                return ex.HResult;
            }
        }

        #endregion IPreviewHandlerVisuals

    }
}
