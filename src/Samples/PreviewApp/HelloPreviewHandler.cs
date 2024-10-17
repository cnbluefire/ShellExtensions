using System.Runtime.InteropServices;

namespace PreviewApp;

internal class HelloPreviewHandler : ShellExtensions.PreviewHandler
{
    private Form1? form1;
    private string? filePath;
    private nint parent;
    private Rectangle rect;

    public override nint WindowHandle => form1?.Handle ?? IntPtr.Zero;

    public override void DoPreview() => CreateWindow(true);

    public override bool Initialize(Stream stream, FileAccess fileAccess) => false;

    public override bool Initialize(string filePath, FileAccess fileAccess)
    {
        this.filePath = filePath;
        form1?.SetFilePath(filePath);
        return true;
    }

    public override void OnParentChanged(nint newParent, Rectangle newRect)
    {
        rect = newRect;
        parent = newParent;

        CreateWindow();
    }

    public override void OnRectChanged(Rectangle newRect)
    {
        rect = newRect;

        CreateWindow();
    }

    public override nint QueryFocus() => 0;

    public override void SetFocus() { }

    public override void Unload()
    {
        parent = 0;
        rect = default;

        if (form1 != null && form1.IsHandleCreated)
        {
            form1.Invoke(() =>
            {
                var handle = form1.Handle;
                form1.Hide();
                SetParent(handle, 0);
            });
        }
    }

    private void CreateWindow(bool showWindow = false)
    {
        if (parent != 0 && rect.Width > 0 && rect.Height > 0)
        {
            Program.Dispatcher.Invoke(() =>
            {
                var _parent = parent;
                var _rect = rect;
                if (_parent != 0 && _rect.Width > 0 && _rect.Height > 0)
                {
                    if (form1 == null) form1 = new Form1()
                    {
                        FormBorderStyle = FormBorderStyle.None
                    };

                    form1.SetFilePath(filePath);

                    form1.SetBounds(_rect.X, _rect.Y, _rect.Width, _rect.Height);

                    // get parent window
                    if (GetAncestor(form1.Handle, 1) != _parent)
                    {
                        SetParent(form1.Handle, _parent);
                    }

                    if (showWindow)
                    {
                        ShowNoActive(form1);
                    }
                }
            });
        }
    }

    private static void ShowNoActive(Form form)
    {
        const int SW_SHOWNOACTIVATE = 4;

        ShowWindow(form.Handle, SW_SHOWNOACTIVATE);
    }

    [DllImport("user32.dll")]
    private static extern int ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern nint SetParent(nint hWndChild, nint hWndNewParent);

    [DllImport("user32.dll")]
    private static extern nint GetAncestor(nint hWnd, uint gaFlags);

}
