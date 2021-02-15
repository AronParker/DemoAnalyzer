using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DemoAnalyzer.Tools
{
    public class Heatmap : IDisposable
    {
        private HeatmapSafeHandle heatmap;
        private HeatmapStampSafeHandle stamp;
        private bool disposedValue;

        public int Width { get; }
        public int Height { get; }

        public Heatmap(int width, int height)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            heatmap = NativeMethods.AllocHeatmap((uint)width, (uint)height);

            if (heatmap.IsInvalid)
                throw new OutOfMemoryException();

            Width = width;
            Height = height;
        }

        public void AddPoint(int x, int y)
        {
            if ((uint)x >= Width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if ((uint)y >= Height)
                throw new ArgumentOutOfRangeException(nameof(y));

            NativeMethods.AddPointToHeatmap(heatmap, (uint)x, (uint)y);
        }

        public void AddPoint(int x, int y, HeatmapStamp stamp)
        {
            if ((uint)x >= Width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if ((uint)y >= Height)
                throw new ArgumentOutOfRangeException(nameof(y));

            NativeMethods.AddPointToHeatmapWithStamp(heatmap, (uint)x, (uint)y, stamp.stamp);
        }

        public BitmapSource CreateHeatmap()
        {
            var buf = new byte[Width * Height * 4];

            NativeMethods.WriteHeatmap(heatmap, buf);

            for (int i = 0; i < buf.Length; i += 4)
            {
                // Have: RGBA
                // Want: BGRA

                // Swap R & B
                var r = buf[i + 0];
                buf[i + 0] = buf[i + 2];
                buf[i + 2] = r;
            }

            return BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Bgra32, null, buf, Width * 4);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    heatmap.Dispose();
                    stamp.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}
