using System.Collections.ObjectModel;
using Chess.Models;

namespace Chess.ViewModels
{
    public class MoveHistoryViewModel : ViewModelBase
    {
        public MoveHistoryViewModel(ObservableCollection<TurnData> turns)
        {
            Turns = turns;
        }
        ObservableCollection<TurnData> Turns { get; set; }
    }
}
