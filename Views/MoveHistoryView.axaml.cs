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
            BoardViewModel? board = (BoardViewModel?)this.DataContext;
            if (board != null)
                board.PreviousMove();
        }
        public void next_move(object sender, RoutedEventArgs e)
        {
            BoardViewModel? board = (BoardViewModel?)this.DataContext;
            if (board != null)
                board.NextMove();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
