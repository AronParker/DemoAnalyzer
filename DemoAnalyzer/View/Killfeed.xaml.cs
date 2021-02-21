using DemoAnalyzer.Data;
using DemoAnalyzer.Tools;
using DemoInfo;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DemoAnalyzer
{
    /// <summary>
    /// Interaction logic for Killfeed.xaml
    /// </summary>
    public partial class Killfeed : UserControl
    {
        private StackPanel[] _deathNotices;
        private TextBlock[] _killers;
        private Image[] _weapons;
        private TextBlock[] _victims;
        
        public Killfeed()
        {
            InitializeComponent();

            _deathNotices = new[] { dn1, dn2, dn3, dn4, dn5 };
            _killers = new[] { dn1Killer, dn2Killer, dn3Killer, dn4Killer, dn5Killer };
            _weapons = new[] { dn1Weapon, dn2Weapon, dn3Weapon, dn4Weapon, dn5Weapon };
            _victims = new[] { dn1Victim, dn2Victim, dn3Victim, dn4Victim, dn5Victim };

            Reset();
        }

        public void Reset()
        {
            foreach (var dn in _deathNotices)
                dn.Visibility = Visibility.Hidden;
        }

        public void SetKills(IEnumerable<PlayerKill> kills)
        {
            var index = 0;

            foreach (var kill in kills)
            {
                _deathNotices[index].Visibility = Visibility.Visible;

                if (kill.AssisterName != null)
                    _killers[index].Text = $"{kill.KillerName} + {kill.AssisterName}";
                else if (kill.KillerName != null)
                    _killers[index].Text = kill.KillerName;
                else
                    _killers[index].Text = "";

                switch (kill.KillerTeam)
                {
                    case Team.Terrorist:
                        _killers[index].Foreground = Brushes.IndianRed;
                        break;
                    case Team.CounterTerrorist:
                        _killers[index].Foreground = Brushes.LightSteelBlue;
                        break;
                }

                _weapons[index].Source = Assets.GetDeathNoticeWeaponIcon(kill.Weapon);

                _victims[index].Text = kill.VictimName;

                switch (kill.VictimTeam)
                {
                    case Team.Terrorist:
                        _victims[index].Foreground = Brushes.IndianRed;
                        break;
                    case Team.CounterTerrorist:
                        _victims[index].Foreground = Brushes.LightSteelBlue;
                        break;
                }

                index++;
            }

            while (index < _deathNotices.Length)
                _deathNotices[index++].Visibility = Visibility.Hidden;
        }
    }
}
