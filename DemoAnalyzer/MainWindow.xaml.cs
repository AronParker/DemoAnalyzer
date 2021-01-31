using DemoAnalyzer.Data;
using DemoAnalyzer.ViewModel;
using DemoInfo;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
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
        private DispatcherTimer _playTimer;

        public MainWindow()
        {
            InitializeComponent();

            playersLV.ItemsSource = _playerList;
            minimap.SelectedPlayers = _selectedPlayers;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(playersLV.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Team");
            view.GroupDescriptions.Add(groupDescription);
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

                    if (!minimap.LoadMap(parser.Header.MapName))
                    {
                        MessageBox.Show("The demo file selected is played on a map which is not yet supported by the Demo Analyzer.", "Map not supported", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _demo.Parse(parser);

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
                minimap.UpdatePlayer(playerInfo);
                UpdatePlayerListPlayer(playerInfo);

                if (_selectedPlayers.Contains(playerInfo.EntityID))
                {
                    text += $@"{playerInfo.State.Name} Statistics:

Kills: {playerInfo.Statistics.Kills}
Deaths: {playerInfo.Statistics.Deaths}
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
    }
}
