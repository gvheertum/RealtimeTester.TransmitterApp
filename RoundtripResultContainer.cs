using System;
using System.Collections.Generic;

namespace MCListener
{
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
    }
}