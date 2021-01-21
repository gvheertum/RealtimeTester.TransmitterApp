using System;
using System.Collections.Generic;
using System.Text;

namespace MCListener.TestTool.Configuration
{
    public class AzureConfiguration : IConfigurationValid
    {
        public const string Section = "Azure";
        public string Endpoint { get; set; }
        public void AssertValidity()
        {
            if (string.IsNullOrWhiteSpace(Endpoint)) { throw new ArgumentException("Azure Endpoint invalid", nameof(Endpoint)); }
        }
    }
}
