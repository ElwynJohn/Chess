using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics;

namespace Chess
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener("chess_debug.log"));
            Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Error));
            Trace.AutoFlush = true;
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
