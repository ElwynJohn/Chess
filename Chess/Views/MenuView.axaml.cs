using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

using Chess.ViewModels;

namespace Chess.Views
{
    public partial class MenuView : ViewBase
    {
        public MenuView()
        {
            Initialized += Util.InitialiseViewModelBase;
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
