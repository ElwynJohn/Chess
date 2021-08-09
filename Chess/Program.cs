using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics;
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.ReactiveUI;

using Chess.Models;

namespace Chess
{
    public static class Extensions
    {
        public static byte[] ToByteArray<T>(this T obj)
        {
            if (obj == null)
                return new byte[0];

            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr<T>(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public static T? ConvertTo<T>(this byte[] data)
        {
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, ptr, size);
            T? result = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return result;
        }

        public static UInt32 ToUInt32(this byte[] data)
        {
            return BitConverter.ToUInt32(data);
        }

        public static Int32 ToInt32(this byte[] data)
        {
            return BitConverter.ToInt32(data);
        }
    }

    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        private static Process? engine;
        private static Avalonia.Logging.LogEventLevel avaloniaLogLevel;
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
            avaloniaLogLevel = Avalonia.Logging.LogEventLevel.Verbose;
#else
            Logger.DebugLevelT = DebugLevel.Info;
            avaloniaLogLevel = Avalonia.Logging.LogEventLevel.Information;
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
                Logger.WWrite("Removing Console.Error from Logger's writers.");
                Logger.RemoveWriter(Console.Error);
                Logger.EWrite(e, "Unhandled exception.");
            };

            string? exe_dir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (exe_dir != null)
            {
                Directory.SetCurrentDirectory(exe_dir);
                Logger.IWrite(exe_dir);
            }
            string engine_path = Path.Join(exe_dir, "ChessEngineMain");
            Logger.IWrite(engine_path);
            engine = Process.Start(new ProcessStartInfo {
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = engine_path,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    });

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);

            Trace.Listeners.Add(new TextWriterTraceListener("chess_trace.log"));

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(avaloniaLogLevel)
                .UseReactiveUI();

        static void OnExit(object? sender, EventArgs e)
        {
            bool? did_close = engine?.CloseMainWindow();
            if (did_close != null && !(bool)did_close)
                engine?.Kill();
        }
    }
}
