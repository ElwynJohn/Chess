using System.Collections.ObjectModel;

namespace Chess.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        public HistoryViewModel(string[] fen)
        {
            Boards = new ObservableCollection<BoardViewModel>();
            for (int i = 0; i < fen.Length; i++)
            {
                Boards.Add(new BoardViewModel(fen[i], false));
            }
        }

        public ObservableCollection<BoardViewModel> Boards { get; set; }
    }
}
