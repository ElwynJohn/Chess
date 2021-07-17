using System;
using System.IO;
using System.Collections.ObjectModel;

using Chess.Models;

namespace Chess.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        public HistoryViewModel(MenuViewModel menuVM)
        {
            Menu = menuVM;
            Boards = new ObservableCollection<BoardViewModel>();
            string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chess", "GameHistorys");
            string[] filePaths = Directory.GetFiles(dirPath);
            foreach (string filePath in filePaths)
                Boards.Add(new BoardViewModel(new ChessBoard(gameRecordPath: filePath), false, false));
        }

        public ObservableCollection<BoardViewModel> Boards { get; set; }
        public MenuViewModel Menu { get; }
    }
}
