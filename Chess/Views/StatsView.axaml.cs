using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Diagnostics;
using System;
using Chess.Models;

namespace Chess.Views
{
    public partial class StatsView : UserControl
    {
        public StatsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Stopwatch sw = new Stopwatch();
        public static bool doTime = true;
        public void StatsViewLoaded(object? sender, EventArgs e)
        {
            if (!doTime)
                return;
            sw.Stop();
            Logger.DWrite($"StatsView loaded in {sw.ElapsedMilliseconds}ms.");
            sw.Reset();
            doTime = false;
        }
    }
}
