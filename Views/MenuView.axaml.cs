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

        public void on_pointer_released_history(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel model = (MainWindowViewModel)((Control)sender).FindAncestorOfType<Window>().DataContext;
            model.List = new HistoryViewModel();
        }
    }
}
