using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

using Chess.Views;
using Chess.Models;

namespace Chess.ViewModels
{
    public struct GameHistory
    {
        public Bitmap BoardImage { get; set; }
        public static int BoardImageSize { get; set; }
        public int Index { get; set; }
        public int Number { get => Index + 1; }
        public static int Height { get; set; }
        public static int Width { get; set; }
        public static int Margin { get; set; }
        public int Position { get => Index * Height + 2 * Index * Margin + Margin; }
    }

    public class HistoryViewModel : ViewModelBase
    {
        public HistoryViewModel(MenuViewModel menuVM)
        {
            var sw = new Stopwatch();
            sw.Start();

            Menu = menuVM;
            string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chess", "GameHistorys");
            string[] filePaths = Directory.GetFiles(dirPath, @"*.png");
            for (int i = 0; i < filePaths.Length; i++)
            {
                var hist = new GameHistory();
                hist.BoardImage = new Bitmap(filePaths[i]);
                hist.Index = i;
                cachedHistories.Add(hist);
            }
            GameHistory.BoardImageSize = 160;
            GameHistory.Height = 200;
            GameHistory.Width = 600;
            GameHistory.Margin = 10;
            Height = (GameHistory.Height + GameHistory.Margin * 2) * cachedHistories.Count;

            sw.Stop();
            Logger.DWrite($"time to construct history view model: {sw.ElapsedMilliseconds}");
        }


        public MenuViewModel Menu { get; }
        public ObservableCollection<GameHistory> Histories { get; set; }
            = new ObservableCollection<GameHistory>();
        public int Height { get; private set; }

        private List<GameHistory> cachedHistories { get; set; } = new List<GameHistory>();

        public void HandleScrollChange(object? sender, ScrollChangedEventArgs e)
        {
            ScrollViewer? sv = sender as ScrollViewer;
            if (sv == null)
            {
                Logger.EWrite("Failed to cast Object to ScrollViewer.");
                return;
            }
            // Histories.Count will always be small, around 5.
            for (int i = 0; i < Histories.Count; i++)
                // if this History is not visible to the user, remove it.
                if (Histories[i].Position + GameHistory.Height < (int)sv.Offset.Y
                        || Histories[i].Position > (int)(sv.Offset.Y + sv.Viewport.Height))
                    Histories.RemoveAt(i);
            // @@Optimise: If this is laggy, we can optimise this. This doesn't
            // need to be a loop, we can calculate which histories should be
            // visible using the information in GameHistory struct.
            for (int i = 0; i < cachedHistories.Count; i++)
                if (cachedHistories[i].Position + GameHistory.Height > (int)sv.Offset.Y
                        && cachedHistories[i].Position < (int)(sv.Offset.Y + sv.Viewport.Height))
                {
                    bool isInHistories = false;
                    // Don't add to Histories if cachedHistories[i] is already in Histories.
                    for (int histIndex = 0; histIndex < Histories.Count; histIndex++)
                        if (cachedHistories[i].Position == Histories[histIndex].Position)
                            isInHistories = true;
                    if (!isInHistories)
                        Histories.Add(cachedHistories[i]);
                }
        }

        public void LoadHistory(object? sender, PointerReleasedEventArgs e)
        {
            Logger.DWrite("Load History");
        }
    }
}
