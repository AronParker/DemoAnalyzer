using DemoInfo;
using Microsoft.Win32;
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
using System.Windows.Threading;
using Vector = System.Windows.Vector;

namespace DemoAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DemoParser _parser;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Demo files (*.dem)|*.dem";
            ofd.FileName = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\broadcast.dem";

            if (ofd.ShowDialog(this) == true)
            {
                _parser = new DemoParser(ofd.OpenFile());
                _parser.ParseHeader();

                _timer = new DispatcherTimer(DispatcherPriority.Normal);
                _timer.Interval = TimeSpan.FromSeconds(1.0 / 60.0);
                _timer.Tick += _timer_Tick;
                _timer.Start();
            }
        }

        class RenderInfo
        {
            public static CombinedGeometry cross;

            public Ellipse e;
            public Polygon poly;
            public LinearGradientBrush lgb;
            public TextBlock death;
            public Path death2;
            public bool alive;

            static RenderInfo()
            {
                cross = new CombinedGeometry(GeometryCombineMode.Union,
                    new RectangleGeometry(new Rect(-7.5, -1.5, 15, 3)),
                    new RectangleGeometry(new Rect(-1.5, -7.5, 3, 15)),
                    new RotateTransform(45));
            }

            public RenderInfo(Player p)
            {
                e = new Ellipse()
                {
                    Width = 15,
                    Height = 15,
                    Fill = p.Team == Team.Terrorist ? Brushes.IndianRed : Brushes.LightSteelBlue,
                    Stroke = p.Team == Team.Terrorist ? Brushes.DarkRed : Brushes.DarkBlue
                };

                lgb = new LinearGradientBrush(Color.FromArgb(64, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 0.0)
                {
                    MappingMode = BrushMappingMode.Absolute
                };

                poly = new Polygon
                {
                    Fill = lgb,
                    Points = new PointCollection { new Point(), new Point(), new Point() }
                };

                death = new TextBlock();
                death.Text = "☠";
                death.FontSize = 20;
                death.Foreground = p.Team == Team.Terrorist ? Brushes.IndianRed : Brushes.LightSteelBlue;
                death.Visibility = Visibility.Hidden;
                death.HorizontalAlignment = HorizontalAlignment.Center;
                death.VerticalAlignment = VerticalAlignment.Center;
                alive = true;
            }
        }

        Dictionary<Player, RenderInfo> renderInfos = new Dictionary<Player, RenderInfo>();

        private void _timer_Tick(object sender, EventArgs e)
        {
            _parser.ParseNextTick();

            foreach (var player in _parser.PlayingParticipants)
            {
                RenderInfo info;

                if (!renderInfos.TryGetValue(player, out info))
                {
                    info = new RenderInfo(player);
                    canvas.Children.Add(info.e);
                    canvas.Children.Add(info.poly);
                    canvas.Children.Add(info.death);
                    renderInfos.Add(player, info);
                }

                if (info.alive != player.IsAlive)
                {
                    if (player.IsAlive)
                    {
                        info.e.Visibility = Visibility.Visible;
                        info.poly.Visibility = Visibility.Visible; 
                        info.death.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        info.e.Visibility = Visibility.Hidden;
                        info.poly.Visibility = Visibility.Hidden;
                        info.death.Visibility = Visibility.Visible;
                    }

                    info.alive = player.IsAlive;
                }

                if (info.alive)
                {
                    var screenSpace = WorldSpaceToScreenSpace(player.Position);
                    var screenSpaceActual = ScreenSpaceToCanvasSpze(screenSpace, image.ActualWidth);

                    var left = screenSpaceActual + CreateVectorFromRotation((float)image.ActualWidth / 10, player.ViewDirectionX - 45f);
                    var right = screenSpaceActual + CreateVectorFromRotation((float)image.ActualWidth / 10, player.ViewDirectionX + 45f);
                    var center = (left + right) / 2;

                    Canvas.SetLeft(info.e, screenSpaceActual.X - info.e.ActualWidth / 2);
                    Canvas.SetTop(info.e, screenSpaceActual.Y - info.e.ActualHeight / 2);

                    info.lgb.StartPoint = new Point(screenSpaceActual.X, screenSpaceActual.Y);
                    info.lgb.EndPoint = new Point(center.X, center.Y);

                    info.poly.Points[0] = new Point(screenSpaceActual.X, screenSpaceActual.Y);
                    info.poly.Points[1] = new Point(left.X, left.Y);
                    info.poly.Points[2] = new Point(right.X, right.Y);
                }
                else
                {
                    var screenSpace = WorldSpaceToScreenSpace(player.LastAlivePosition);
                    var screenSpaceActual = ScreenSpaceToCanvasSpze(screenSpace, image.ActualWidth);

                    Canvas.SetLeft(info.death, screenSpaceActual.X - info.death.ActualWidth / 2);
                    Canvas.SetTop(info.death, screenSpaceActual.Y - info.death.ActualHeight / 2);
                }

            }
        }

        Vector WorldSpaceToScreenSpace(DemoInfo.Vector worldSpace)
        {
            var minimapPosX = -2476f;
            var minimapPosY = 3239f;
            var minimapScale = 4.4f;

            var distanceFromTopLeft = new Vector(worldSpace.X - minimapPosX, minimapPosY - worldSpace.Y);

            return distanceFromTopLeft / minimapScale;
        }

        Vector ScreenSpaceToCanvasSpze(Vector screenSpace, double canvasSize)
        {
            var screenSpaceNorm = screenSpace / 1024.0;
            var screenSpaceActual = screenSpaceNorm * canvasSize;
            return screenSpaceActual;
        }

        Vector CreateVectorFromRotation(float distance, float angle)
        {
            var radians = angle * Math.PI / 180.0;
            var newX = distance * (float)Math.Cos(radians);
            var newY = distance * (float)-Math.Sin(radians);

            return new Vector(newX, newY);
        }
    }
}
