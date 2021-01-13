using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MCListener
{
    public class MulticastRoundtripTester
    {
        private string ip;
        private int port;
        private int intervalMs;
        private int waitMs;

        private RoundtripResultContainer container = new RoundtripResultContainer();
        private MulticastClient multicastClient;
        public MulticastRoundtripTester(string ip, int port, int intervalMs, int waitMs)
        {
            this.ip = ip;
            this.port = port;
            this.intervalMs = intervalMs;
            this.waitMs = waitMs;
            multicastClient = new MulticastClient(ip, port);
        }

        public void Start()
        {
            //Registering receiver
            System.Console.WriteLine("Starting reader");
            multicastClient.StartListening(ProcessResponse);

            //Doing send
            System.Console.WriteLine("Starting writer");
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
                System.Console.WriteLine("This is my own ping, ignore");
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
            System.Console.WriteLine($"{message} is not a valid pong, ignoring"); 
            return (null, null);
        }

        //TODO: Make clean up script
        //TODO: Make script to flag missed beats

        private string GenerateIdentifier()
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }
    }

    public class RoundtripResultContainer
    {
        private Dictionary<string, RoundtripResult> roundtripResults = new Dictionary<string, RoundtripResult>();

        public void RegisterTripStart(string identifier)
        {
            var rt = new RoundtripResult() { Identifier = identifier, StartTime = DateTime.Now };
            roundtripResults.Add(identifier, rt);
        }

        public void RegisterTripResponse(string identifier, string receiver)
        {
            System.Console.WriteLine($"Got response for {identifier} from {receiver}");
            try
            {
                var rt = roundtripResults[identifier];
                rt.Responders.Add(new RoundtripResponse() { ReceiverIdentifier = receiver, ReceiveTime = DateTime.Now });
            }
            catch(Exception e)
            {
                System.Console.WriteLine($"Cannot find identifier: {identifier}");
            }
        }

        //MCPING|{ID}
        //MCPONG|{ID}|{RESPONDER}
    }

    public class RoundtripResult
    {
        public string Identifier { get; set; }
        public DateTime StartTime { get; set; }
        public List<RoundtripResponse> Responders { get; set; } = new List<RoundtripResponse>();
        public bool IsSuccess { get { return Responders.Any(); } }
    }
    public class RoundtripResponse
    {
        public DateTime ReceiveTime { get; set; }
        public string ReceiverIdentifier { get; set; }
    }
}