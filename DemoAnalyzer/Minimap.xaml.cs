using DemoAnalyzer.Data;
using DemoInfo;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private Dictionary<int, PlayerRenderInfo> _renderInfos = new Dictionary<int, PlayerRenderInfo>();

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

        public HashSet<int> SelectedPlayers { get; set; }

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

        public void BeginUpdate()
        {
            ResetUsedRenderInfos();
        }

        public bool SelectPlayer(int entityId, bool selected)
        {
            if (!_renderInfos.TryGetValue(entityId, out var renderInfo))
                return false;

            renderInfo.Selected = selected;

            var fillColor = GetFillColor(selected, renderInfo.Team);
            var strokeColor = GetStrokeColor(selected, renderInfo.Team);

            renderInfo.PlayerPos.Fill = fillColor;
            renderInfo.PlayerPos.Stroke = strokeColor;
            renderInfo.DeathPos.Fill = fillColor;
            renderInfo.DeathPos.Stroke = strokeColor;

            return true;
        }

        public void UpdatePlayer(Data.PlayerInfo player)
        {
            var isSpectator = player.State.Team <= DemoInfo.Team.Spectate;

            if (isSpectator)
                return;

            var renderInfo = GetOrCreateRenderInfo(player.EntityID);
            var playerPos = WorldSpaceToScreenSpace(new System.Windows.Vector(player.Position.PositionX, player.Position.PositionY));

            if (player.State.IsAlive)
            {
                renderInfo.PlayerPos.Visibility = Visibility.Visible;
                renderInfo.PlayerSight.Visibility = Visibility.Visible;
                renderInfo.DeathPos.Visibility = Visibility.Hidden;

                renderInfo.Team = player.State.Team;
                renderInfo.Selected = SelectedPlayers.Contains(player.EntityID);

                var fillColor = GetFillColor(renderInfo.Selected, renderInfo.Team);
                var strokeColor = GetStrokeColor(renderInfo.Selected, renderInfo.Team);

                renderInfo.PlayerPos.Fill = fillColor;
                renderInfo.PlayerPos.Stroke = strokeColor;

                Canvas.SetLeft(renderInfo.PlayerPos, playerPos.X - 15 / 2);
                Canvas.SetTop(renderInfo.PlayerPos, playerPos.Y - 15 / 2);

                var left = playerPos + CreateVectorFromRotation(128, player.Position.ViewDirectionX - 45f);
                var right = playerPos + CreateVectorFromRotation(128, player.Position.ViewDirectionX + 45f);
                var center = (left + right) / 2;

                renderInfo.playerSightBrush.StartPoint = new Point(playerPos.X, playerPos.Y);
                renderInfo.playerSightBrush.EndPoint = new Point(center.X, center.Y);

                renderInfo.PlayerSight.Points[0] = new Point(playerPos.X, playerPos.Y);
                renderInfo.PlayerSight.Points[1] = new Point(left.X, left.Y);
                renderInfo.PlayerSight.Points[2] = new Point(right.X, right.Y);
            }
            else
            {
                renderInfo.PlayerPos.Visibility = Visibility.Hidden;
                renderInfo.PlayerSight.Visibility = Visibility.Hidden;
                renderInfo.DeathPos.Visibility = Visibility.Visible;

                renderInfo.Team = player.State.Team;
                renderInfo.Selected = SelectedPlayers.Contains(player.EntityID);

                var fillColor = GetFillColor(renderInfo.Selected, renderInfo.Team);
                var strokeColor = GetStrokeColor(renderInfo.Selected, renderInfo.Team);

                renderInfo.DeathPos.Fill = fillColor;
                renderInfo.DeathPos.Stroke = strokeColor;

                Canvas.SetLeft(renderInfo.DeathPos, playerPos.X);
                Canvas.SetTop(renderInfo.DeathPos, playerPos.Y);
            }
        }

        public void EndUpdate()
        {
            RemoveUnusedRenderInfos();
        }

        private PlayerRenderInfo GetOrCreateRenderInfo(int entityId)
        {
            PlayerRenderInfo result;

            if (!_renderInfos.TryGetValue(entityId, out result))
            {
                result = new PlayerRenderInfo();

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
                _renderInfos.Add(entityId, result);
            }

            result.Used = true;
            return result;
        }

        private void ResetUsedRenderInfos()
        {
            foreach (var pri in _renderInfos.Values)
                pri.Used = false;
        }

        private void RemoveUnusedRenderInfos()
        {
            foreach (var renderInfo in _renderInfos)
            {
                if (!renderInfo.Value.Used)
                {
                    canvas.Children.Remove(renderInfo.Value.PlayerPos);
                    canvas.Children.Remove(renderInfo.Value.PlayerSight);
                    canvas.Children.Remove(renderInfo.Value.DeathPos);
                }
            }

            foreach (var key in _renderInfos.Where(x => !x.Value.Used).Select(x => x.Key).ToArray())
                _renderInfos.Remove(key);
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

        private static Brush GetFillColor(bool selected, Team team)
        {
            if (selected)
                return Brushes.LimeGreen;

            switch (team)
            {
                case Team.Terrorist:
                    return Brushes.IndianRed;
                case Team.CounterTerrorist:
                    return Brushes.DodgerBlue;
            }

            return Brushes.Pink;
        }

        private static Brush GetStrokeColor(bool selected, Team team)
        {
            if (selected)
                return Brushes.DarkGreen;

            switch (team)
            {
                case Team.Terrorist:
                    return Brushes.DarkRed;
                case Team.CounterTerrorist:
                    return Brushes.DarkBlue;
            }

            return Brushes.Pink;
        }

        private class PlayerRenderInfo
        {
            public Ellipse PlayerPos;
            public Polygon PlayerSight;
            public LinearGradientBrush playerSightBrush;
            public Path DeathPos;
            public Team Team;
            public bool Selected;
            public bool Used;
        }
    }
}
