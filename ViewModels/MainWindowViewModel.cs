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
            List = new PlayViewModel(bvm, gamePanel);
        }

        private ViewModelBase? list;
        public ViewModelBase? List
        {
            get { return list; }
            set { list = value; NotifyPropertyChanged(); }
        }

        new public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
