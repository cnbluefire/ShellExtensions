using ShellExtensions;
using ShellExtensions.Helpers;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ShellExtDll;

public static class DllMain
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Init()
    {
        ShellExtensions.ShellExtensionsClassFactory.RegisterInProcess<ExplorerCommandSample>(
            new Guid("A7234747-7C8E-4FF6-B284-9E6A014957B7"));

        ShellExtensions.ShellExtensionsClassFactory.RegisterInProcess<ThumbnailProviderSample>(
            new Guid("0A639F6F-58EB-4427-BC59-8F69787C085C"));
    }

    [UnmanagedCallersOnly(EntryPoint = "DllCanUnloadNow")]
    private static int DllCanUnloadNow() => ShellExtensions.ShellExtensionsClassFactory.DllCanUnloadNow();

    [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
    private unsafe static int DllGetClassObject(Guid* clsid, Guid* riid, void** ppv) => ShellExtensions.ShellExtensionsClassFactory.DllGetClassObject(clsid, riid, ppv);
}

public partial class ExplorerCommandSample : ShellExtensions.ExplorerCommand
{
    public override string? GetTitle(ShellItemArray shellItems)
    {
        return "Sample 1. Shell Extension dll";
    }

    public override void Invoke(ExplorerCommandInvokeEventArgs args)
    {
        var sb = new StringBuilder();

        var dllPath = ShellExtensions.DllModule.Location;

        var contextMenuTypeAccessor = this.ServiceProvider.GetService<IContextMenuTypeAccessor>();
        if (contextMenuTypeAccessor != null)
        {
            sb.AppendLine(contextMenuTypeAccessor.ContextMenuType.ToString());
        }

        if (PackageProperties.Current != null)
        {
            sb.AppendLine(PackageProperties.Current.ToString());
        }

        if (ApplicationProperties.Current != null)
        {
            sb.AppendLine(ApplicationProperties.Current.ToString());
        }

        if (!string.IsNullOrEmpty(dllPath))
        {
            sb.AppendLine(dllPath);
        }

        if (args.Folder != null)
        {
            sb.AppendLine(args.Folder.FullPath);
        }

        if (args.ShellItems != null)
        {
            foreach (var item in args.ShellItems)
            {
                sb.AppendLine(item.FullPath);
            }
        }

        MessageBoxW(default, sb.ToString(), "ContextMenu", 0);
    }

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int MessageBoxW(nint hWnd, string lpText, string lpCaption, uint uType);
}

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
