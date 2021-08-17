using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Chess.ViewModels;
using Chess.Models;

namespace Chess.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContextChanged += Util.InitialiseViewModelBase;
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
