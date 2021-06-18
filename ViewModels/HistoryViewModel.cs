using System;
using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;
using Chess.Models;

namespace Chess.ViewModels
{
    public class HistoryViewModel : ViewModelBase
    {
        public HistoryViewModel()
        {
            Boards = new ObservableCollection<BoardViewModel>();
            string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chess", "GameHistorys");
            string[] filePaths = Directory.GetFiles(dirPath);
            foreach (string filePath in filePaths)
                Boards.Add(new BoardViewModel(filePath, false, false));
        }

        public ObservableCollection<BoardViewModel> Boards { get; set; }
    }
}
