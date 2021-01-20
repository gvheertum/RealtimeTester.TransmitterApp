using Firebase.Database;
using Firebase.Database.Query;
using MCListener.Shared;
using MCListener.TestTool.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

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
        void StartReceiving(Func<string, bool> handler);
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
        
        //We now handle the received pongs equal to the Multicast, but are doing "real" objects as part of the ping, for now ok, but not so pretty
        public void StartReceiving(Func<string, bool> handler)
        {
            firebaseClient.Child(configuration.TopicResponses).AsObservable<FirebaseResponseMessage>().Subscribe((data) => {
                //Some of the request respond empty
                if (data != null && !string.IsNullOrWhiteSpace(data.Key))
                {
                    logger.LogDebug($"Got data from Firebase: {data.Key}");
                    if (handler(data.Key))
                    {
                        RemovePongFromFirebase(data.Key);
                    }
                }
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
    }

}
