﻿
using System.CommandLine;
using System.Runtime.InteropServices;

namespace ExplorerContextMenu.ResourceHelper;

public static class Program
{
    public static async Task Main(string[] args)
    {
#if DEBUG
        if (args.Length == 0)
        {
            //args = [
            //    "--input", "D:\\blue-fire\\source\\repos\\ShellExtensions\\src\\Samples\\ShellExtensions.Package\\bin\\x64\\Debug\\AppX\\ConsoleApp\\ExplorerContextMenu.dll",
            //    "--resource-json","C:\\Users\\blue-fire\\Downloads\\1\\menu.json",
            //    "--output", "D:\\blue-fire\\source\\repos\\ShellExtensions\\src\\Samples\\ShellExtensions.Package\\bin\\x64\\Debug\\AppX\\ConsoleApp\\ExplorerContextMenu.2.dll",
            //    "--overwritten", "true"
            //];

            var cmd = """
                --input "C:\Users\blue-fire\source\repos\ShellExtensions\src\nuget\ExplorerContextMenu\build\..\tools\x86\ExplorerContextMenu.dll" --resource-json "C:\Users\blue-fire\source\repos\ShellExtensions\src\Samples\ConsoleApp\ExplorerContextMenu.json" --output "C:\Users\blue-fire\source\repos\ShellExtensions\src\Samples\ConsoleApp\obj\Debug\net8.0-windows8.0\ExplorerContextMenu\abc.dll" --overwritten"
                """;

            args = System.CommandLine.Parsing.CommandLineStringSplitter.Instance.Split(cmd).ToArray();
        }
#endif

        var inputOption = new Option<FileInfo?>(
            name: "--input",
            description: "Specify the dll file that requires resource embedding.");
        inputOption.AddAlias("-i");

        var resourceJsonOption = new Option<FileInfo?>(
            name: "--resource-json",
            description: "Specify the resource JSON file.");
        resourceJsonOption.AddAlias("-r");

        var outputOption = new Option<FileInfo?>(
            name: "--output",
            description: "Specify the output location for the DLL file after embedding resources.");
        outputOption.AddAlias("-o");

        var overwrittenOption = new Option<bool>(
            name: "--overwritten",
            getDefaultValue: () => false,
            description: "Overwrite the file if it already exists.");

        var rootCommand = new RootCommand("Embed context menu configuration resources into a dll file.")
        {
            inputOption,
            resourceJsonOption,
            outputOption,
            overwrittenOption,
        };

        rootCommand.SetHandler(Run,
            inputOption,
            resourceJsonOption,
            outputOption,
            overwrittenOption);

        await rootCommand.InvokeAsync(args);
    }

    private static void Run(FileInfo? inputFile, FileInfo? jsonFile, FileInfo? outputFile, bool overwritten)
    {
        if (inputFile == null || !inputFile.Exists)
        {
            Console.Error.WriteLine("The input file is invalid.");
            Environment.ExitCode = HResults.E_INVALIDARG;
            return;
        }

        if (jsonFile == null || !jsonFile.Exists)
        {
            Console.Error.WriteLine("The resource JSON file is invalid.");
            Environment.ExitCode = HResults.E_INVALIDARG;
            return;
        }

        if (outputFile == null || string.IsNullOrEmpty(outputFile.DirectoryName))
        {
            Console.Error.WriteLine("The output location is invalid.");
            Environment.ExitCode = HResults.E_INVALIDARG;
            return;
        }

        if (outputFile.Exists && !overwritten)
        {
            Console.Error.WriteLine("Output file already exists and overwriting is not allowed.");
            Environment.ExitCode = HResults.E_INVALIDARG;
            return;
        }

        try
        {
            if (!Directory.Exists(outputFile.DirectoryName))
            {
                Directory.CreateDirectory(outputFile.DirectoryName);
            }
            outputFile = inputFile.CopyTo(outputFile.FullName, true);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return;
        }

        var handle = NativeMethods.BeginUpdateResource(outputFile.FullName, true);
        if (handle == IntPtr.Zero)
        {
            HandleLastWin32Error();
            return;
        }

        try
        {
            ResourceWriter.WriteResource(handle, jsonFile.FullName);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            NativeMethods.EndUpdateResource(handle, true);
            return;
        }
        var res = NativeMethods.EndUpdateResource(handle, false);

        if (!res)
        {
            HandleLastWin32Error();
            return;
        }

    }

    private static void HandleException(Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        Environment.ExitCode = ex.HResult;
    }

    private static void HandleLastWin32Error()
    {
        var hr = Marshal.GetHRForLastWin32Error();
        var ex = Marshal.GetExceptionForHR(hr);
        HandleException(ex ?? Marshal.GetExceptionForHR(HResults.E_UNEXPECTED)!);
    }
}

internal static class HResults
{
    public const int E_INVALIDARG = unchecked((int)0x80070057);

    public const int E_UNEXPECTED = unchecked((int)0x8000FFFF);
}

internal unsafe static class NativeMethods
{
    public static bool UpdateResource(
        IntPtr hUpdate,
        ResourceName lpType,
        ResourceName lpName,
        ushort wLanguage,
        void* lpData,
        uint cb)
    {
        var lpTypeName = lpType.Name ?? "";
        var lpNameName = lpName.Name ?? "";

        fixed (char* pTypeName = lpTypeName)
        fixed (char* pNameName = lpNameName)
        {
            var pTypeName1 = pTypeName;
            var pNameName1 = pNameName;

            if (lpType.Name == null) pTypeName1 = (char*)lpType.Id;
            if (lpName.Name == null) pNameName1 = (char*)lpName.Id;

            return UpdateResource(hUpdate, pTypeName1, pNameName1, wLanguage, lpData, cb);
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true)]
    internal static extern IntPtr BeginUpdateResource(
        [MarshalAs(UnmanagedType.LPWStr)] string pFileName,
        bool bDeleteExistingResources);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true)]
    internal static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true)]
    private static extern bool UpdateResource(
        IntPtr hUpdate,
        char* lpType,
        char* lpName,
        ushort wLanguage,
        void* lpData,
        uint cb);


}

internal struct ResourceName
{
    public ResourceName(string name)
    {
        Id = 0;

        if (name.Length == 0) Name = "\0";
        else if (name[name.Length - 1] == '\0') Name = name;
        else Name = name + "\0";
    }

    public ResourceName(ushort id) { Name = null; Id = id; }

    public string? Name;

    public ushort Id;



    public static implicit operator ResourceName(string name) => new ResourceName(name);

    public static implicit operator ResourceName(ushort id) => new ResourceName(id);
}