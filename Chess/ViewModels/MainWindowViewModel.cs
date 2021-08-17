using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Chess.Models;

namespace Chess.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            var bvm = new BoardViewModel(new AIChessBoard());
            var gamePanel = new GamePanelViewModel(bvm, null, null);
            Menu = new MenuViewModel(this);
            ChildViews = new PlayViewModel(bvm, gamePanel, Menu);
        }

        private ViewModelBase? childViews;
        public ViewModelBase? ChildViews
        {
            get { return childViews; }
            set { childViews = value; NotifyPropertyChanged(); }
        }
        public MenuViewModel Menu { get; set; }

        new public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
