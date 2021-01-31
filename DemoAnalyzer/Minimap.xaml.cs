using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemoAnalyzer
{
    /// <summary>
    /// Interaction logic for Minimap.xaml
    /// </summary>
    public partial class Minimap : UserControl
    {
        private static CombinedGeometry s_cross;

        private double _minimapPosX;
        private double _minimapPosY;
        private double _minimapScale;

        private List<PlayerRenderInfo> _renderInfos = new List<PlayerRenderInfo>();

        static Minimap()
        {
            s_cross = new CombinedGeometry(GeometryCombineMode.Union,
                new RectangleGeometry(new Rect(-7.5, -1.5, 15, 3)),
                new RectangleGeometry(new Rect(-1.5, -7.5, 3, 15)),
                new RotateTransform(45));

            if (s_cross.CanFreeze)
                s_cross.Freeze();
        }

        public Minimap()
        {
            InitializeComponent();
        }

        public bool LoadMap(string mapName)
        {
            switch (mapName)
            {
                case "de_cache":
                    _minimapPosX = -2000;
                    _minimapPosY = 3250;
                    _minimapScale = 5.5;
                    break;
                case "de_dust2":
                    _minimapPosX = -2476;
                    _minimapPosY = 3239;
                    _minimapScale = 4.4;
                    break;
                case "de_inferno":
                    _minimapPosX = -2087;
                    _minimapPosY = 3870;
                    _minimapScale = 4.9;
                    break;
                case "de_mirage":
                    _minimapPosX = -3230;
                    _minimapPosY = 1713;
                    _minimapScale = 5.00;
                    break;
                case "de_nuke":
                    _minimapPosX = -3453;
                    _minimapPosY = 2887;
                    _minimapScale = 7;
                    break;
                case "de_train":
                    _minimapPosX = -2477;
                    _minimapPosY = 2392;
                    _minimapScale = 4.7;
                    break;
                case "de_vertigo":
                    _minimapPosX = -3168;
                    _minimapPosY = 1762;
                    _minimapScale = 4.0;
                    break;
                default:
                    return false;
            }

            canvas.Background = GetCanvasBackground(mapName);
            return true;
        }

        public void SetPlayers(IEnumerable<PlayerInfo> players)
        {
            var i = 0;

            foreach (var player in players)
            {
                var info = GetRenderInfo(i++);
                var state = player.State;
                var playerPos = WorldSpaceToScreenSpace(new System.Windows.Vector(state.PositionX, state.PositionY));

                if (state.IsAlive)
                {
                    info.PlayerPos.Visibility = Visibility.Visible;
                    info.PlayerSight.Visibility = Visibility.Visible;
                    info.DeathPos.Visibility = Visibility.Hidden;

                    info.PlayerPos.Fill = state.IsTerrorist ? Brushes.IndianRed : Brushes.DodgerBlue;
                    info.PlayerPos.Stroke = state.IsTerrorist ? Brushes.DarkRed : Brushes.DarkBlue;

                    Canvas.SetLeft(info.PlayerPos, playerPos.X - 15 / 2);
                    Canvas.SetTop(info.PlayerPos, playerPos.Y - 15 / 2);

                    var left = playerPos + CreateVectorFromRotation(128, state.ViewDirectionX - 45f);
                    var right = playerPos + CreateVectorFromRotation(128, state.ViewDirectionX + 45f);
                    var center = (left + right) / 2;

                    info.playerSightBrush.StartPoint = new Point(playerPos.X, playerPos.Y);
                    info.playerSightBrush.EndPoint = new Point(center.X, center.Y);

                    info.PlayerSight.Points[0] = new Point(playerPos.X, playerPos.Y);
                    info.PlayerSight.Points[1] = new Point(left.X, left.Y);
                    info.PlayerSight.Points[2] = new Point(right.X, right.Y);
                }
                else
                {
                    info.PlayerPos.Visibility = Visibility.Hidden;
                    info.PlayerSight.Visibility = Visibility.Hidden;
                    info.DeathPos.Visibility = Visibility.Visible;

                    info.DeathPos.Fill = state.IsTerrorist ? Brushes.IndianRed : Brushes.DodgerBlue;
                    info.DeathPos.Stroke = state.IsTerrorist ? Brushes.DarkRed : Brushes.DarkBlue;

                    Canvas.SetLeft(info.DeathPos, playerPos.X);
                    Canvas.SetTop(info.DeathPos, playerPos.Y);
                }
            }

            if (i < _renderInfos.Count)
                RemoveUnusedRenderInfos(i);
        }

        private PlayerRenderInfo GetRenderInfo(int index)
        {
            if (index < _renderInfos.Count)
                return _renderInfos[index];

            var result = new PlayerRenderInfo();

            result.PlayerPos = new Ellipse
            {
                Width = 15,
                Height = 15,
                Visibility = Visibility.Hidden
            };

            result.playerSightBrush = new LinearGradientBrush(Color.FromArgb(64, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 0.0)
            {
                MappingMode = BrushMappingMode.Absolute
            };

            result.PlayerSight = new Polygon
            {
                Fill = result.playerSightBrush,
                Points = new PointCollection { new Point(), new Point(), new Point() },
                Visibility = Visibility.Hidden
            };

            result.DeathPos = new Path
            {
                Data = s_cross,
                Visibility = Visibility.Hidden
            };

            canvas.Children.Add(result.PlayerPos);
            canvas.Children.Add(result.PlayerSight);
            canvas.Children.Add(result.DeathPos);

            _renderInfos.Add(result);

            return result;
        }

        private void RemoveUnusedRenderInfos(int index)
        {
            for (int j = index; j < _renderInfos.Count; j++)
            {
                canvas.Children.Remove(_renderInfos[j].PlayerPos);
                canvas.Children.Remove(_renderInfos[j].PlayerSight);
                canvas.Children.Remove(_renderInfos[j].DeathPos);
            }

            _renderInfos.RemoveRange(index, _renderInfos.Count - index);
        }

        private System.Windows.Vector WorldSpaceToScreenSpace(System.Windows.Vector worldSpace)
        {
            var distanceFromTopLeft = new System.Windows.Vector(worldSpace.X - _minimapPosX, _minimapPosY - worldSpace.Y);
            
            return distanceFromTopLeft / _minimapScale;
        }

        private static System.Windows.Vector CreateVectorFromRotation(float distance, float angle)
        {
            var radians = angle * Math.PI / 180.0;
            var newX = distance * (float)Math.Cos(radians);
            var newY = distance * (float)-Math.Sin(radians);

            return new System.Windows.Vector(newX, newY);
        }

        private static ImageBrush GetCanvasBackground(string mapName)
        {
            //$"pack://application:,,,/DemoAnalyzer;component/assets/{mapName}_radar_spectate.dds"

            var uri = new Uri($"assets/minimaps/{mapName}_radar_spectate.dds", UriKind.Relative);
            return new ImageBrush(new BitmapImage(uri));
        }

        private struct PlayerRenderInfo
        {
            public Ellipse PlayerPos;
            public Polygon PlayerSight;
            public LinearGradientBrush playerSightBrush;
            public Path DeathPos;
        }
    }
}
