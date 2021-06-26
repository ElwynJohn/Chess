using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.VisualTree;
using Chess.ViewModels;

namespace Chess.Views
{
    public partial class GamePanelView : UserControl
    {
        public GamePanelView()
        {
            InitializeComponent();
        }

        public void ClickMoves(object sender, PointerReleasedEventArgs e)
        {
            GamePanelViewModel? gamePanel = this.DataContext as GamePanelViewModel;
            if (gamePanel != null)
                gamePanel.Content = gamePanel.Moves;
        }

        public void ClickStats(object sender, PointerReleasedEventArgs e)
        {
            GamePanelViewModel? gamePanel = this.DataContext as GamePanelViewModel;
            if (gamePanel != null)
                gamePanel.Content = gamePanel.Stats;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
