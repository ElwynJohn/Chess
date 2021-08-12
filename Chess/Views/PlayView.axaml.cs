using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;

using Chess.ViewModels;
using Chess.Models;

using static Chess.Models.ChessPiece;

namespace Chess.Views
{
    public partial class PlayView : ViewBase
    {
        public PlayView()
        {
            Initialized += Util.InitialiseViewModelBase;
            InitializeComponent();
        }

        public void on_pointer_released(object sender, PointerReleasedEventArgs e)
        {
            var panel = sender as Panel;

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

                var pvm = (PlayViewModel?)this.DataContext;
                if (pvm == null)
                    return;
                var bvm = pvm.Bvm;
                if (bvm != null && bvm.IsInteractable)
                    pvm.Bvm?.Board?.PromoteTo(promotion);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
