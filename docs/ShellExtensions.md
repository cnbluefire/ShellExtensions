# ShellExtensions

提供 IExplorerCommand / IThumbnailProvider / IPreviewHandler 的 .net 封装

```ps1
dotnet add package ShellExtensions
```

### 使用 .net native 发布实现 IExplorerCommand 和 IThumbnailProvider 的 dll

#### 使用 ShellExtensions.ExplorerCommand 实现 IExplorerCommand

```cs

public partial class ExplorerCommandSample : ShellExtensions.ExplorerCommand
{
    public override string? GetTitle(ShellItemArray shellItems)
    {
        return "Sample 1. Shell Extension dll";
    }
}

```

#### 使用 ShellExtensions.ThumbnailProvider 实现 IThumbnailProvider

```cs

public class ThumbnailProviderSample : ShellExtensions.ThumbnailProvider
{
    public override void GetThumbnail(Stream stream, Size size, out nint hBitmap, out ThumbnailProviderPixelFormat pixelFormat)
    {
        using (var bitmap = new System.Drawing.Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        {
            using (var g = Graphics.FromImage(bitmap))
            {
                g.FillEllipse(Brushes.Red, new Rectangle(default, size));
            }
            hBitmap = bitmap.GetHbitmap();
            pixelFormat = ThumbnailProviderPixelFormat.Format32bppArgb;
        }
    }
}

```

#### 注册 COM 对象和编写导出函数

```cs
public static class DllMain
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Init()
    {
        ShellExtensions.ShellExtensionsClassFactory.RegisterInProcess<ExplorerCommandSample>(
            new Guid("your explorer command guid"));

        ShellExtensions.ShellExtensionsClassFactory.RegisterInProcess<ThumbnailProviderSample>(
            new Guid("your thumbnail provider guid"));
    }

    [UnmanagedCallersOnly(EntryPoint = "DllCanUnloadNow")]
    private static int DllCanUnloadNow() => ShellExtensions.ShellExtensionsClassFactory.DllCanUnloadNow();

    [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
    private unsafe static int DllGetClassObject(Guid* clsid, Guid* riid, void** ppv) => ShellExtensions.ShellExtensionsClassFactory.DllGetClassObject(clsid, riid, ppv);
}
```

#### 配置项目
```xml
<PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>

<PropertyGroup Condition="'$(Platform)' != 'AnyCPU'">
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
    <StaticallyLinked>true</StaticallyLinked>
    <IlcGenerateMstatFile>true</IlcGenerateMstatFile>
    <IlcGenerateDgmlFile>true</IlcGenerateDgmlFile>
</PropertyGroup>
```

#### 发布 dll
```ps1
dotnet publish -c Release -r win-x86 -o .\bin\output\win-x86
dotnet publish -c Release -r win-x64 -o .\bin\output\win-x64
dotnet publish -c Release -r win-arm64 -o .\bin\output\win-arm64
```


### 使用 WinForms 实现 IPreviewHandler

```cs
static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        var s = nint.Size;
        var a = ShellExtensions.PackageProperties.Current;

        ApplicationConfiguration.Initialize();

        ShellExtensions.ShellExtensionsClassFactory.RegisterLocalServer<HelloPreviewHandler>(
            new Guid("your preview handler guid"),
            ShellExtensions.ShellExtensionsClassFactory.REGCLS.REGCLS_SINGLEUSE);

        Application.Idle += Application_Idle;
        Application.Run();

        static void Application_Idle(object? sender, EventArgs e)
        {
            Application.Idle -= Application_Idle;
            Dispatcher = new DispatcherImpl();
            _ = Dispatcher.Handle;
        }
    }

    internal static DispatcherImpl Dispatcher { get; private set; } = null!;

    internal class DispatcherImpl : Control { }
}
```


```cs
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

```