using Microsoft.Extensions.Logging;

namespace MCListener.TestTool.Testers
{
    public class MulticastWriteTester
    {
        private ILogger<MulticastListenTester> logger;
        private IMulticastClient multicastClient;

        public MulticastWriteTester(ILogger<MulticastListenTester> logger, IMulticastClient multicastClient)
        {
            this.logger = logger;
            this.multicastClient = multicastClient;
        }

        public void StartTest()
        {
            logger.LogInformation("Start writing");
            multicastClient.SendMessage("This is test");
        }
    }
}