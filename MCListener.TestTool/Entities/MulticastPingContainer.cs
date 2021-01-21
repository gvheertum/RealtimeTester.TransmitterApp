using System;
using System.Collections.Generic;
using System.Linq;
using MCListener.Shared;
using Microsoft.Extensions.Logging;

namespace MCListener.TestTool.Entities
{
    public interface IPingDiagnosticContainer
    {
        PingDiagnostic RegisterTripStart(string sessionIdentifier, string identifier);
        bool RegisterTripResponse(PingDiagnosticResponse response);
        bool RegisterTripResponse(PingDiagnostic tripData, PingDiagnosticResponse response);
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


        public bool RegisterTripResponse(PingDiagnosticResponse response)
        {
            logger.LogDebug($"Got response for {response.PingIdentifier} from {response.ReceiverIdentifier}");
            try
            {
                if (MulticastPings.ContainsKey(response.PingIdentifier))
                {
                    var rt = MulticastPings[response.PingIdentifier];
                    return RegisterTripResponse(rt, response);
                }
                else
                {
                    //In theory the process can crash if the key is removed after the first if, locking here might be a too large impact so we allow it here and let the error handling take care of that case
                    logger.LogWarning($"Cannot process {response.Channel} from {response.ReceiverIdentifier}, ping {response.SessionIdentifier}|{response.PingIdentifier} was already purged");
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.LogWarning($"Cannot process {response.Channel} from {response.ReceiverIdentifier}, for: {response.SessionIdentifier}|{response.PingIdentifier} -> {e.Message}");
                return false;
            }
        }

        public bool RegisterTripResponse(PingDiagnostic tripData, PingDiagnosticResponse response)
        {
            //If the tripdata was not ours we skip processing and signal that it's not our process for now
            if (tripData == null) { return false; }

            if (IsNewResponseForPing(tripData, response))
            {
                tripData?.Responders.Add(response);
            }
            else
            {
                logger.LogDebug($"Response on ping {tripData.SessionIdentifier}|{tripData.PingIdentifier} by {response.ReceiveTime} over {response.Channel} is already processed, skipping duplicate");
            }
            return tripData != null; //whether or not we were allowed to process this
        }

        // Check if there is already a response from this device, we are not doing duplicates
        private bool IsNewResponseForPing(PingDiagnostic tripData, PingDiagnosticResponse response)
        {
            if (tripData == null || response == null) { return false; }
            return !tripData.Responders.Any(r => r.Channel == response.Channel && r.ReceiverIdentifier == response.ReceiverIdentifier);
        }

        public void PurgeTripResponse(PingDiagnostic result)
        {
            try
            {
                this.MulticastPings.Remove(result.PingIdentifier);
                logger.LogDebug($"Purged record: {result.PingIdentifier}");
            }
            catch (Exception e)
            {
                logger.LogDebug($"Cannot purge: {result.PingIdentifier} -> {e.Message}");
            }
        }
    }
}