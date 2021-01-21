using Microsoft.Extensions.Logging;

namespace MCListener.TestTool.Testers
{
    public class MulticastListenTester
    {
        private ILogger<MulticastListenTester> logger;
        private IMulticastClient multicastClient;

        public MulticastListenTester(ILogger<MulticastListenTester> logger, IMulticastClient multicastClient)
        {
            this.logger = logger;
            this.multicastClient = multicastClient;
        }
        public void StartTest()
        {
            logger.LogInformation("Start listening, press ctrl+c to terminate");
            multicastClient.StartListening((r) =>
            {
                logger.LogInformation($"Got data: {r}");
            });
        }
    }
}