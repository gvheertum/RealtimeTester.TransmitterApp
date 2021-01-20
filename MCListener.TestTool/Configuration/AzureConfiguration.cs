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

    public class TesterConfiguration : IConfigurationValid
    {
        public const string Section = "Tester";
        public int IntervalMS { get; set; }
        public int WaitMS { get; set; }

        public bool TestMulticast { get; set; }
        public bool TestFirebase { get; set; }
        public void AssertValidity()
        { 
            if( IntervalMS <= 0) { throw new ArgumentException("Ping interval invalid", nameof(IntervalMS)); }
            if (WaitMS <= 0) { throw new ArgumentException("Wait interval invalid", nameof(WaitMS)); }
            if (!TestMulticast && !TestFirebase) { throw new ArgumentException("Test at least Multicast or Firebase", nameof(TesterConfiguration)); }
        }
    }
}
