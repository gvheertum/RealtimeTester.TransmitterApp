using System;

namespace MCListener.Shared
{
    public class PingDiagnosticResponse
    {
        public DateTime ReceiveTime { get; set; }
        public string SessionIdentifier { get; set; }
        public string PingIdentifier { get; set; }
        public string ReceiverIdentifier { get; set; }

        public PingDiagnosticResponseChannel Channel { get; set; }

        public PingDiagnosticResponseDeviceDetail DeviceDetail { get; set; }

    }

    public class PingDiagnosticResponseDeviceDetail
    {

    }
    
    public enum PingDiagnosticResponseChannel
    {
        Unknown,
        Multicast,
        Firebase
    }
}