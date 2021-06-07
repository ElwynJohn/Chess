using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
		public MainWindowViewModel()
		{
			List = new PlayViewModel();
		}

        public PlayViewModel List { get; }
	}
}
