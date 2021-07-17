using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Chess.Models;

namespace Tests
{
    public class LoggerTests
    {
        [Fact]
        public void WriteBufferTest()
        {
            Logger.DebugLevelT = DebugLevel.All;
            var sw = new StringWriter();
            Logger.AddWriter(sw, false);

            Logger.Buffer += "one ";
            Logger.Buffer += "two ";
            Logger.Buffer += "three";
            Logger.EWrite();
            Logger.Buffer += "two ";
            Logger.Buffer += "three ";
            Logger.Buffer += "four";
            Logger.WWrite();
            Logger.Buffer += "three ";
            Logger.Buffer += "four ";
            Logger.Buffer += "five";
            Logger.IWrite();
            Logger.Buffer += "four ";
            Logger.Buffer += "five ";
            Logger.Buffer += "six";
            Logger.DWrite();

            Match match = Regex.Match(sw.GetStringBuilder().ToString(), @": one two three$",
                    RegexOptions.Multiline);
            Assert.True(match.Success);

            match = Regex.Match(sw.GetStringBuilder().ToString(), @": two three four$",
                    RegexOptions.Multiline);
            Assert.True(match.Success);

            match = Regex.Match(sw.GetStringBuilder().ToString(), @": three four five$",
                    RegexOptions.Multiline);
            Assert.True(match.Success);

            match = Regex.Match(sw.GetStringBuilder().ToString(), @": four five six$",
                    RegexOptions.Multiline);
            Assert.True(match.Success);

            Assert.Equal("", Logger.Buffer);
        }

        [Fact]
        public void BufferAsyncTest()
        {
            Logger.DebugLevelT = DebugLevel.All;
            var sw = new StringWriter();
            Logger.AddWriter(sw, false);
            var waitHandle = new AutoResetEvent(false);

            Task t = new Task((AutoResetEvent) =>
            {
                waitHandle.WaitOne();
                Logger.Buffer += "three four";
                waitHandle.Set();
                Logger.IWrite();
            }, waitHandle);
            t.Start();

            Thread.Sleep(5);
            Logger.Buffer += "one ";
            waitHandle.Set();
            waitHandle.WaitOne();
            Logger.Buffer += "two";
            Logger.IWrite();

            t.Wait();

            Match match = Regex.Match(sw.GetStringBuilder().ToString(), @": one two$",
                    RegexOptions.Multiline);
            Assert.True(match.Success);

            match = Regex.Match(sw.GetStringBuilder().ToString(), @": three four$",
                    RegexOptions.Multiline);
            Assert.True(match.Success);
        }

        [Fact]
        public void DebugLevelTest()
        {
            var sw = new StringWriter();
            Logger.AddWriter(sw, false);

            Logger.DebugLevelT = DebugLevel.Error;
            Logger.WWrite("test");
            Logger.DebugLevelT = DebugLevel.Warning;
            Logger.IWrite("test");
            Logger.DebugLevelT = DebugLevel.Info;
            Logger.DWrite("test");
            Logger.DebugLevelT = DebugLevel.None;
            Logger.DWrite("test");

            Match match = Regex.Match(sw.GetStringBuilder().ToString(), @"test");
            Assert.True(!match.Success);

            Logger.DebugLevelT = DebugLevel.Error;
            Logger.EWrite("test one two three test");
            Logger.DebugLevelT = DebugLevel.Warning;
            Logger.WWrite("test one two three test");
            Logger.DebugLevelT = DebugLevel.Info;
            Logger.IWrite("test one two three test");
            Logger.DebugLevelT = DebugLevel.Debug;
            Logger.DWrite("test one two three test");
            Logger.DebugLevelT = DebugLevel.All;
            Logger.DWrite("test one two three test");

            MatchCollection matches = Regex.Matches
                (sw.GetStringBuilder().ToString(), @": test one two three test$",
                    RegexOptions.Multiline);
            Assert.Equal(5, matches.Count);
        }

        [Fact]
        public void DebugLevelPrevTest()
        {
            Logger.DebugLevelT = DebugLevel.All;
            Logger.DebugLevelT = DebugLevel.Debug;
            Assert.Equal(DebugLevel.Debug, Logger.DebugLevelT);
            Logger.SetLevelToPrev();
            Assert.Equal(DebugLevel.All, Logger.DebugLevelT);
        }
    }
}
