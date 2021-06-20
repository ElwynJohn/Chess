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
            MainWindowViewModel? model = (MainWindowViewModel?)((Control)sender).FindAncestorOfType<Window>().DataContext;
            if (model != null)
                model.List = new PlayViewModel();
        }

        public void on_click_history(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel? model = (MainWindowViewModel?)((Control)sender).FindAncestorOfType<Window>().DataContext;
            if (model != null)
                model.List = new HistoryViewModel();
        }
    }
}
