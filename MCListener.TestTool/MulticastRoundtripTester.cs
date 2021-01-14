using System;
using System.Linq;
using System.Threading;
using MCListener.Shared;
using MCListener.TestTool.Entities;
using Microsoft.Extensions.Logging;

namespace MCListener.TestTool
{
    public class MulticastRoundtripTester
    {
        private int intervalMs;
        private int waitMs;
        private ILogger<MulticastRoundtripTester> logger;
        private IMulticastPingContainer container;
        private MulticastClient multicastClient;
        private string sessionIdentifier;

        public MulticastRoundtripTester(MulticastClient multicast, int intervalMs, int waitMs, IMulticastPingContainer container, ILogger<MulticastRoundtripTester> logger)
        {
            this.intervalMs = intervalMs;
            this.waitMs = waitMs;
            this.logger = logger;
            this.container = container;
            multicastClient = multicast;
            this.sessionIdentifier = GenerateIdentifier();
        }

        public void Start()
        {
            //Registering receiver
            logger.LogDebug("Starting reader");
            multicastClient.StartListening(ProcessResponse);

            //Doing send
            logger.LogDebug($"Starting writer with sessionid: {sessionIdentifier}");
            while(true)
            {
                var pingIdentifier = GenerateIdentifier();                
                logger.LogDebug($"Ping: {pingIdentifier}");
                
                // Send the outgoing message
                multicastClient.SendMessage($"MCPING|{sessionIdentifier}|{pingIdentifier}");
                var tripData = container.RegisterTripStart(sessionIdentifier, pingIdentifier);

                HandleFinalizeOfPing(tripData, this.waitMs); //Schedule the way for resolve thread
                
                //Sleep until sending the netxt ping.
                Thread.Sleep(intervalMs);
            }
        }
        //TODO: do something with the sessionID

        private void HandleFinalizeOfPing(MulticastPing roundtrip, int sleep)
        {
            new Thread(() => {
                Thread.Sleep(sleep);
                logger.LogDebug($"Waking up to resolve ping: {roundtrip.SessionIdentifier}");

                if(roundtrip.IsSuccess)
                {
                    string formattedReplies = String.Join("|", roundtrip.Responders.Select(r => $"{r.ReceiveTime.ToString("HH:mm:ss.fff")}|{r.ReceiverIdentifier}"));
                    logger.LogInformation($"{{{roundtrip.StartTime.ToString("HH:mm:ss.fff")}|{sessionIdentifier}|{roundtrip.PingIdentifier}|SUCCESS|{{{formattedReplies}}}}}");
                }
                else
                {
                    logger.LogCritical($"{{{roundtrip.StartTime.ToString("HH:mm:ss.fff")}|{sessionIdentifier}|{roundtrip.PingIdentifier}|FAILED}}");
                }

                container.PurgeTripResponse(roundtrip);
            }).Start();
        }



        private void ProcessResponse(string response)
        {
            var msgData = ParseMessage(response);
            if(msgData.messageId == null) { return; } //Invalid message (or not for us, so ignore it)
            
            container.RegisterTripResponse(msgData.messageId, msgData.receiverId);
        }

        //Parse the received mesage in the messageID and receiverID
        private (string sessionId, string messageId, string receiverId) ParseMessage(string message)
        {
            logger.LogTrace($"Received message: {message}");

            if(message.StartsWith("MCPONG|"))
            {
                logger.LogDebug("Recognized as a pong command");
                string[] spl = message.Split("|");
                if(spl.Length >= 4) 
                {   
                    return (sessionId: spl[1], messageId: spl[2], receiverId: spl[3]);
                }
            }

            //Fallback
            logger.LogDebug($"{message} is not a valid pong, ignoring"); 
            return (null, null, null);
        }

        //TODO: Make clean up script
        //TODO: Make script to flag missed beats

        private string GenerateIdentifier()
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }
    }
}