using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics;
using System;
using System.IO;
using Pastel;
using Chess.Models;

namespace Chess
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            var sw = new StreamWriter("chess_test.log");
            var swColour = new StreamWriter("chess_test_col.log");
            sw.AutoFlush = true;
            swColour.AutoFlush = true;
            Logger.AddWriter(sw, false);
            Logger.AddWriter(swColour, true);
            Logger.AddWriter(System.Console.Error, true);
            Logger.DebugLevelT = DebugLevel.All;
            Logger.DWrite("hello");
            Logger.IWrite("hello");
            Logger.EWrite("hello");
            Logger.WWrite("hello");

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
