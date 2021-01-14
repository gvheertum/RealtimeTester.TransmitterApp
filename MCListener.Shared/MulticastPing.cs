using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCListener.Shared
{
    public class MulticastPing
    {
        public string SessionIdentifier { get; set; }
        public string PingIdentifier { get; set; }
        public DateTime StartTime { get; set; }
        public List<MulticastPong> Responders { get; set; } = new List<MulticastPong>();
        public bool IsSuccess { get { return Responders.Any(); } }
    }
}
