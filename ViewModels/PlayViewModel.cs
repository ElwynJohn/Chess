namespace Chess.ViewModels
{
    public class PlayViewModel : ViewModelBase
    {
        public PlayViewModel(BoardViewModel board)
        {
            Board = board;
            Menu = new MenuViewModel();
            GamePanel = new GamePanelViewModel(board);
        }

        public BoardViewModel Board { get; }
        public MenuViewModel Menu { get; }
        public GamePanelViewModel GamePanel { get; }
    }
}
