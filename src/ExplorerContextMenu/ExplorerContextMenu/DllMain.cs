using ShellExtensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Windows.Win32.UI.Shell;

namespace ExplorerContextMenu;

public static class DllMain
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Init()
    {
        cachedItems = new ConcurrentDictionary<Guid, nint>();
        var rootModel = LoadResource();
        if (rootModel != null)
        {
            var factory = new ModelFactory(rootModel);

            foreach (var key in factory.GetKeys())
            {
                var item = factory.GetItem(key);
                if (item != null)
                {
                    ShellExtensions.ShellExtensionsClassFactory.RegisterInProcess(key, () => new ModelBasedCommand(factory, item));
                }
            }

            foreach (var key in factory.SeparatorGuids)
            {
                ShellExtensions.ShellExtensionsClassFactory.RegisterInProcess(key, () => new SeparatorCommand(key));
            }
        }
    }

    private static ConcurrentDictionary<Guid, nint>? cachedItems;

    [UnmanagedCallersOnly(EntryPoint = "DllCanUnloadNow")]
    private static int DllCanUnloadNow() => ShellExtensions.ShellExtensionsClassFactory.DllCanUnloadNow();

    [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
    private unsafe static int DllGetClassObject(Guid* clsid, Guid* riid, void** ppv)
    {
        if (cachedItems != null && cachedItems.TryGetValue(*clsid, out var pv))
        {
            *ppv = (void*)pv;
            return ShellExtensions.ComInterfaces.HResults.S_OK;
        }
        var hr = ShellExtensions.ShellExtensionsClassFactory.DllGetClassObject(clsid, riid, ppv);
        if (hr == ShellExtensions.ComInterfaces.HResults.S_OK && cachedItems != null)
        {
            cachedItems.TryAdd(*clsid, (nint)(*ppv));
        }
        return hr;
    }


    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "Test")]
    private static void Test()
    {
        LoadResource();
    }

    public unsafe static Models.ExplorerContextMenuModel? LoadResource()
    {
        char* ptr = null;

        var length = Windows.Win32.PInvoke.LoadString(
            (Windows.Win32.Foundation.HMODULE)ShellExtensions.DllModule.HINSTANCE,
            0, (char*)&ptr, 0);

        if (length > 0)
        {
            var json = new string(ptr, 0, length);
            Console.WriteLine(json);

            try
            {
                return JsonSerializer.Deserialize(json, JsonModelContext.Default.ExplorerContextMenuModel);
            }
            catch { }
        }
        else
        {
            Console.WriteLine(Marshal.GetLastWin32Error());
        }

        return null;
    }
}
