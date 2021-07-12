using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Chess.ViewModels;
using Chess.Models;

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

        public void on_click(object sender, RoutedEventArgs e)
        {
            MenuViewModel? menuVM = DataContext as MenuViewModel;
            Button? button = sender as Button;
            if (menuVM == null || button == null)
                return;
            string? buttonText = button.Content as string;
            if (buttonText == null)
                return;

            menuVM.ClickButton(buttonText);
        }
    }
}
