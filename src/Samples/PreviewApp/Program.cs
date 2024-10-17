
namespace PreviewApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        ShellExtensions.ShellExtensionsClassFactory.RegisterLocalServer<HelloPreviewHandler>(
            new Guid("A3949267-36A0-4EB5-81B5-E468E566A59C"),
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
