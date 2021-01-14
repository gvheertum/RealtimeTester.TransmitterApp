using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MCListener
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = DependencyInjection.CreateServiceProvider();
            
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting application");
            //TODO: Move this to the app settings
            //TODO: The DI for the loggers could be a bit nicer
            string ip = "236.99.250.121";
            int port = 30011;
            int portTest = 30011;
            
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
                new MulticastRoundtripTester(mcc, 2000, 5000, serviceProvider.GetRequiredService<IRoundtripResultContainer>(), serviceProvider.GetRequiredService<ILogger<MulticastRoundtripTester>>()).Start();
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

    
}
