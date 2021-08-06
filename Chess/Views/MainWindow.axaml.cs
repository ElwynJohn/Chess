using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Chess.Views
{
    public partial class MainWindow : Window
    {
        public static double sHeight {get; set;}
        public static double sWidth {get; set;}

        public MainWindow()
        {
            EffectiveViewportChanged += OnResized;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public void OnResized(object? sender, EventArgs e)
        {
            sHeight = Height;
            sWidth = Width;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
