using Firebase.Database;
using Firebase.Database.Query;
using MCListener.Shared;
using MCListener.TestTool.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCListener.TestTool.Firebase
{
    public interface IFirebaseChannel
    {
        System.Threading.Tasks.Task WritePing(PingDiagnostic ping);
        System.Threading.Tasks.Task DisposePing(PingDiagnostic ping);
    }

    public class FirebaseChannel : IFirebaseChannel
    {
        private FirebaseConfiguration configuration;
        private FirebaseClient firebaseClient;

        public FirebaseChannel(IOptions<Configuration.FirebaseConfiguration> configuration)
        {
            this.configuration = configuration.Value;
            this.configuration.AssertValidity();

            this.firebaseClient = new FirebaseClient(this.configuration.Endpoint, new FirebaseOptions { });
        }

        public async System.Threading.Tasks.Task WritePing(PingDiagnostic ping)
        {
            await firebaseClient
                .Child(configuration.Topic)
                .Child(GetIdentifier(ping))
                .PutAsync(ping.GetCopy());

            //TODO: Observe stuff?
            firebaseClient
                .Child(configuration.Topic)
                .Child(GetIdentifier(ping))
                .AsObservable<PingDiagnostic>((e,b) => {}).Subscribe((e) => {
                    Console.WriteLine("Got change!");
                    Console.WriteLine(e);
                });
        }

        public async System.Threading.Tasks.Task DisposePing(PingDiagnostic ping)
        {
            await firebaseClient
                .Child(configuration.Topic)
                .Child(GetIdentifier(ping))
                .DeleteAsync();
        }

        private string GetIdentifier(PingDiagnostic ping)
        {
            return $"{ping.SessionIdentifier}|{ping.PingIdentifier}";
        }
    }

}
