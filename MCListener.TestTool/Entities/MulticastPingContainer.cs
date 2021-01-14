using System;
using System.Collections.Generic;
using MCListener.Shared;
using Microsoft.Extensions.Logging;

namespace MCListener.TestTool.Entities
{
    public interface IMulticastPingContainer
    {
        MulticastPing RegisterTripStart(string sessionIdentifier, string identifier);
        void RegisterTripResponse(string identifier, string receiver);
        void RegisterTripResponse(MulticastPing tripData, string receiver);
        void PurgeTripResponse(MulticastPing tripData);
    }

    public class MulticastPingContainer : IMulticastPingContainer
    {
        private Dictionary<string, MulticastPing> MulticastPings = new Dictionary<string, MulticastPing>();
        private ILogger<MulticastPingContainer> logger;

        public MulticastPingContainer(ILogger<MulticastPingContainer> logger)
        {
            this.logger = logger;
        }

        public MulticastPing RegisterTripStart(string sessionIdentifier, string pingIdentifier)
        {
            var rt = new MulticastPing() { SessionIdentifier = sessionIdentifier, PingIdentifier = pingIdentifier, StartTime = DateTime.Now };
            MulticastPings.Add(pingIdentifier, rt);
            return rt;
        }

        //TODO: No session id used here, but is that logical?

        public void RegisterTripResponse(string identifier, string receiver)
        {
            logger.LogDebug($"Got response for {identifier} from {receiver}");
            try
            {
                var rt = MulticastPings[identifier];
                RegisterTripResponse(rt, receiver);
            }
            catch(Exception e)
            {
               logger.LogWarning($"Cannot find identifier: {identifier}");
            }
        }

        public void RegisterTripResponse(MulticastPing tripData, string receiver)
        {
            tripData?.Responders.Add(new MulticastPong() { ReceiverIdentifier = receiver, ReceiveTime = DateTime.Now });
        }

        public void PurgeTripResponse(MulticastPing result)
        {
            try 
            {
                this.MulticastPings.Remove(result.PingIdentifier);
                logger.LogDebug($"Purged record: {result.PingIdentifier}");
            }
            catch(Exception e)
            {
                logger.LogDebug($"Cannot purge: {result.PingIdentifier} -> {e.Message}");
            }
        }
    }
}