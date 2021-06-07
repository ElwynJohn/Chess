using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Input;
using Chess.Models;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

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

		public void change_rectangle_color(object sender, PointerReleasedEventArgs e)
		{
			if (e.InitialPressMouseButton != MouseButton.Right)
				return;
			
			Rectangle rectangle = (Rectangle)sender;
			ChessTile oldTile = (ChessTile)rectangle.DataContext;
			ChessTile tile = new ChessTile
			{
				HighlightedFill = oldTile.HighlightedFill,
				NormalFill = oldTile.NormalFill
			};

			if (oldTile.IsHighlighted)
				tile.Fill = tile.NormalFill;
			else
				tile.Fill = tile.HighlightedFill;
			tile.IsHighlighted = !oldTile.IsHighlighted;	

			rectangle.DataContext = tile;
		}
    }
}
