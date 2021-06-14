using Chess.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;

namespace Chess.ViewModels
{
    public class PlayViewModel : ViewModelBase
    {
        public PlayViewModel(string fen)
        {
            Board = new BoardViewModel(fen, true);
        }

        public BoardViewModel Board { get; }
    }
}
