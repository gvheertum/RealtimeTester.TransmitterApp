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
        private const string DefaultModeParam = "DefaultMode";
        //TODO: Add retries
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


            //TODO: Have a responder also available to see whether we can install this on the local network to check for responses?
            if(args?.Any(a => a == "listen") == true)
            {
                var mcc = serviceProvider.GetRequiredService<MulticastClient>();
                logger.LogInformation("Start listening, press ctrl+c to terminate");
                mcc.StartListening((r) => 
                {
                    logger.LogInformation($"Got data: {r}");
                });            
            }
            else if(args?.Any(a => a == "write") == true)
            {
                var mcc = serviceProvider.GetRequiredService<MulticastClient>();
                logger.LogInformation("Start writing");
                mcc.SendMessage("This is test");
            }
            else if(args?.Any(a => a == "test") == true)
            {
                logger.LogInformation("Start test");
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

    //TODO: test modes should listen to all applicable channels?
}
