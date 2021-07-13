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

        public static void EWrite(string message, [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
        {
            WriteHeader(Error, filePath, line, callerName);
            Write(message);
            StackTrace st = new StackTrace();
            Write(st.ToString());
        }
        public static void WWrite(string message, [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
        {
            WriteHeader(Warning, filePath, line, callerName);
            Write(message);
        }
        public static void IWrite(string message, [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
        {
            WriteHeader(Info, filePath, line, callerName);
            Write(message);
        }
        public static void DWrite(string message, [CallerFilePath] string filePath="",
                [CallerLineNumber] int line=0, [CallerMemberName] string callerName="")
        {
            WriteHeader(DebugLevel.Debug, filePath, line, callerName);
            Write(message);
        }

        private static void Write(string message)
        {
            foreach (TextWriter writer in Writers)
            {
                writer.Write(message);
            }
        }

        private static void WriteHeader(DebugLevel level, string filePath,
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
            Write(sb.ToString());
        }

        private static string GetRelativePath(string fullPath)
        {
            return "." + Regex.Replace(fullPath, @".*chess", "", RegexOptions.IgnoreCase);
        }
    }
}
