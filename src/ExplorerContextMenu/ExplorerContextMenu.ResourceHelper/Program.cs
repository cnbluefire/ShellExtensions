
using System.CommandLine;
using System.Runtime.InteropServices;

namespace ExplorerContextMenu.ResourceHelper;

internal static class Program
{
    internal static async Task Main(string[] args)
    {
#if DEBUG
        if (args.Length == 0)
        {
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

        var handle = ResourceWriter.BeginUpdateResource(outputFile.FullName, true);
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
            ResourceWriter.EndUpdateResource(handle, true);
            return;
        }
        var res = ResourceWriter.EndUpdateResource(handle, false);

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
