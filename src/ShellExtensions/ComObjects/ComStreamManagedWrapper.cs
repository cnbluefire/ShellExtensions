using Windows.Win32.System.Com;

namespace ShellExtensions.ComObjects
{
    internal unsafe class ComStreamManagedWrapper : Stream
    {
        private bool isDisposed;
        private IStream* pStream;
        private STGM grfMode;
        private bool canSeek;

        internal ComStreamManagedWrapper(IStream* pStream, uint grfMode)
        {
            fixed (Guid* riid_IStream = &IStream.IID_Guid)
            fixed (IStream** ppStream = &this.pStream)
            {
                pStream->QueryInterface(riid_IStream, (void**)ppStream).ThrowOnFailure();
            }
            this.grfMode = (STGM)grfMode;

            try
            {
                pStream->Seek(0, SeekOrigin.Begin, null);
                canSeek = true;
            }
            catch
            {
                canSeek = false;
            }
        }

        public override bool CanRead => (grfMode & STGM.STGM_READ) == STGM.STGM_READ;

        public override bool CanSeek => canSeek;

        public override bool CanWrite => (grfMode & STGM.STGM_WRITE) == STGM.STGM_WRITE;

        public override long Length
        {
            get
            {
                pStream->Stat(out var statstg, 1);
                return (long)statstg.cbSize;
            }
        }

        public override long Position
        {
            get
            {
                ulong pos = 0;
                pStream->Seek(0, SeekOrigin.Current, &pos);
                return (long)pos;
            }
            set => pStream->Seek(value, SeekOrigin.Begin, null);
        }

        public override void Flush()
        {
            pStream->Commit(0);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            fixed (byte* pBuf = buffer)
            {
                var count2 = Math.Min(buffer.Length - offset, count);
                uint count3 = 0;

                var hr = pStream->Read(pBuf, (uint)count2, &count3);
                if (hr.Succeeded) return (int)count3;
            }
            return 0;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            fixed (byte* pBuf = buffer)
            {
                var count2 = Math.Min(buffer.Length - offset, count);
                uint count3 = 0;

                pStream->Write(pBuf, (uint)count2, &count3).ThrowOnFailure();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ulong pos = 0;
            pStream->Seek(offset, origin, &pos);
            return (long)pos;
        }

        public override void SetLength(long value)
        {
            pStream->SetSize((ulong)value);
        }

        protected override void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (pStream != null)
                {
                    pStream->Release();
                    pStream = null;
                }
                isDisposed = true;
            }

            base.Dispose(disposing);
        }

        ~ComStreamManagedWrapper()
        {
            Dispose(false);
        }
    }
}
