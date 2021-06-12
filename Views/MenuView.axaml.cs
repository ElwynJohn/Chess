using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Chess.ViewModels;

namespace Chess.Views
{
    public partial class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void on_click_play(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel model = (MainWindowViewModel)((Control)sender).FindAncestorOfType<Window>().DataContext;
            string fen = "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2";
            model.List = new PlayViewModel(fen);
        }

        public void on_click_history(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel model = (MainWindowViewModel)((Control)sender).FindAncestorOfType<Window>().DataContext;
            string[] gameHistorys = new string[]
            {
                "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2",
                "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2"
            };
            model.List = new HistoryViewModel(gameHistorys);
        }
    }
}
