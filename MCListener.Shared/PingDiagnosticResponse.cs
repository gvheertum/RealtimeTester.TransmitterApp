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
        public string CellularType { get; set; }
        public string CellularProvider { get; set; }
        public int? CellularSignalStrength { get; set; }
        public string WifiProvider { get; set; }
        public int? WifiSignalStrength { get; set; }
        public int? BatteryPercentage { get; set; }
        public int? VolumePercentage { get; set; }
    }

    public enum PingDiagnosticResponseChannel
    {
        Unknown,
        Multicast,
        Firebase
    }
}