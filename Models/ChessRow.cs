using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Chess.Models
{
    public class ChessRow
    {
        public ObservableCollection<ChessTile>? RowTiles { get; set; }
    }
}
