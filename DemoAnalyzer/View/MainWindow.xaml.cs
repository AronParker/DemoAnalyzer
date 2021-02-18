using DemoAnalyzer.Data;
using DemoAnalyzer.Tools;
using DemoAnalyzer.ViewModel;
using DemoInfo;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DemoAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DemoData _demo = new DemoData();
        private ObservableCollection<PlayerListViewItem> _playerList = new ObservableCollection<PlayerListViewItem>();
        private HashSet<int> _selectedPlayers = new HashSet<int>();
        private DispatcherTimer _playTimer = new DispatcherTimer(DispatcherPriority.Send);
        private int _ticksPerSecond = 64;

        private ImageSource _playImageSource;
        private ImageSource _pauseImageSource;

        public MainWindow()
        {         
            InitializeComponent();
            
            playersLV.ItemsSource = _playerList;
            minimap.SelectedPlayers = _selectedPlayers;

            _playImageSource = new BitmapImage(new Uri($"assets/icons/play.png", UriKind.Relative));
            _pauseImageSource = new BitmapImage(new Uri($"assets/icons/pause.png", UriKind.Relative));
            
            _playTimer.Tick += _playTimer_Tick;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(playersLV.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Team");
            view.GroupDescriptions.Add(groupDescription);
        }

        private void _playTimer_Tick(object sender, EventArgs e)
        {
            if (timeline.LastTick == 0)
                return;

            int currentTick = timeline.PlaybackPosition;
            int nextTick = currentTick + 1;

            if (nextTick <= timeline.LastTick)
            {
                timeline.PlaybackPosition = nextTick;
            }
            else
            {
                _playTimer.Stop();
                playImage.Source = _pauseImageSource;
            }
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Demo files (*.dem)|*.dem";
            ofd.FileName = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\broadcast.dem";

            if (ofd.ShowDialog(this) == true)
            {
                using (var parser = new DemoParser(ofd.OpenFile()))
                {
                    parser.ParseHeader();

                    try
                    {
                        _demo.Parse(parser);
                    }
                    catch (DemoDataException ex)
                    {
                        MessageBox.Show($"Failed to load demo: {ex.Message}", "Unable to load demo", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    minimap.LoadMap(parser.Header.MapName);
                    timeline.Init(_demo.Rounds, _demo.LastTick);
                }
            }
        }

        private void timeline_PlaybackPositionChanged(object sender, EventArgs e)
        {
            var text = "";

            BeginPlayerListUpdate();
            minimap.BeginUpdate();

            foreach (var playerInfo in _demo.ReadPlayerInfos(timeline.PlaybackPosition))
            {
                minimap.UpdatePlayer(playerInfo, _demo);
                UpdatePlayerListPlayer(playerInfo);

                if (_selectedPlayers.Contains(playerInfo.EntityID))
                {
                    text += $@"{playerInfo.State.Name} Statistics:

Kills: {playerInfo.Statistics.Kills}
Deaths: {playerInfo.Statistics.Deaths}
K/D Ratio: {(double)playerInfo.Statistics.Kills / (double)playerInfo.Statistics.Deaths:F2}
Assists: {playerInfo.Statistics.Assists}
Score: {playerInfo.Statistics.Score}
MVPs: {playerInfo.Statistics.MVPs}
Ping: {playerInfo.Statistics.Ping}
Clantag: {playerInfo.Statistics.Clantag}
TotalCashSpent: {playerInfo.Statistics.TotalCashSpent}

";
                }
            }

            EndPlayerListUpdate();
            minimap.EndUpdate();

            if (text == "")
            {
                playerInfos.Text = "Click on a player to view more information about them.";
            }
            else
            {
                playerInfos.Text = text;
            }

            killfeed.SetKills(_demo.ReadRecentKills(timeline.PlaybackPosition, 5));
        }

        private void BeginPlayerListUpdate()
        {
            foreach (var player in _playerList)
                player.Used = false;
        }

        private void UpdatePlayerListPlayer(Data.PlayerInfo playerInfo)
        {
            var idx = FindPlayerListIndex(playerInfo.EntityID);

            if (idx == -1)
            {
                var newPlayer = new PlayerListViewItem(playerInfo.EntityID, playerInfo.State.Name, playerInfo.State.Team, playerInfo.State.Team != Team.Spectate && !playerInfo.State.IsAlive)
                {
                    Used = true
                };

                _playerList.Add(newPlayer);
            }
            else
            {
                var existingPlayer = _playerList[idx];

                existingPlayer.Name = playerInfo.State.Name;

                if (existingPlayer.Team != playerInfo.State.Team)
                {
                    existingPlayer.Team = playerInfo.State.Team;
                    ICollectionView view = CollectionViewSource.GetDefaultView(playersLV.ItemsSource);
                    view.Refresh();
                } 

                existingPlayer.Dead = playerInfo.State.Team != Team.Spectate && !playerInfo.State.IsAlive;
                existingPlayer.Used = true;
            }        
        }

        private int FindPlayerListIndex(int entityId)
        {
            for (int i = 0; i < _playerList.Count; i++)
                if (_playerList[i].EntityID == entityId)
                    return i;

            return -1;
        }

        private void EndPlayerListUpdate()
        {
            for (int i = _playerList.Count - 1; i >= 0; i--)
                if (!_playerList[i].Used)
                    _playerList.RemoveAt(i);
        }

        private void playersLV_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (var added in e.AddedItems)
            {
                var entityID = ((PlayerListViewItem)added).EntityID;
                _selectedPlayers.Add(entityID);
            }

            foreach (var removed in e.RemovedItems)
            {
                var entityID = ((PlayerListViewItem)removed).EntityID;
                _selectedPlayers.Remove(entityID);
            }

            timeline_PlaybackPositionChanged(this, EventArgs.Empty);
        }

        private void SkipPrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (timeline.LastTick == 0)
                return;

            var currentTick = timeline.PlaybackPosition;
            var selectedRound = _demo.Rounds.LastOrDefault(x => x.StartTick < currentTick);

            if (selectedRound.StartTick != 0)
                timeline.PlaybackPosition = selectedRound.StartTick;
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playTimer.IsEnabled)
            {
                _playTimer.Stop();
                playImage.Source = _playImageSource;
            }
            else
            {
                _playTimer.Interval = TimeSpan.FromSeconds(1.0 / _ticksPerSecond);
                _playTimer.Start();

                playImage.Source = _pauseImageSource;
            }
        }

        private void SkipNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (timeline.LastTick == 0)
                return;

            var currentTick = timeline.PlaybackPosition;
            var selectedRound = _demo.Rounds.FirstOrDefault(x => x.StartTick > currentTick);

            if (selectedRound.StartTick != 0)
                timeline.PlaybackPosition = selectedRound.StartTick;
        }

        private void HeatmapButton_Click(object sender, RoutedEventArgs e)
        {
            var selectionStart = timeline.SelectionStart;
            var selectionEnd = timeline.SelectionEnd;

            if (selectionStart == -1 || selectionEnd == -1)
            {
                MessageBox.Show("You must make a selection in the timeline. You can do so by holding your left mouse button and dragging it over the timeline.", "Heatmap generation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedPlayers = playersLV.SelectedItems.Cast<PlayerListViewItem>().Select(x => x.EntityID).ToHashSet();

            if (selectedPlayers.Count == 0)
            {
                MessageBox.Show("No players selected. You must at least select one player to make a heatmap.", "Heatmap generation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var cts = new CancellationTokenSource())
            {
                var a = new HeatmapWindow(cts, _demo, selectedPlayers, selectionStart, selectionEnd);
                var b = a.ShowDialog();
            }

            /*var a = new Heatmap(128, 128);
            var rnd = new Random();

            a.AddPoint(64 - 32, 64 - 32);

            var bmp = a.CreateHeatmap();
            var c = new PngBitmapEncoder();
            c.Frames.Add(BitmapFrame.Create(bmp));

            using (var fs = File.OpenWrite("test.png"))
                c.Save(fs);

            Process.Start("test.png");*/
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
