using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MCListener.TestTool.Entities;
using MCListener.Shared.Helpers;
using MCListener.TestTool.Testers;

namespace MCListener.TestTool
{
    class Program
    {
        private const string DefaultModeParam = "DefaultMode";
        //TODO: Add retries
        //TODO: Add hw id
        static void Main(string[] args)
        {
           

            var serviceProvider = DependencyInjection.CreateServiceProvider();
            

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting application");
            
            //Check if we have arguments, if no use the default from the config
            if (!args.Any())
            {
                logger.LogWarning("No runmode provided"); 
                string configFallbackRunMode = DependencyInjection.GetConfiguration().GetSection(DefaultModeParam).Value;
                if(!string.IsNullOrWhiteSpace(configFallbackRunMode))
                {
                    logger.LogWarning($"Falling back to runmode from config: {configFallbackRunMode}");
                    args = new string[] { configFallbackRunMode };
                }
            }

            if(args?.Any(a => a == "listen") == true)
            {
                logger.LogInformation("Start listen mode");
                serviceProvider.GetRequiredService<MulticastListenTester>().StartTest();
            }
            else if(args?.Any(a => a == "write") == true)
            {
                logger.LogInformation("Start test write");
                serviceProvider.GetRequiredService<MulticastWriteTester>().StartTest();
            }
            else if(args?.Any(a => a == "test") == true)
            {
                logger.LogInformation("Start test mode (firebase/multicast)");
                serviceProvider.GetRequiredService<IRoundtripTester>().Start();
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
