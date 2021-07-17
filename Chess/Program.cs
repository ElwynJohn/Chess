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
#if DEBUG
            Logger.DebugLevelT = DebugLevel.All;
#else
            Logger.DebugLevelT = DebugLevel.Info;
#endif
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs args) =>
            {
                Exception e = (Exception)args.ExceptionObject;
                // We remove Console.Error because this stops the exception message
                // being printed twics: once by Logger and once by the system.
                // @@FIXME: Can we stop the CLR from printing the exception message and
                // print it using our own logger? Also, does this exception message
                // get printed in the same way on other platforms?
                Logger.RemoveWriter(Console.Error);
                Logger.ExceptionWrite(e, "Unhandled exception.");
            };

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
