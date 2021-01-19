using System;
using MCListener.Shared.Helpers;
using MCListener.TestTool.Entities;
using MCListener.TestTool.Firebase;
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

            // Setup config
            Configure<Configuration.AzureConfiguration>(Configuration.AzureConfiguration.Section);
            Configure<Configuration.FirebaseConfiguration>(Configuration.FirebaseConfiguration.Section);
            Configure<Configuration.MulticastConfiguration>(Configuration.MulticastConfiguration.Section);
            Configure<Configuration.TesterConfiguration>(Configuration.TesterConfiguration.Section);

            // Configuration
            var configuration = GetConfiguration();
            ServiceCollection.AddSingleton(configuration);
            ServiceCollection.AddTransient<IRoundtripTester, RoundtripTester>();
            ServiceCollection.AddTransient<IMulticastClient, MulticastClient>();
            ServiceCollection.AddTransient<IFirebaseChannel, FirebaseChannel>();
            ServiceCollection.AddTransient<IPingDiagnosticContainer, PingDiagnosticContainer>();
            ServiceCollection.AddTransient<IPingDiagnosticMessageTransformer, PingDiagnosticMessageTransformer>();

            
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

        public static IConfigurationRoot GetConfiguration()
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json")
               .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
               .AddJsonFile("appsettings.secret.json", optional: true, reloadOnChange: true)
               .Build();
            return configuration;
        }
    }
}
