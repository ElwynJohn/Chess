using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Chess.Models.DebugLevel;

namespace Chess.Models
{
    public static class Logger
    {
        [ThreadStatic] private static DebugLevel prevDebugLevelT;
        [ThreadStatic] private static DebugLevel debugLevelT = All;
        public static DebugLevel DebugLevelT { get => debugLevelT;
            set {
                prevDebugLevelT = debugLevelT;
                debugLevelT = value;
            } }
        public static List<TextWriter> Writers { get; set; } = new List<TextWriter>();

        // If you want to write multiple messages under the same header then write
        // to BufMes (buffered message) first, then call xWrite() with no message argument
        public static string BufMes { get; set; } = "";

        public static void EWrite(string message="", [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
            => Write(message, Error, filePath, line, callerName);

        public static void WWrite(string message="", [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
            => Write(message, Warning, filePath, line, callerName);

        public static void IWrite(string message="", [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
            => Write(message, Info, filePath, line, callerName);

        public static void DWrite(string message="", [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
            => Write(message, DebugLevel.Debug, filePath, line, callerName);

        private static void Write(string message, DebugLevel level,
                string filePath, int line, string callerName)
        {
            string header = GenerateHeader(level, filePath, line, callerName);
            StackTrace st = new StackTrace();
            foreach (TextWriter writer in Writers)
            {
                writer.Write(header);
                if (message == "")
                    writer.Write(BufMes);
                else
                    writer.Write(message);
                writer.Write('\n');
                if (level == Error)
                    writer.Write(st.ToString());
            }
        }

        private static string GenerateHeader(DebugLevel level, string filePath,
                int line, string callerName)
        {
            StringBuilder sb = new StringBuilder();
            if (level == Error)
            {
                sb.Append("ERROR: ");
            }
            else if (level == Warning)
            {
                sb.Append("WARN : ");
            }
            else if (level == Info)
            {
                sb.Append("INFO : ");
            }
            else if (level == DebugLevel.Debug)
            {
                sb.Append("DEBUG: ");
            }

            sb.Append($"[{DateTime.UtcNow.ToString("s")}] ");

            sb.Append($"{GetRelativePath(filePath)}:{line}:{callerName}: ");
            return sb.ToString();
        }

        private static string GetRelativePath(string fullPath)
        {
            return "." + Regex.Replace(fullPath, @".*chess", "", RegexOptions.IgnoreCase);
        }
    }
}
