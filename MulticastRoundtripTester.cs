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
        private IRoundtripResultContainer container;
        private MulticastClient multicastClient;
        public MulticastRoundtripTester(MulticastClient multicast, int intervalMs, int waitMs, IRoundtripResultContainer container, ILogger<MulticastRoundtripTester> logger)
        {
            this.intervalMs = intervalMs;
            this.waitMs = waitMs;
            this.logger = logger;
            this.container = container;
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
                logger.LogDebug($"Ping: {identifier}");
                
                // Send the outgoing message
                multicastClient.SendMessage($"MCPING|{identifier}");
                var tripData = container.RegisterTripStart(identifier);

                HandleFinalizeOfPing(tripData, this.waitMs); //Schedule the way for resolve thread
                
                //Sleep until sending the netxt ping.
                Thread.Sleep(intervalMs);
            }
        }

        private void HandleFinalizeOfPing(RoundtripResult roundtrip, int sleep)
        {
            new Thread(() => {
                Thread.Sleep(sleep);
                logger.LogDebug($"Waking up to resolve ping: {roundtrip.Identifier}");

                if(roundtrip.IsSuccess)
                {
                    string formattedReplies = String.Join("|", roundtrip.Responders.Select(r => $"{r.ReceiveTime.ToString("HH:mm:ss.fff")}|{r.ReceiverIdentifier}"));
                    logger.LogInformation($"{{{roundtrip.StartTime.ToString("HH:mm:ss.fff")}|{roundtrip.Identifier}|SUCCESS|{{{formattedReplies}}}}}");
                }
                else
                {
                    logger.LogCritical($"{{{roundtrip.StartTime.ToString("HH:mm:ss.fff")}|{roundtrip.Identifier}|FAILED}}");
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
        private (string messageId, string receiverId) ParseMessage(string message)
        {
            logger.LogTrace($"Received message: {message}");
            if(message.StartsWith("MCPING|")) 
            {
                logger.LogDebug("This is my own ping, ignore");
                return (null, null); 
            }
            else if(message.StartsWith("MCPONG|"))
            {
                logger.LogDebug("Received a pong");
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