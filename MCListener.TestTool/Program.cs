using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MCListener.TestTool.Entities;
using MCListener.Shared.Helpers;

namespace MCListener.TestTool
{
    class Program
    {
        //TODO: Add retries
        static void Main(string[] args)
        {
            var serviceProvider = DependencyInjection.CreateServiceProvider();
            
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting application");
            //TODO: Move this to the app settings
            //TODO: The DI for the loggers could be a bit nicer
            string ip = "236.99.250.121";
            int port = 30011;
            int portTest = 30022;
            //TODO: Have a responder also available to see whether we can install this on the local network to check for responses
            if(args?.Any(a => a == "listen") == true)
            {
                var mcc = new MulticastClient(ip, port, serviceProvider.GetRequiredService<ILogger<MulticastClient>>());
                logger.LogInformation("Start listening, press ctrl+c to terminate");
                mcc.StartListening((r) => 
                {
                    logger.LogInformation($"Got data: {r}");
                });            
            }
            else if(args?.Any(a => a == "write") == true)
            {
                var mcc = new MulticastClient(ip, port, serviceProvider.GetRequiredService<ILogger<MulticastClient>>());
                logger.LogInformation("Start writing");
                mcc.SendMessage("This is test");
            }
            else if(args?.Any(a => a == "test") == true)
            {
                logger.LogInformation("Start test");
                var mcc = new MulticastClient(ip, portTest, serviceProvider.GetRequiredService<ILogger<MulticastClient>>());
                //TODO: timings can also go in the config
                //TODO: Get me from DI... :(
                new RoundtripTester(mcc, 1000, 2000, serviceProvider.GetRequiredService<IPingDiagnosticContainer>(), serviceProvider.GetRequiredService<IPingDiagnosticMessageTransformer>(), serviceProvider.GetRequiredService<ILogger<RoundtripTester>>()).Start();
            }
            else
            {
                logger.LogInformation("Use the following params:");
                logger.LogInformation("listen    To read from the MC channel");
                logger.LogInformation("write     To write to the MC channel");
                logger.LogInformation("test      Start the tester");
            }
        }
    }

    //TODO: test modes should listen to all applicable channels?
}
