using System;

namespace MCListener.TestTool.Configuration
{
    public class FirebaseConfiguration : IConfigurationValid
    {
        public const string Section = "Firebase";
        public string TopicRequests { get; set; }
        public string TopicResponses { get; set; }
        public bool PurgeFirebaseOnStart { get; set; }

        public string Endpoint { get; set; }
        public void AssertValidity()
        {
            if (string.IsNullOrWhiteSpace(TopicRequests)) { throw new ArgumentException("Firebase Topic Requests", nameof(TopicRequests)); }
            if (string.IsNullOrWhiteSpace(TopicResponses)) { throw new ArgumentException("Firebase Topic Responses", nameof(TopicResponses)); }
            if (string.IsNullOrWhiteSpace(Endpoint)) { throw new ArgumentException("Firebase Endpoint invalid", nameof(Endpoint)); }
        }
    }
}
