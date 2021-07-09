using System.ComponentModel;
using System.Runtime.CompilerServices;
using Chess.Models;

namespace Chess.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            List = new PlayViewModel(new BoardViewModel(new AIChessBoard()));
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
