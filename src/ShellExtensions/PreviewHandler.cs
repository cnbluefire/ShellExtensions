using ShellExtensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellExtensions
{
    public abstract class PreviewHandler : IShellExtensions
    {
        private nint parenthWnd;

        internal nint ParentInternal { get => parenthWnd; set => parenthWnd = value; }

        public virtual nint Parent { get => ParentInternal; }

        public abstract nint WindowHandle { get; }

        public abstract void OnParentChanged(nint newParent, System.Drawing.Rectangle newRect);

        public abstract void OnRectChanged(System.Drawing.Rectangle newRect);

        public virtual bool TranslateAccelerator(ref MSG msg)
        {
            return false;
        }

        public abstract nint QueryFocus();

        public abstract void SetFocus();

        public abstract void DoPreview();

        public abstract void Unload();

        public abstract bool Initialize(Stream stream, FileAccess fileAccess);

        public abstract bool Initialize(string filePath, FileAccess fileAccess);

        public virtual void OnBackgroundChanged(System.Drawing.Color color) { }

        public virtual void OnForegroundChanged(System.Drawing.Color color) { }

        public struct MSG
        {
            public nint hwnd;

            public uint message;

            public nuint wParam;

            public nint lParam;

            public uint time;

            public global::System.Drawing.Point pt;
        }
    }
}
