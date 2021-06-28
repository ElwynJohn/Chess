using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Chess.Views
{
    public partial class GameStatsView : UserControl
    {
        public GameStatsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
