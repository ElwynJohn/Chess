using System;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Chess.ViewModels;

namespace Chess.Models
{
    public class ChessBoard
    {
        /// <summary>0x88 representation of the board</summary>
        private ChessPiece[] state = new ChessPiece[128];
        public BoardViewModel? BoardGUI;
        private ChessTile[] tiles;

        public static Dictionary<char, ChessPiece> FenToPieceMap = new Dictionary<char, ChessPiece>
        {
            {'r', ChessPiece.Castle},
            {'n', ChessPiece.Knight},
            {'b', ChessPiece.Bishop},
            {'q', ChessPiece.Queen},
            {'k', ChessPiece.King},
            {'p', ChessPiece.Pawn},
            {'R', ChessPiece.Castle | ChessPiece.IsWhite},
            {'N', ChessPiece.Knight | ChessPiece.IsWhite},
            {'B', ChessPiece.Bishop | ChessPiece.IsWhite},
            {'Q', ChessPiece.Queen | ChessPiece.IsWhite},
            {'K', ChessPiece.King | ChessPiece.IsWhite},
            {'P', ChessPiece.Pawn | ChessPiece.IsWhite},
        };

        public static int Pos0x88To64(int pos)
        {
            return (pos + (pos & 7)) >> 1;
        }

        public static int Pos64To0x88(int pos)
        {
            return pos + (pos & ~7);
        }

        /// <summary>Get or set the board state by 64 (default) or 0x88 based
        /// index.</summary>
        public ChessPiece this[int pos, bool pos0x88 = false]
        {
            get
            {
                if (pos0x88)
                    return state[pos];
                else
                    return this[pos % 8, pos / 8];
            }
            set
            {
                if (pos0x88)
                {
                    state[pos] = value;
                    tiles[pos].Update();
                }
                else
                    this[pos % 8, pos / 8] = value;
            }
        }

        /// <summary>Get or set the board by file-rank.</summary>
        public ChessPiece this[int file, int rank]
        {
            get => state[rank * 16 + file];
            set { state[rank * 16 + file] = value; tiles[rank * 8 + file].Update(); }
        }

        public ChessBoard(BoardViewModel bvm, string fen)
        {
            BoardGUI = bvm;
            tiles = bvm.tiles;

            var pieces = bvm.ParseFen(fen);

            for (int i = 0; i < 64; i++)
                state[Pos64To0x88(i)] = pieces[i];
        }

        public override string ToString()
        {
            StringBuilder rv = new StringBuilder();
            for (int i = 0; i < 128; i++)
            {
                if ((i & 0x88) != 0)
                    continue;
                int file = i & 7;
                char c = FenToPieceMap.FirstOrDefault(x => x.Value.Equals(this[i, true])).Key;
                rv.Append(c != '\0' ? c : '0');
                if (file == 7)
                    rv.Append("\n");
                else
                    rv.Append(" ");
            }
            rv.Remove(rv.Length - 1, 1); // Get rid of trailing newline;
            return rv.ToString();
        }
    }
}
