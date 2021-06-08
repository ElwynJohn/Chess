using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
		public MainWindowViewModel()
		{
			string fen = "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2";
			List = new PlayViewModel(fen);
		}

        public PlayViewModel List { get; }
	}
}
