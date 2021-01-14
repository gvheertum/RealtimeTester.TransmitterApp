using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace MCListener
{
    public interface IRoundtripResultContainer
    {
        RoundtripResult RegisterTripStart(string identifier);
        void RegisterTripResponse(string identifier, string receiver);
        void RegisterTripResponse(RoundtripResult tripData, string receiver);
        void PurgeTripResponse(RoundtripResult tripData);
    }

    public class RoundtripResultContainer : IRoundtripResultContainer
    {
        private Dictionary<string, RoundtripResult> roundtripResults = new Dictionary<string, RoundtripResult>();
        private ILogger<RoundtripResultContainer> logger;

        public RoundtripResultContainer(ILogger<RoundtripResultContainer> logger)
        {
            this.logger = logger;
        }

        public RoundtripResult RegisterTripStart(string identifier)
        {
            var rt = new RoundtripResult() { Identifier = identifier, StartTime = DateTime.Now };
            roundtripResults.Add(identifier, rt);
            return rt;
        }

        public void RegisterTripResponse(string identifier, string receiver)
        {
            logger.LogDebug($"Got response for {identifier} from {receiver}");
            try
            {
                var rt = roundtripResults[identifier];
                RegisterTripResponse(rt, receiver);
            }
            catch(Exception e)
            {
               logger.LogWarning($"Cannot find identifier: {identifier}");
            }
        }

        public void RegisterTripResponse(RoundtripResult tripData, string receiver)
        {
            tripData?.Responders.Add(new RoundtripResponse() { ReceiverIdentifier = receiver, ReceiveTime = DateTime.Now });
        }

        public void PurgeTripResponse(RoundtripResult result)
        {
            try 
            {
                this.roundtripResults.Remove(result.Identifier);
                logger.LogDebug($"Purged record: {result.Identifier}");
            }
            catch(Exception e)
            {
                logger.LogDebug($"Cannot purge: {result.Identifier} -> {e.Message}");
            }
        }
    }
}