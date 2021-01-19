using System;
using System.Collections.Generic;
using System.Text;

namespace MCListener.TestTool.Configuration
{
    public interface IConfigurationValid
    {
        void AssertValidity();
    }



    public class AzureConfiguration : IConfigurationValid
    {
        public const string Section = "Azure";
        public string Endpoint { get; set; }
        public void AssertValidity()
        {
            if (string.IsNullOrWhiteSpace(Endpoint)) { throw new ArgumentException("Azure Endpoint invalid", nameof(Endpoint)); }
        }
    }

    public class FirebaseConfiguration : IConfigurationValid
    {
        public const string Section = "Firebase";
        public string Topic { get; set; }
        public string Endpoint { get; set; }
        public void AssertValidity()
        {
            if (string.IsNullOrWhiteSpace(Topic)) { throw new ArgumentException("Firebase Topic", nameof(Topic)); }
            if (string.IsNullOrWhiteSpace(Endpoint)) { throw new ArgumentException("Firebase Endpoint invalid", nameof(Endpoint)); }
        }
    }

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

    public class TesterConfiguration : IConfigurationValid
    {
        public const string Section = "Tester";
        public int IntervalMS { get; set; }
        public int WaitMS { get; set; }
        public void AssertValidity()
        { 
            if( IntervalMS <= 0) { throw new ArgumentException("Ping interval invalid", nameof(IntervalMS)); }
            if (WaitMS <= 0) { throw new ArgumentException("Wait interval invalid", nameof(WaitMS)); }
        }
    }
}
