using System;

namespace MCListener.TestTool.Configuration
{
    public class MulticastConfiguration : IConfigurationValid
    {
        public const string Section = "Multicast";
        public string Ip { get; set; }
        public int Port { get; set; }

        public void AssertValidity()
        {
            if (Port <= 0) { throw new ArgumentException("Multicast port invalid", nameof(Port)); }
            if (string.IsNullOrWhiteSpace(Ip)) { throw new ArgumentException("Multicast IP invalid", nameof(Ip)); }
        }
    }
}
