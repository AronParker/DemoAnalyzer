using DemoInfo;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static DemoAnalyzer.DemoState;
using Vector = System.Windows.Vector;

namespace DemoAnalyzer
{
    public class TeamToColorConverter  : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var team = (Team)value;

            switch (parameter)
            {
                case "headline":
                    {
                        switch (team)
                        {
                            case Team.Terrorist:
                                return Brushes.DarkRed;
                            case Team.CounterTerrorist:
                                return Brushes.DarkBlue;
                            default:
                                throw new Exception("Unsupported team");
                        }
                    }
                case "body":
                    {
                        switch (team)
                        {
                            case Team.Terrorist:
                                return Brushes.IndianRed;
                            case Team.CounterTerrorist:
                                return Brushes.LightBlue;
                            default:
                                throw new Exception("Unsupported team");
                        }
                    }
                default:
                    throw new Exception("Unsupported parameter");
            }

        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DemoState _state = new DemoState();
        private ObservableCollection<PlayerInfo> _playerInfos = new ObservableCollection<PlayerInfo>();
        //private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            playersLV.ItemsSource = _playerInfos;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(playersLV.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Team");
            view.GroupDescriptions.Add(groupDescription);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
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

                    _state.Parse(parser);

                    timeline.Init(_state.MinTick, _state.MaxTick, _state.Rounds);
                }
            }
        }

        private void timeline_PlaybackPositionChanged(object sender, EventArgs e)
        {
            _playerInfos.Clear();

            foreach (var p in _state.ReadPlayerStates(timeline.PlaybackPosition))
                _playerInfos.Add(p);
            
            minimap.SetPlayers(_playerInfos);

        }
    }
}
