using DemoInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DemoAnalyzer.Tools
{
    public static class Assets
    {
        public static BitmapImage GetMinimap(string mapName)
        {
            // If minimaps are embedded as resource, the Uri is as follows:
            // $"pack://application:,,,/DemoAnalyzer;component/assets/{mapName}_radar_spectate.dds"
            
            var uri = new Uri($"{AppDomain.CurrentDomain.BaseDirectory}assets/minimaps/{mapName}_radar_spectate.dds", UriKind.Absolute);
            return new BitmapImage(uri);
        }

        public static BitmapImage GetDeathNoticeWeaponIcon(Equipment weapon)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}assets/deathnotice/";

            switch (weapon.Weapon)
            {
                case EquipmentElement.Unknown:
                    path += "icon-suicide.png";
                    break;
                case EquipmentElement.P2000:
                    path += "icon-hkp2000.png";
                    break;
                case EquipmentElement.Glock:
                    path += "icon-glock.png";
                    break;
                case EquipmentElement.P250:
                    path += "icon-p250.png";
                    break;
                case EquipmentElement.Deagle:
                    path += "icon-deagle.png";
                    break;
                case EquipmentElement.FiveSeven:
                    path += "icon-fiveseven.png";
                    break;
                case EquipmentElement.DualBarettas:
                    path += "icon-elite.png";
                    break;
                case EquipmentElement.Tec9:
                    path += "icon-tec9.png";
                    break;
                case EquipmentElement.CZ:
                    path += "icon-cz75a.png";
                    break;
                case EquipmentElement.USP:
                    path += "icon-usp.png";
                    break;
                case EquipmentElement.Revolver:
                    path += "icon-revolver.png";
                    break;
                case EquipmentElement.MP7:
                    path += "icon-mp7.png";
                    break;
                case EquipmentElement.MP9:
                    path += "icon-mp9.png";
                    break;
                case EquipmentElement.Bizon:
                    path += "icon-bizon.png";
                    break;
                case EquipmentElement.Mac10:
                    path += "icon-mac10.png";
                    break;
                case EquipmentElement.UMP:
                    path += "icon-ump45.png";
                    break;
                case EquipmentElement.P90:
                    path += "icon-p90.png";
                    break;
                case EquipmentElement.MP5SD:
                    path += "icon-mp5sd.png";
                    break;
                case EquipmentElement.SawedOff:
                    path += "icon-sawedoff.png";
                    break;
                case EquipmentElement.Nova:
                    path += "icon-nova.png";
                    break;
                case EquipmentElement.XM1014:
                    path += "icon-xm1014.png";
                    break;
                case EquipmentElement.M249:
                    path += "icon-m249.png";
                    break;
                case EquipmentElement.Negev:
                    path += "icon-negev.png";
                    break;
                case EquipmentElement.Gallil:
                    path += "icon-galilar.png";
                    break;
                case EquipmentElement.Famas:
                    path += "icon-famas.png";
                    break;
                case EquipmentElement.AK47:
                    path += "icon-ak47.png";
                    break;
                case EquipmentElement.M4A4:
                    path += "icon-m4a1.png";
                    break;
                case EquipmentElement.M4A1:
                    path += "icon-m4a1.png";
                    break;
                case EquipmentElement.Scout:
                    path += "icon-scout.png";
                    break;
                case EquipmentElement.SG556:
                    path += "icon-sg556.png";
                    break;
                case EquipmentElement.AUG:
                    path += "icon-aug.png";
                    break;
                case EquipmentElement.AWP:
                    path += "icon-awp.png";
                    break;
                case EquipmentElement.Scar20:
                    path += "icon-scar20.png";
                    break;
                case EquipmentElement.G3SG1:
                    path += "icon-g3sg1.png";
                    break;
                case EquipmentElement.Zeus:
                    path += "icon-taser.png";
                    break;
                case EquipmentElement.Knife:
                    path += "icon-knife.png";
                    break;
                case EquipmentElement.World:
                    path += "icon-suicide.png";
                    break;
                case EquipmentElement.Decoy:
                    path += "icon-decoy.png";
                    break;
                case EquipmentElement.Molotov:
                    path += "icon-molotov.png";
                    break;
                case EquipmentElement.Incendiary:
                    path += "icon-incgrenade_impact.png";
                    break;
                case EquipmentElement.Flash:
                    path += "icon-flashbang_impact.png";
                    break;
                case EquipmentElement.Smoke:
                    path += "icon-smokegrenade_impact.png";
                    break;
                case EquipmentElement.HE:
                    path += "icon-hegrenade.png";
                    break;
                default:
                    path += "icon-suicide.png";
                    break;
            }

            return new BitmapImage(new Uri(path, UriKind.Absolute));
        }
    }
}
