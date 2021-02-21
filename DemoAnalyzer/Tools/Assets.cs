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
    }
}
