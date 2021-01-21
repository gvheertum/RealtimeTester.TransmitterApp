using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MCListener.Shared;

namespace MCListener.Service
{
    public static class MulticastResultFunction
    {
  
        [FunctionName("RegisterPingData")]
        public static async Task<IActionResult> RegisterPingData([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Ping/{sessionId}/{pingId}")] HttpRequest req,
            ILogger log, string sessionId, string pingId)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                PingDiagnostic ping = JsonConvert.DeserializeObject<PingDiagnostic>(requestBody);
                log.LogInformation($"Received ping: {sessionId}|{pingId}");
                log.LogInformation($"Ping data: {ping?.IsSuccess}");

                //Sanity checks
                if (ping == null) { log.LogError("No ping data provided"); return new BadRequestObjectResult("No ping body received"); }
                if (sessionId != ping.SessionIdentifier) { log.LogError($"Mismatch in sessionID: {sessionId} vs {ping.SessionIdentifier}"); return new BadRequestObjectResult("SessionID mismatch between endpoint and body"); }
                if (pingId != ping.PingIdentifier) { log.LogError($"Mismatch in ping: {pingId} vs {ping.PingIdentifier}"); return new BadRequestObjectResult("PingID mismatch between endpoint and body"); }

                //Store the data
                new PingDataStore(log).RegisterPingData(ping);

                //Success, tnx and bye
                return new OkObjectResult(new { success = true, message = $"Stored ping {sessionId}|{pingId}" });
            }
            catch(Exception e)
            {
                return new BadRequestObjectResult(new { success = false, message = $"Could not process request: {e.Message}" });

            }
        }
    }

    public class PingDataStore
    {
        private ILogger logger;

        public PingDataStore(ILogger logger)
        {
            this.logger = logger;
        }

        public void RegisterPingData(PingDiagnostic diagnostic)
        {
            logger.LogInformation("Processing storage of diagnostic");
            RegisterPingRequest(diagnostic);
            diagnostic?.Responders?.ForEach(r => RegisterPongResponse(diagnostic, r));
        }

        private void RegisterPingRequest(PingDiagnostic diagnostic)
        {
            logger.LogInformation($"Writing request: {diagnostic.PingIdentifier}");

        }

        private void RegisterPongResponse(PingDiagnostic diagnostic, PingDiagnosticResponse response)
        {
            logger.LogInformation($"Writing response: {response.ReceiverIdentifier} for ping {diagnostic.PingIdentifier}");
        }
    }
}
