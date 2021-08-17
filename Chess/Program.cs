using System;
using System.IO;
using System.Threading.Tasks;
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

        public static byte[] ToByteArray(this Message mess)
        {
            var b = new byte[24];
            mess.Length.ToByteArray<int>().CopyTo(b, 0);
            ((int)mess.Type).ToByteArray<int>().CopyTo(b, sizeof(int));
            mess.Guid.ToByteArray().CopyTo(b, 2 * sizeof(int));

            return b;
        }

        public static byte[] Concat(this byte[] a, params byte[][] arrays)
        {
            int resultLen = a.Length;
            foreach (byte[] b in arrays)
                resultLen += b.Length;
            int currentOffset = 0;
            byte[] result = new byte[resultLen];
            a.CopyTo(result, currentOffset);
            currentOffset += a.Length;
            foreach (byte[] b in arrays)
            {
                b.CopyTo(result, currentOffset);
                currentOffset += b.Length;
            }

            return result;
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
        private static string traceLogFile = "chess_trace.log";

        public static async Task Main(string[] args)
        {
            // Before we do anything else, cd to the directory that the
            // executable file is in.
            string? exe_dir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            Directory.SetCurrentDirectory(exe_dir!);

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
            Logger.DebugLevelT = DebugLevel.All;
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

            string engine_path = Path.Join(exe_dir, "ChessEngineMain");
            engine = Process.Start(new ProcessStartInfo {
                    UseShellExecute = true,
                    CreateNoWindow = false,
#if DEBUG
                    WindowStyle = ProcessWindowStyle.Normal,
#else
                    WindowStyle = ProcessWindowStyle.Hidden,
#endif
                    FileName = engine_path,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    });

            Console.CancelKeyPress += OnExit;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);

            // Clear the trace file on startup. Otherwise it can get really big
            System.IO.File.WriteAllText(traceLogFile, String.Empty);
            Trace.Listeners.Add(new TextWriterTraceListener(traceLogFile));

            // If we haven't connected after 5 seconds, something is definitely
            // wrong and we should just throw TimeoutException
            int timeout = 5000;
            var wConnect = Message.client_w.ConnectAsync(timeout);
            var rConnect = Message.client_r.ConnectAsync(timeout);

            await wConnect;
            await rConnect;

            Message.replyThread.Start();
            Message.requestThread.Start();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(avaloniaLogLevel)
                .UseReactiveUI();

        public static void OnExit(object? sender, EventArgs e)
        {
            bool? did_close = engine?.CloseMainWindow();
            if (did_close != null && !(bool)did_close)
                engine?.Kill();
        }
    }
}
