using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Pastel;
using static Chess.Models.DebugLevel;

namespace Chess.Models
{
    public static class Logger
    {
        private struct Writer
        {
            public Writer(TextWriter writer, bool isColoured) {
                this.writer = writer;
                this.isColoured = isColoured;
            }
            public TextWriter writer;
            public bool isColoured;
        }

        private static AsyncLocal<DebugLevel> debugLevelT = new AsyncLocal<DebugLevel>();
        public static DebugLevel DebugLevelT { get => debugLevelT.Value;
            set {
                DebugLevel temp = debugLevelT.Value;
                debugLevelT.Value = value;
                prevDebugLevelT.Value = temp;
            } }
        // If you want to write multiple messages under the same header then write
        // to Buffer first, then call xWrite() with no message argument
        private static AsyncLocal<string> buffer = new AsyncLocal<string>();
        public static string Buffer {
            get => buffer.Value != null ? buffer.Value : "";
            set => buffer.Value = value; }


        private static AsyncLocal<DebugLevel> prevDebugLevelT = new AsyncLocal<DebugLevel>();
        private static List<Writer> Writers = new List<Writer>();
        private const string LOC_COL = "#076672";
        private const string ERR_COL = "#aa322F";
        private const string WAR_COL = "#B59D00";
        private const string INF_COL = "#35C15A";
        private const string DBG_COL = "#586DD2";


        public static void SetLevelToPrev() => DebugLevelT = prevDebugLevelT.Value;

        public static void AddWriter(TextWriter writer, bool isColoured)
            => Writers.Add(new Writer(writer, isColoured));
        public static void ClearWriters() => Writers.Clear();
        public static void RemoveWriter(TextWriter writerToRemove)
        {
            for (int i = 0; i < Writers.Count; i++)
            {
                if (Writers[i].writer == writerToRemove)
                    Writers.RemoveAt(i);
            }
        }

        public static void ExceptionWrite(Exception e, string message="",
                [CallerFilePath] string filePath="", [CallerLineNumber] int line=0,
                [CallerMemberName] string callerName="")
        {
            message += " " + e.ToString() + ": " + e.Message;
            Write(message, Error, filePath, line, callerName);
        }

        public static void EWrite(string message="", [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
        {
            if (DebugLevelT >= Error)
                Write(message, Error, filePath, line, callerName, true);
        }

        public static void WWrite(string message="", [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
        {
            if (DebugLevelT >= Warning)
                Write(message, Warning, filePath, line, callerName);
        }

        public static void IWrite(string message="", [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
        {
            if (DebugLevelT >= Info)
                Write(message, Info, filePath, line, callerName);
        }

        public static void DWrite(string message="", [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
        {
            if (DebugLevelT >= DebugLevel.Debug)
                Write(message, DebugLevel.Debug, filePath, line, callerName);
        }

        private static void Write(string message, DebugLevel level,
                string filePath, int line, string callerName, bool writeStackTrace = false)
        {
            StackTrace st = new StackTrace();
            Func<string,string,string> pastel;
            foreach (Writer writer in Writers)
            {
                pastel = writer.isColoured ? ConsoleExtensions.Pastel : (input, col) => input;
                Action<string> writeMessage = message => {
                        if (level == Error)
                            message = pastel(message, ERR_COL);
                        writer.writer.Write(message);
                        writer.writer.Write('\n');
                };

                string header = GenerateHeader
                    (level, pastel, filePath, line, callerName);
                writer.writer.Write(header);

                if (message == "")
                    writeMessage(Buffer);
                else
                    writeMessage(message);

                if (writeStackTrace)
                    writer.writer.Write(pastel(st.ToString(), ERR_COL));
            }
            Buffer = "";
        }

        private static string GenerateHeader(DebugLevel level, Func<string,string,string> pastel,
                string filePath, int line, string callerName)
        {
            StringBuilder sb = new StringBuilder();
            if (level == Error)
                sb.Append(pastel("ERROR: ", ERR_COL));
            else if (level == Warning)
                sb.Append(pastel("WARN : ", WAR_COL));
            else if (level == Info)
                sb.Append(pastel("INFO : ", INF_COL));
            else if (level == DebugLevel.Debug)
                sb.Append(pastel("DEBUG: ", DBG_COL));

            sb.Append($"[{DateTime.UtcNow.ToString("s")}] ");

            string relativePath = Path.GetRelativePath
                (Directory.GetCurrentDirectory(), filePath);
            sb.Append(pastel($"{relativePath}:{line}:{callerName}: ", LOC_COL));
            return sb.ToString();
        }
    }
}
