using System;

namespace MCListener.TestTool.Configuration
{
    public class MulticastConfiguration : IConfigurationValid
    {
        public const string Section = "Multicast";
        public string Ip { get; set; }
        public int Port { get; set; }

        public bool PerformBurst { get; set; }
        public int BurstCount { get; set; }
        public int BurstIntervalMS { get; set; }

        public void AssertValidity()
        {
            if (Port <= 0) { throw new ArgumentException("Multicast port invalid", nameof(Port)); }
            if (string.IsNullOrWhiteSpace(Ip)) { throw new ArgumentException("Multicast IP invalid", nameof(Ip)); }
            if (PerformBurst && (BurstCount <= 1 || BurstIntervalMS <= 0)) { throw new ArgumentException("If burst is enabled the BurstCount and BurstIntervalMS should be set and valid (interval >= 1 count > 1)", nameof(PerformBurst)); }
        }
    }
}
