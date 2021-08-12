using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;

using Chess.Models;
using Chess.ViewModels;

namespace Chess.Views
{
    public partial class HistoryView : UserControl
    {
        public HistoryView()
        {
            Initialized += Util.InitialiseViewModelBase;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void InitScrollViewer(object sender, EventArgs e)
        {
            var sv = sender as ScrollViewer;
            if (sv == null)
            {
                Logger.EWrite("Failed to cast Object to ScrollViewer");
                return;
            }
            var vm = sv.DataContext as HistoryViewModel;
            if (vm == null)
            {
                Logger.EWrite("Failed to cast ScrollViewer to HistoryViewModel");
                return;
            }
            sv.ScrollChanged += vm.HandleScrollChange;
        }

        public void InitImage(object sender, EventArgs e)
        {
            var vm = DataContext as HistoryViewModel;
            if (vm == null)
            {
                Logger.EWrite("Casting HistoryView's DataContext to HistoryViewModel " +
                    "resulted in a null value.");
                return;
            }
            var image = sender as Image;
            if (image == null)
            {
                Logger.EWrite("Failed to cast Object to Image.");
                return;
            }
            image.PointerReleased += vm.LoadHistory;
        }

        public static bool doTime = true;
        public static Stopwatch sw = new Stopwatch();

        public void HistoryViewLoaded(object sender, EventArgs e)
        {
            if (!doTime)
                return;
            sw.Stop();
            Logger.DWrite($"HistoryView loaded in  {sw.ElapsedMilliseconds}ms.");
            sw.Reset();
            doTime = false;
        }
    }
}
