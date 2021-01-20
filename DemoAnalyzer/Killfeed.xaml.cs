using DemoInfo;
using System;
using System.Collections.Generic;
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

            foreach (var dn in _deathNotices)
                dn.Visibility = Visibility.Hidden;
        }

        public void SetKills(IEnumerable<PlayerKilledEventArgs> kills)
        {
            var index = 0;

            foreach (var kill in kills)
            {
                _deathNotices[index].Visibility = Visibility.Visible;

                if (kill.Assister != null)
                    _killers[index].Text = $"{kill.Killer.Name} + {kill.Assister.Name}";
                else
                    _killers[index].Text = kill.Killer.Name;

                switch (kill.Killer.Team)
                {
                    case Team.Terrorist:
                        _killers[index].Foreground = Brushes.IndianRed;
                        break;
                    case Team.CounterTerrorist:
                        _killers[index].Foreground = Brushes.LightSteelBlue;
                        break;
                }

                _weapons[index].Source = GetWeaponIcon(kill.Weapon);

                _victims[index].Text = kill.Victim.Name;

                switch (kill.Victim.Team)
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

        private BitmapImage GetWeaponIcon(Equipment weapon)
        {
            return new BitmapImage(new Uri(GetWeaponPath(weapon), UriKind.Relative));
        }
        
        private static string GetWeaponPath(Equipment weapon)
        {
            switch (weapon.Weapon)
            {
                case EquipmentElement.Unknown:
                    return @"assets\deathnotice\icon-suicide.png";
                case EquipmentElement.P2000:
                    return @"assets\deathnotice\icon-hkp2000.png";
                case EquipmentElement.Glock:
                    return @"assets\deathnotice\icon-glock.png";
                case EquipmentElement.P250:
                    return @"assets\deathnotice\icon-p250.png";
                case EquipmentElement.Deagle:
                    return @"assets\deathnotice\icon-deagle.png";
                case EquipmentElement.FiveSeven:
                    return @"assets\deathnotice\icon-fiveseven.png";
                case EquipmentElement.DualBarettas:
                    return @"assets\deathnotice\icon-elite.png";
                case EquipmentElement.Tec9:
                    return @"assets\deathnotice\icon-tec9.png";
                case EquipmentElement.CZ:
                    return @"assets\deathnotice\icon-cz75a.png";
                case EquipmentElement.USP:
                    return @"assets\deathnotice\icon-usp.png";
                case EquipmentElement.Revolver:
                    return @"assets\deathnotice\icon-revolver.png";
                case EquipmentElement.MP7:
                    return @"assets\deathnotice\icon-mp7.png";
                case EquipmentElement.MP9:
                    return @"assets\deathnotice\icon-mp9.png";
                case EquipmentElement.Bizon:
                    return @"assets\deathnotice\icon-bizon.png";
                case EquipmentElement.Mac10:
                    return @"assets\deathnotice\icon-mac10.png";
                case EquipmentElement.UMP:
                    return @"assets\deathnotice\icon-ump45.png";
                case EquipmentElement.P90:
                    return @"assets\deathnotice\icon-p90.png";
                case EquipmentElement.MP5SD:
                    return @"assets\deathnotice\icon-mp5sd.png";
                case EquipmentElement.SawedOff:
                    return @"assets\deathnotice\icon-sawedoff.png";
                case EquipmentElement.Nova:
                    return @"assets\deathnotice\icon-nova.png";
                case EquipmentElement.XM1014:
                    return @"assets\deathnotice\icon-xm1014.png";
                case EquipmentElement.M249:
                    return @"assets\deathnotice\icon-m249.png";
                case EquipmentElement.Negev:
                    return @"assets\deathnotice\icon-negev.png";
                case EquipmentElement.Gallil:
                    return @"assets\deathnotice\icon-galilar.png";
                case EquipmentElement.Famas:
                    return @"assets\deathnotice\icon-famas.png";
                case EquipmentElement.AK47:
                    return @"assets\deathnotice\icon-ak47.png";
                case EquipmentElement.M4A4:
                    return @"assets\deathnotice\icon-m4a1.png";
                case EquipmentElement.M4A1:
                    return @"assets\deathnotice\icon-m4a1.png";
                case EquipmentElement.Scout:
                    return @"assets\deathnotice\icon-scout.png";
                case EquipmentElement.SG556:
                    return @"assets\deathnotice\icon-sg556.png";
                case EquipmentElement.AUG:
                    return @"assets\deathnotice\icon-aug.png";
                case EquipmentElement.AWP:
                    return @"assets\deathnotice\icon-awp.png";
                case EquipmentElement.Scar20:
                    return @"assets\deathnotice\icon-scar20.png";
                case EquipmentElement.G3SG1:
                    return @"assets\deathnotice\icon-g3sg1.png";
                case EquipmentElement.Zeus:
                    return @"assets\deathnotice\icon-taser.png";
                case EquipmentElement.Knife:
                    return @"assets\deathnotice\icon-knife.png";
                case EquipmentElement.World:
                    return @"assets\deathnotice\icon-suicide.png";
                case EquipmentElement.Decoy:
                    return @"assets\deathnotice\icon-decoy.png";
                case EquipmentElement.Molotov:
                    return @"assets\deathnotice\icon-molotov.png";
                case EquipmentElement.Incendiary:
                    return @"assets\deathnotice\icon-incgrenade_impact.png";
                case EquipmentElement.Flash:
                    return @"assets\deathnotice\icon-flashbang_impact.png";
                case EquipmentElement.Smoke:
                    return @"assets\deathnotice\icon-smokegrenade_impact.png";
                case EquipmentElement.HE:
                    return @"assets\deathnotice\icon-hegrenade.png";
                default:
                    return @"assets\deathnotice\icon-suicide.png";
            }
        }
        
    }
}
