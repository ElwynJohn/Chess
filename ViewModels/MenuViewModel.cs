using System.Collections.ObjectModel;
using Avalonia.Controls;
using Chess.Models;

namespace Chess.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public MenuViewModel(MainWindowViewModel parentWindow)
        {
            this.parentWindow = parentWindow;
            Buttons = new ObservableCollection<string>();
            DisplayDefault();
        }

        public ObservableCollection<string> Buttons { get; set; }

        private enum ButtonMenu
        {
            Default,
            GameMode,
            Time,
            MatchHistory,
            Statistics,
        }
        private ButtonMenu buttonMenu = ButtonMenu.Default;
        private MainWindowViewModel parentWindow;
        private BoardViewModel? bvm = null;

        // Check which ButtonMenu is being displayed and then call a function that
        // will handle any button click that can occur on that ButtonMenu
        public void ClickButton(string buttonText)
        {
            switch (buttonMenu)
            {
                case ButtonMenu.Default:
                    Default(buttonText);
                    break;
                case ButtonMenu.GameMode:
                    GameMode(buttonText);
                    break;
                case ButtonMenu.Time:
                    Time(buttonText);
                    break;
            }
        }

        public void Default(string buttonText)
        {
            switch (buttonText)
            {
                case "Play":
                    DisplayGameMode();
                    break;
                 case "Match History":
                    parentWindow.ChildViews = new HistoryViewModel(this);
                    break;
                case "Statistics":
                    break;
            }
        }
        public void GameMode(string buttonText)
        {
            switch (buttonText)
            {
               case "Back":
                    DisplayDefault();
                    return;
                case "Online":
                    bvm = new BoardViewModel(new AIChessBoard());
                    break;
                case "vs Computer":
                    bvm = new BoardViewModel(new AIChessBoard());
                    break;
                case "Local":
                    bvm = new BoardViewModel(new ChessBoard());
                    break;
            }
            DisplayTime();
        }
        public void Time(string buttonText)
        {
            if (bvm == null)
                return;
            switch (buttonText)
            {
                case "Back":
                    DisplayGameMode();
                    return;
               case "5 mins":
                    parentWindow.ChildViews = CreateTimedMode(bvm, 300);
                    break;
                case "10 mins":
                    parentWindow.ChildViews = CreateTimedMode(bvm, 600);
                    break;
                case "20 mins":
                    parentWindow.ChildViews = CreateTimedMode(bvm, 1200);
                    break;
                case "30 mins":
                    parentWindow.ChildViews = CreateTimedMode(bvm, 1800);
                    break;
                case "60 mins":
                    parentWindow.ChildViews = CreateTimedMode(bvm, 3600);
                    break;
                case "No timer":
                    parentWindow.ChildViews = new PlayViewModel
                        (bvm, new GamePanelViewModel(bvm, null, null), this);
                    break;
            }
            DisplayDefault();
        }

        private void DisplayDefault()
        {
            Buttons.Clear();
            Buttons.Add("Play");
            Buttons.Add("Match History");
            Buttons.Add("Statistics");
            buttonMenu = ButtonMenu.Default;
        }
        private void DisplayGameMode()
        {
            Buttons.Clear();
            Buttons.Add("Back");
            Buttons.Add("Online");
            Buttons.Add("vs Computer");
            Buttons.Add("Local");
            buttonMenu = ButtonMenu.GameMode;
        }
        private void DisplayTime()
        {
            Buttons.Clear();
            Buttons.Add("Back");
            Buttons.Add("5 mins");
            Buttons.Add("10 mins");
            Buttons.Add("20 mins");
            Buttons.Add("30 mins");
            Buttons.Add("60 mins");
            Buttons.Add("No timer");
            buttonMenu = ButtonMenu.Time;
        }

        private PlayViewModel CreateTimedMode(BoardViewModel bvm, int gameTime)
        {
            var whiteTimer = new ChessTimer(bvm.Board, true, gameTime);
            var blackTimer = new ChessTimer(bvm.Board, false, gameTime);
            var gamePanel = new GamePanelViewModel(bvm, whiteTimer, blackTimer);
            var pvm = new PlayViewModel(bvm, gamePanel, this);
            whiteTimer.Start();
            return pvm;
        }
    }
}
