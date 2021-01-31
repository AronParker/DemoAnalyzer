using DemoAnalyzer.Data;
using DemoAnalyzer.ViewModel;
using DemoInfo;
using Microsoft.Win32;
using System;
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
        private DispatcherTimer _playTimer;

        public MainWindow()
        {
            InitializeComponent();
            
            playersLV.ItemsSource = _playerList;

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
            BeginPlayerListUpdate();
            minimap.BeginUpdate();

            foreach (var playerInfo in _demo.ReadPlayerInfos(timeline.PlaybackPosition))
            {
                minimap.UpdatePlayer(playerInfo);
                UpdatePlayerListPlayer(playerInfo);
            }

            EndPlayerListUpdate();
            minimap.EndUpdate();

            //_playerInfos.Clear();
            //
            //foreach (var p in _state.ReadPlayerStates(timeline.PlaybackPosition))
            //    _playerInfos.Add(p);
            //
            //minimap.SetPlayers(_playerInfos);
            //killfeed.SetKills(_state.ReadRecentKills(timeline.PlaybackPosition));
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
    }
}
