using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Chess.ViewModels;

namespace Chess.Views
{
    public partial class MoveHistoryView : UserControl
    {
        public MoveHistoryView()
        {
            InitializeComponent();
        }

        public void previous_move(object sender, RoutedEventArgs e)
        {
            MoveHistoryViewModel? board = (MoveHistoryViewModel?)this.DataContext;
            if (board != null)
                board.bvm.PreviousMove();
        }
        public void next_move(object sender, RoutedEventArgs e)
        {
            MoveHistoryViewModel? board = (MoveHistoryViewModel?)this.DataContext;
            if (board != null)
                board.bvm.NextMove();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
