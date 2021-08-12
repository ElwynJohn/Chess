using Avalonia.Controls;

using Chess.ViewModels;

namespace Chess.Views
{
    public class ViewBase : UserControl
    {
        public ViewModelBase? ViewModel
        {
            get => DataContext as ViewModelBase;
        }
    }
}
