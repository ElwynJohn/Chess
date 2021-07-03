using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;

using Chess.ViewModels;
using Chess.Models;

using static Chess.Models.ChessPiece;

namespace Chess.Views
{
    public partial class PlayView : UserControl
    {
        public PlayView()
        {
            InitializeComponent();
        }

        public void on_pointer_released(object sender, PointerReleasedEventArgs e)
        {
            var panel = sender as Panel;

            var pvm = (PlayViewModel?)this.DataContext;

            if (panel != null)
            {
                ChessPiece promotion = Queen;
                switch (panel.Name)
                {
                    case "PromoteBishop":
                        promotion = Bishop;
                        break;
                    case "PromoteKnight":
                        promotion = Knight;
                        break;
                    case "PromoteQueen":
                        promotion = Queen;
                        break;
                    case "PromoteCastle":
                        promotion = Castle;
                        break;
                }

                if (pvm == null)
                    return;

                var bvm = pvm.Board;
                bvm.RequestPromotion(promotion);
                bvm.SyncBoardState();
                pvm.IsPromoting = false;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
