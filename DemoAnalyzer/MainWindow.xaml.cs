using DemoAnalyzer.Data;
using DemoInfo;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace DemoAnalyzer
{
    public class PlayerListViewItem : INotifyPropertyChanged
    {
        private string _name = "";
        private Team _team = Team.Spectate;
        private bool _dead = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        { 
            get => _name; 
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Team Team 
        { 
            get => _team;
            set
            {
                if (_team != value)
                {
                    _team = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Dead
        { 
            get => _dead;
            set
            {
                if (_dead != value)
                {
                    _dead = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DemoState _state = new DemoState();
        private DemoData _data = new DemoData();
        private ObservableCollection<PlayerListViewItem> _playerInfos = new ObservableCollection<PlayerListViewItem>();
        //private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            
            playersLV.ItemsSource = _playerInfos;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(playersLV.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Team");
            view.GroupDescriptions.Add(groupDescription);

            _playerInfos.Add(new PlayerListViewItem() { Name = "Aron", Team = Team.Spectate, Dead = false });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            _playerInfos[0].Name = "Pon";

            return;
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

                    _data.Parse(parser);
                    _state.Parse(parser);

                    timeline.Init(_state.MinTick, _state.MaxTick, _state.Rounds);
                }
            }
        }

        private void timeline_PlaybackPositionChanged(object sender, EventArgs e)
        {
            //_playerInfos.Clear();
            //
            //foreach (var p in _state.ReadPlayerStates(timeline.PlaybackPosition))
            //    _playerInfos.Add(p);
            //
            //minimap.SetPlayers(_playerInfos);
            //killfeed.SetKills(_state.ReadRecentKills(timeline.PlaybackPosition));
        }
    }
}
