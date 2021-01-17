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
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Timeline : UserControl
    {
        private int _minTick;
        private int _maxTick;
        private int _playbackPosition = 0;
        private int _selectionStart = 0;
        private int _selectionEnd = 0;

        public int MinTick => _minTick;
        public int MaxTick => _maxTick;

        public int PlaybackPosition
        {
            get => _playbackPosition;
            set
            {
                if (_maxTick == 0)
                    throw new InvalidOperationException();

                if (_playbackPosition != value)
                {
                    _playbackPosition = value;

                    var canvasPosition = TickToCanvasPosition(value);

                    playback.X1 = canvasPosition;
                    playback.X2 = canvasPosition;

                    PlaybackPositionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler PlaybackPositionChanged;

        public Timeline()
        {
            InitializeComponent();
        }

        public void Init(int minTick, int maxTick)
        {
            _minTick = minTick;
            _maxTick = maxTick;
            _playbackPosition = 0;

            playback.Visibility = Visibility.Visible;
            playback.X1 = 0;
            playback.X2 = 0;

            PlaybackPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Deinit()
        {
            _minTick = 0;
            _maxTick = 0;

            hover.Visibility = Visibility.Hidden;
            playback.Visibility = Visibility.Hidden;
            selection.Visibility = Visibility.Hidden;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.WidthChanged && MaxTick != 0)
            {
                var canvasPosition = TickToCanvasPosition(_playbackPosition);

                playback.X1 = canvasPosition;
                playback.X2 = canvasPosition;
            }

            if (sizeInfo.HeightChanged)
            {
                playback.Y1 = 0;
                playback.Y2 = canvas.ActualHeight;

                hover.Y1 = 0;
                hover.Y2 = canvas.ActualHeight;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (MaxTick == 0)
                return;

            var tickByPosition = MousePositionToTick(e);
            var canvasPosition = TickToCanvasPosition(tickByPosition);

            if (_playbackPosition != tickByPosition)
            {
                _playbackPosition = tickByPosition;
                playback.X1 = canvasPosition;
                playback.X2 = canvasPosition;

                PlaybackPositionChanged?.Invoke(this, EventArgs.Empty);
            }

            _selectionStart = tickByPosition;
            selection.Visibility = Visibility.Hidden;

            e.MouseDevice.Capture(this);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (MaxTick == 0)
                return;

            var tickByPosition = MousePositionToTick(e);
            var canvasPosition = TickToCanvasPosition(tickByPosition);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                hover.Visibility = Visibility.Hidden;

                if (_playbackPosition != tickByPosition)
                {
                    _playbackPosition = tickByPosition;
                    playback.X1 = canvasPosition;
                    playback.X2 = canvasPosition;

                    PlaybackPositionChanged?.Invoke(this, EventArgs.Empty);
                }

                _selectionEnd = tickByPosition;

                var start = Math.Min(_selectionStart, _selectionEnd);
                var end = Math.Max(_selectionStart, _selectionEnd);

                if (end - start >= 1000)
                {
                    var startCanvasPosition = TickToCanvasPosition(start);
                    var endCanvasPosition = TickToCanvasPosition(end);

                    selection.Visibility = Visibility.Visible;
                    Canvas.SetLeft(selection, startCanvasPosition);
                    Canvas.SetTop(selection, 0);
                    selection.Width = endCanvasPosition - startCanvasPosition;
                    selection.Height = canvas.ActualHeight;
                }
                else
                {
                    selection.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                hover.Visibility = Visibility.Visible;
                hover.X1 = canvasPosition;
                hover.X2 = canvasPosition;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            e.MouseDevice.Capture(null);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            hover.Visibility = Visibility.Hidden;
        }

        private int MousePositionToTick(MouseEventArgs e)
        {
            var pos = e.MouseDevice.GetPosition(canvas);
            var percentage = (double)pos.X / canvas.ActualWidth;
            var ticks = percentage * MaxTick;

            return (int)Math.Round(ticks);
        }

        private double TickToCanvasPosition(int tick)
        {
            if (MaxTick == 0)
                throw new InvalidOperationException();

            var percentage = (double)tick / MaxTick;
            var canvasPosition = percentage * canvas.ActualWidth;

            return canvasPosition;
        }
    }
}
