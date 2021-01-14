using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MCListener
{
    public class MulticastRoundtripTester
    {
        private string ip;
        private int port;
        private int intervalMs;
        private int waitMs;
        private ILogger<MulticastRoundtripTester> logger;
        private RoundtripResultContainer container = new RoundtripResultContainer();
        private MulticastClient multicastClient;
        public MulticastRoundtripTester(MulticastClient multicast, int intervalMs, int waitMs, ILogger<MulticastRoundtripTester> logger)
        {
            this.intervalMs = intervalMs;
            this.waitMs = waitMs;
            this.logger = logger;
            multicastClient = multicast; 
        }

        public void Start()
        {
            //Registering receiver
            logger.LogDebug("Starting reader");
            multicastClient.StartListening(ProcessResponse);

            //Doing send
            logger.LogDebug("Starting writer");
            while(true)
            {
                var identifier = GenerateIdentifier();                
                System.Console.WriteLine($"Ping: {identifier}");
                RelayOutputMessage(identifier);
                container.RegisterTripStart(identifier);

                Thread.Sleep(intervalMs);
            }
        }

        private void RelayOutputMessage(string identifier)
        {
            multicastClient.SendMessage($"MCPING|{identifier}");
        }

        private void ProcessResponse(string response)
        {
            var msgData = ParseMessage(response);
            if(msgData.messageId == null) { return; } //Invalid message
            
            container.RegisterTripResponse(msgData.messageId, msgData.receiverId);
        }

        private (string messageId, string receiverId) ParseMessage(string message)
        {
            if(message.StartsWith("MCPING|")) 
            {
                logger.LogDebug("This is my own ping, ignore");
                return (null, null); 
            }
            else if(message.StartsWith("MCPONG|"))
            {
                string[] spl = message.Split("|");
                if(spl.Length >= 3) 
                {   
                    return (messageId: spl[1], receiverId: spl[2]);
                }
            }

            //Fallback
            logger.LogDebug($"{message} is not a valid pong, ignoring"); 
            return (null, null);
        }

        //TODO: Make clean up script
        //TODO: Make script to flag missed beats

        private string GenerateIdentifier()
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }
    }
}