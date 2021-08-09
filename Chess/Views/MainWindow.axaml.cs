using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Chess.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Closed += Program.OnExit;
            Closed += (sender, e) => System.Environment.Exit(0);
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
