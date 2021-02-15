using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoAnalyzer.Tools
{
    public class HeatmapStamp : IDisposable
    {
        internal HeatmapStampSafeHandle stamp;
        private bool disposedValue;

        public int Radius { get; }

        public HeatmapStamp(int radius)
        {
            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius));

            stamp = NativeMethods.CreateStamp((uint)radius);

            if (stamp.IsInvalid)
                throw new OutOfMemoryException();

            Radius = radius;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stamp.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }
    }
}
