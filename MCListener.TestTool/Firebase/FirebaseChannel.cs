using Firebase.Database;
using Firebase.Database.Query;
using MCListener.Shared;
using MCListener.TestTool.Configuration;
using MCListener.TestTool.Testers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MCListener.TestTool.Firebase
{
    public class FirebaseResponseMessage
    {
        public string data { get; set; }
    }

    public interface IFirebaseChannel
    {
        System.Threading.Tasks.Task WritePing(PingDiagnostic ping);
        System.Threading.Tasks.Task DisposePing(PingDiagnostic ping);
        void StartReceiving(Func<string, PingLookupResult> handler);
    }
    //TODO: this one kinda crashes ;)
    public class FirebaseChannel : IFirebaseChannel
    {
        private FirebaseConfiguration configuration;
        private ILogger<FirebaseChannel> logger;
        private FirebaseClient firebaseClient;

        public FirebaseChannel(IOptions<Configuration.FirebaseConfiguration> configuration, ILogger<FirebaseChannel> logger)
        {
            this.configuration = configuration.Value;
            this.configuration.AssertValidity();
            this.logger = logger;

            this.firebaseClient = new FirebaseClient(this.configuration.Endpoint, new FirebaseOptions { });
        }

        //List of results that are allowed to remove the response from the firebase sub (we should also cleanup expired stuff)
        private static PingLookupResult[] ValidPingLookupResults = new[] { PingLookupResult.Found, PingLookupResult.NotInCollection };

        //We now handle the received pongs equal to the Multicast, but are doing "real" objects as part of the ping, for now ok, but not so pretty
        public void StartReceiving(Func<string, PingLookupResult> handler)
        {
            if(configuration.PurgeFirebaseOnStart) { PurgeFirebaseInstance(); }
            firebaseClient.Child(configuration.TopicResponses).AsObservable<FirebaseResponseMessage>().Subscribe((data) => {
                new Thread(() => //offload this thread to the background to allow us to get data updates
                {
                    //Some of the request responds are empty
                    if (data != null && !string.IsNullOrWhiteSpace(data.Key))
                    {
                        logger.LogDebug($"Got data from Firebase: {data.Key}");
                        var handleRes = handler(data.Key);
                        if (ValidPingLookupResults.Contains(handleRes))
                        {
                            RemovePongFromFirebase(data.Key);
                        }
                    }
                }).Start();
            });
        }

        private void RemovePongFromFirebase(string identifier)
        {
            try
            {
                logger.LogDebug($"removing from Firebase: {identifier}");
                firebaseClient.Child(configuration.TopicResponses).Child(identifier).DeleteAsync().GetAwaiter().GetResult();
                logger.LogDebug($"Removed from Firebase: {identifier}");
            }
            catch (Exception e)
            {
                logger.LogWarning($"Cannot purge {identifier} from firebase: {e.Message}");

            }
        }

        public async System.Threading.Tasks.Task WritePing(PingDiagnostic ping)
        {
            try
            {
                await firebaseClient
                    .Child(configuration.TopicRequests)
                    .Child(GetIdentifier(ping))
                    .PutAsync(ping.GetCopy());
            }
            catch(Exception e)
            {
                logger.LogWarning($"Cannot put ping {GetIdentifier(ping)}: {e.Message}");
            }
        }

        //TODO: The dispose is possibly not called for missed pings, which is flooding the datastream
        public async System.Threading.Tasks.Task DisposePing(PingDiagnostic ping)
        {
            try
            {
                await firebaseClient
                    .Child(configuration.TopicRequests)
                    .Child(GetIdentifier(ping))
                    .DeleteAsync();
            }
            catch(Exception e)
            {
                logger.LogWarning($"Cannot dispose ping {GetIdentifier(ping)}: {e.Message}");
            }
        }

        private string GetIdentifier(PingDiagnostic ping)
        {
            return $"{ping.SessionIdentifier}|{ping.PingIdentifier}";
        }

        private void PurgeFirebaseInstance()
        {
            logger.LogInformation("Purging firebase instance data before start");
            try { firebaseClient.Child(configuration.TopicRequests).DeleteAsync().GetAwaiter().GetResult(); }
            catch(Exception e) { logger.LogWarning($"Could not purge firebase topic: {configuration.TopicRequests}, {e.Message}"); }

            try { firebaseClient.Child(configuration.TopicResponses).DeleteAsync().GetAwaiter().GetResult(); }
            catch (Exception e) { logger.LogWarning($"Could not purge firebase topic: {configuration.TopicResponses}, {e.Message}"); }
        }
    }

}
