using System;
using MCListener.TestTool.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace MCListener.TestTool
{
    public static class DependencyInjection
    {
        private static readonly ServiceCollection ServiceCollection;

        static DependencyInjection()
        {
            ServiceCollection = new ServiceCollection();
            
            // Configuration
            var configuration = GetConfiguration();
            ServiceCollection.AddSingleton(configuration);
            ServiceCollection.AddTransient<IMulticastPingContainer, MulticastPingContainer>();
            
            // Logging
            var nlogConfiguration = new NLogLoggingConfiguration(configuration.GetSection("NLog"));
            ServiceCollection.AddLogging(configure => configure.AddNLog(nlogConfiguration));
            ServiceCollection.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
        }

        public static void AddTransient<TInterface, TType>()
            where TInterface : class
            where TType : class, TInterface
        {
            ServiceCollection.AddTransient<TInterface, TType>();
        }

        public static void Configure<TType>(string sectionName)
            where TType : class
        {
            ServiceCollection.Configure<TType>(GetConfiguration().GetSection(sectionName));
        }

        public static IServiceProvider CreateServiceProvider()
        {
            return ServiceCollection.BuildServiceProvider();
        }

        private static IConfigurationRoot GetConfiguration()
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json")
               .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
               .Build();
            return configuration;
        }
    }
}
