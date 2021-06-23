using Chess.Models;
using System.Collections.ObjectModel;

namespace Chess.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public MenuViewModel()
        {
            Buttons = new ObservableCollection<string>(new string[]
            {
                "Play",
                "Match History",
                "Statistics",
            });
        }

        public ObservableCollection<string> Buttons { get; set; }
    }
}
