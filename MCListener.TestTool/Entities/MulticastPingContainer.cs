using System;
using System.Collections.Generic;
using MCListener.Shared;
using Microsoft.Extensions.Logging;

namespace MCListener.TestTool.Entities
{
    public interface IPingDiagnosticContainer
    {
        PingDiagnostic RegisterTripStart(string sessionIdentifier, string identifier);
        void RegisterTripResponse(PingDiagnosticResponse response);
        void RegisterTripResponse(PingDiagnostic tripData, PingDiagnosticResponse response);
        void PurgeTripResponse(PingDiagnostic tripData);
    }

    public class PingDiagnosticContainer : IPingDiagnosticContainer
    {
        private Dictionary<string, PingDiagnostic> MulticastPings = new Dictionary<string, PingDiagnostic>();
        private ILogger<PingDiagnosticContainer> logger;

        public PingDiagnosticContainer(ILogger<PingDiagnosticContainer> logger)
        {
            this.logger = logger;
        }

        public PingDiagnostic RegisterTripStart(string sessionIdentifier, string pingIdentifier)
        {
            var rt = new PingDiagnostic() { SessionIdentifier = sessionIdentifier, PingIdentifier = pingIdentifier, StartTime = DateTime.Now };
            MulticastPings.Add(pingIdentifier, rt);
            return rt;
        }

        //TODO: No session id used here, but is that logical?

        public void RegisterTripResponse(PingDiagnosticResponse response)
        {
            logger.LogDebug($"Got response for {response.PingIdentifier} from {response.ReceiverIdentifier}");
            try
            {
                var rt = MulticastPings[response.PingIdentifier];
                RegisterTripResponse(rt, response);
            }
            catch(Exception e)
            {
               logger.LogWarning($"Cannot find identifier: {response.PingIdentifier}");
            }
        }

        public void RegisterTripResponse(PingDiagnostic tripData, PingDiagnosticResponse response)
        {
            tripData?.Responders.Add(response);
        }

        public void PurgeTripResponse(PingDiagnostic result)
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