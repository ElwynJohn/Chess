using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Chess.ViewModels;

namespace Chess.Views
{
    public partial class GameStatsView : ViewBase
    {
        public GameStatsView()
        {
            Initialized += Util.InitialiseViewModelBase;
            Initialized += (s, e) =>
                ((GameStatsViewModel)DataContext).OnViewInitialisation(s, e);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
