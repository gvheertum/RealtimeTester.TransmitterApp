using System;
using System.Collections.Generic;
using System.Linq;

namespace MCListener
{
    public class RoundtripResult
    {
        public string Identifier { get; set; }
        public DateTime StartTime { get; set; }
        public List<RoundtripResponse> Responders { get; set; } = new List<RoundtripResponse>();
        public bool IsSuccess { get { return Responders.Any(); } }
    }
}