using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Chess.Views
{
    public partial class PlayView : UserControl
    {
        public PlayView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}