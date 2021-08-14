using Avalonia.Controls;

using ReactiveUI;

namespace Chess.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
#pragma warning disable CS8618
        public ContentControl View;
        public Window Window;
#pragma warning restore CS8618
    }
}
