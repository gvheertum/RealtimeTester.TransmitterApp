using MCListener.Shared;
using MCListener.TestTool.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace MCListener.TestTool.Cloud
{
    public interface IAzureFunctionPublisher
    {
        void PublishToAzure(PingDiagnostic diagnostic);
    }
    public class AzureFunctionPublisher : IAzureFunctionPublisher
    {
        private ILogger<AzureFunctionPublisher> logger;
        private AzureConfiguration azureConfig;

        public AzureFunctionPublisher(ILogger<AzureFunctionPublisher> logger, IOptions<AzureConfiguration> azureConfig)
        {
            this.logger = logger;
            this.azureConfig = azureConfig.Value;
            this.azureConfig.AssertValidity();
        }

        public void PublishToAzure(PingDiagnostic diagnostic)
        {
            string urlToUse = ComposeUrl(azureConfig.Endpoint, diagnostic);
            new Thread(() => {
                try
                {
                    logger.LogInformation($"Publishing to to Azure {urlToUse}");
                    HttpClient client = new HttpClient();
                    var objectString = JsonConvert.SerializeObject(diagnostic);

                    var content = new StringContent(objectString, Encoding.UTF8, "application/json");
                    var res = client.PostAsync(urlToUse, content).GetAwaiter().GetResult();
                    logger.LogInformation($"Published to Azure {urlToUse} - {res.StatusCode}");
                }
                catch (Exception e)
                {
                    logger.LogWarning($"Cannot publish to Azure {urlToUse}: {e.Message}");
                }
            }).Start();
        }

        private string ComposeUrl(string rootUrl, PingDiagnostic diag)
        {
            if (!rootUrl.EndsWith("/")) { rootUrl += "/"; }
            return $"{rootUrl}api/Ping/{diag.SessionIdentifier}/{diag.PingIdentifier}/";
        }
    }
}
