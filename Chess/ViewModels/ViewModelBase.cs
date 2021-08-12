using Avalonia.Controls;

using ReactiveUI;

namespace Chess.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
#pragma warning disable CS8618
        public UserControl View;
        public Window Window;
#pragma warning restore CS8618
    }
}
