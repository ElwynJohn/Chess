using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Chess.ViewModels;
using Chess.Models;

namespace Chess.Views
{
    public partial class MoveHistoryView : ViewBase
    {
        public MoveHistoryView()
        {
            Initialized += Util.InitialiseViewModelBase;
            InitializeComponent();
        }

        public void SvOnInitialized(object sender, EventArgs e)
        {
            var vm = DataContext as MoveHistoryViewModel;
            if (vm == null)
            {
                Logger.EWrite("Cast failed.");
                return;
            }
            var sv = sender as ScrollViewer;
            if (sv == null)
            {
                Logger.EWrite("Cast failed.");
                return;
            }
            vm.sv = sv;
            sv.LayoutUpdated += vm.SvLayoutUpdated;
        }

        public void previous_move(object sender, RoutedEventArgs e)
        {
            MoveHistoryViewModel? board = (MoveHistoryViewModel?)this.DataContext;
            if (board != null)
                board.Bvm.PreviousMove();
        }
        public void next_move(object sender, RoutedEventArgs e)
        {
            MoveHistoryViewModel? board = (MoveHistoryViewModel?)this.DataContext;
            if (board != null)
                board.Bvm.NextMove();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
