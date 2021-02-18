using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoAnalyzer.Data
{
    [Serializable]
    public class DemoDataException : Exception
    {
        public DemoDataException() { }
        public DemoDataException(string message) : base(message) { }
        public DemoDataException(string message, Exception inner) : base(message, inner) { }
        protected DemoDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
