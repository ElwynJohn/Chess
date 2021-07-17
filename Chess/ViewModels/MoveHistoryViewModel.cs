using System.Collections.ObjectModel;
using System;
using Avalonia.Media;
using Chess.Models;

namespace Chess.ViewModels
{
    public class MoveHistoryViewModel : ViewModelBase
    {
        public MoveHistoryViewModel(BoardViewModel bvm)
        {
            Bvm = bvm;
            Bvm.Board.Update += UpdateTurns;
            Turns = new ObservableCollection<TurnData>();
        }

        public BoardViewModel Bvm { get; private init; }
        public ObservableCollection<TurnData> Turns { get; set; }

        public void UpdateTurns(object sender, BoardUpdateEventArgs e)
        {
            // if UpdateTurns is called but a move has not been made, such
            // as when a piece is promoted but not moved, then return.
            if (e.Move == null || e.Board == null || e.Board.Boards == null)
                return;

            bool newTurn = e.Board.Boards.Count % 2 == 0;
            if (newTurn)
                Turns.Add(new TurnData()
                {
                    Turn = (e.Board.Boards.Count) / 2,
                    WhiteMove = e.Move,
                    BlackMove = default,
                    Fill = Turns.Count % 2 == 1 ?
                        new SolidColorBrush(0xFF2A2A40) :
                        new SolidColorBrush(0xFF323240)
                });
            else
            {
                TurnData temp = Turns[Turns.Count - 1];
                Turns[Turns.Count - 1] = new TurnData()
                {
                    Turn = temp.Turn,
                    WhiteMove = temp.WhiteMove,
                    BlackMove = e.Move,
                    Fill = temp.Fill
                };
            }
        }
    }
}
