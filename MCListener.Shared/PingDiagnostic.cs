using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCListener.Shared
{
    public class PingDiagnostic
    {
        public string SessionIdentifier { get; set; }
        public string PingIdentifier { get; set; }
        public DateTime StartTime { get; set; }
        public List<PingDiagnosticResponse> Responders { get; set; } = new List<PingDiagnosticResponse>();
        public bool IsSuccess { get { return Responders.Any(); } }
    }
}
