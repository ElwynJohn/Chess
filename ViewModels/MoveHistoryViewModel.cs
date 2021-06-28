using System.Collections.ObjectModel;
using Chess.Models;

namespace Chess.ViewModels
{
    public class MoveHistoryViewModel : ViewModelBase
    {
        public BoardViewModel bvm;
        public MoveHistoryViewModel(ObservableCollection<TurnData> turns, BoardViewModel bvm)
        {
            this.bvm = bvm;
            Turns = turns;
        }
        ObservableCollection<TurnData> Turns { get; set; }
    }
}
