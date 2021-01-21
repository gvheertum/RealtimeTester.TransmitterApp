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
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace MCListener.Service
{
    public static class MulticastResultFunction
    {
  
        [FunctionName("RegisterPingData")]
        public static async Task<IActionResult> RegisterPingData([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Ping/{sessionId}/{pingId}")] HttpRequest req,
            ILogger log, ExecutionContext context, string sessionId, string pingId)
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
                WarnOnInconsistentIdentifiers(ping, log);

                //Init config
                var configRoot = GetConfigurationRoot(context);

                //Store the data
                new PingDataWriter(configRoot, log).RegisterPingData(ping);

                //Success, tnx and bye
                return new OkObjectResult(new { success = true, message = $"Stored ping {sessionId}|{pingId}" });
            }
            catch(Exception e)
            {
                log.LogError($"Failed request: {e.ToString()}");
                return new BadRequestObjectResult(new { success = false, message = $"Could not process request: {e.Message}" });

            }
        }

        private static void WarnOnInconsistentIdentifiers(PingDiagnostic diag, ILogger log)
        {
            if(!diag.Responders?.Any() == true) { return; }
            if(diag.Responders.Any(r => r.PingIdentifier != diag.PingIdentifier || r.SessionIdentifier != diag.SessionIdentifier))
            {
                log.LogWarning($"One or more responses in ping {diag.SessionIdentifier}|{diag.PingIdentifier} did not contain the same identifiers (but will be forced to the parent ping anyway)");
            }
        }

        private static IConfigurationRoot GetConfigurationRoot(ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return config;
        }
    }
}
