using System.Collections.ObjectModel;
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

        public void on_click(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel? mainWindowVM = ((Control)sender).FindAncestorOfType<Window>().DataContext as MainWindowViewModel;
            MenuViewModel? menuVM = DataContext as MenuViewModel;
            if (mainWindowVM == null)
                return;
            if (menuVM == null)
                return;

            string buttonText = (string)((Button)sender).Content;
            switch (buttonText)
            {
                case "Play":
                    menuVM.Buttons.Clear();
                    menuVM.Buttons.Add("Back");
                    menuVM.Buttons.Add("Online");
                    menuVM.Buttons.Add("vs Computer");
                    menuVM.Buttons.Add("Local");
                    break;
                case "Back":
                    menuVM.Buttons.Clear();
                    menuVM.Buttons.Add("Play");
                    menuVM.Buttons.Add("Match History");
                    menuVM.Buttons.Add("Statistics");
                    break;
                case "Online":
                    mainWindowVM.List = new PlayViewModel(new BoardViewModel());
                    break;
                case "vs Computer":
                    mainWindowVM.List = new PlayViewModel(new AIBoardViewModel());
                    break;
                case "Local":
                    mainWindowVM.List = new PlayViewModel(new BoardViewModel());
                    break;
                case "Match History":
                    mainWindowVM.List = new HistoryViewModel();
                    break;
                case "Statistics":
                    break;
            }
        }
    }
}
