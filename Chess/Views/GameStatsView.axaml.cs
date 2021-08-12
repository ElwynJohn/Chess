using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Chess.Views
{
    public partial class GameStatsView : ViewBase
    {
        public GameStatsView()
        {
            Initialized += Util.InitialiseViewModelBase;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
