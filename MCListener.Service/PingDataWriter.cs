using Microsoft.Extensions.Logging;
using MCListener.Shared;
using Microsoft.Extensions.Configuration;
using System;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MCListener.Service
{
    public class PingDataWriter
    {
        private ILogger logger;
        private IConfigurationRoot config;
        private const string ConnectionStringNameStorage = "OCCPingDiagnostics";
        public PingDataWriter(IConfigurationRoot config, ILogger logger)
        {
            this.logger = logger;
            this.config = config;
            if (string.IsNullOrWhiteSpace(config.GetConnectionString(ConnectionStringNameStorage))) { throw new Exception($"Connection string: {ConnectionStringNameStorage} missing"); }
        }

        public void RegisterPingData(PingDiagnostic diagnostic)
        {
            logger.LogInformation($"Processing storage of diagnostic with {diagnostic.Responders?.Count ?? 0} responses");
            RegisterPingRequest(diagnostic);
            diagnostic?.Responders?.ForEach(r => RegisterPongResponse(diagnostic, r));
        }

        private const string SQLStoreRequest = @"
            INSERT INTO [dbo].[PingRequest]
	            ([SessionIdentifier],[PingIdentifier],[StartTime])
            VALUES
                (@SessionIdentifier,@PingIdentifier,@StartTime)
            ";
        private void RegisterPingRequest(PingDiagnostic diagnostic)
        {
            logger.LogInformation($"Writing request: {diagnostic.PingIdentifier}");
            RunQuery(SQLStoreRequest, new
            {
                SessionIdentifier = diagnostic.SessionIdentifier,
                PingIdentifier = diagnostic.PingIdentifier,
                StartTime = diagnostic.StartTime
            });
        }

        private const string SQLStoreResponse = @"
            INSERT INTO [dbo].[PingResponse]
	            ([SessionIdentifier],[PingIdentifier],[ReceiverIdentifer],[ReceiveTime],[Channel],[CellularType],[CellularProvider],[CellularSignalStrength],[WifiProvider],[WifiSignalStrength],[BatteryPercentage],[VolumePercentage])
            VALUES
	            (@SessionIdentifier,@PingIdentifier,@ReceiverIdentifer,@ReceiveTime,@Channel,@CellularType,@CellularProvider,@CellularSignalStrength,@WifiProvider,@WifiSignalStrength,@BatteryPercentage,@VolumePercentage)
        ";

        private void RegisterPongResponse(PingDiagnostic diagnostic, PingDiagnosticResponse response)
        {
            logger.LogInformation($"Writing response: {response.ReceiverIdentifier} for ping {diagnostic.PingIdentifier}");
            RunQuery(SQLStoreResponse, new {
                SessionIdentifier = diagnostic.SessionIdentifier,
                PingIdentifier = diagnostic.PingIdentifier,
                ReceiverIdentifer = response.ReceiverIdentifier,
                ReceiveTime = response.ReceiveTime,
                Channel = response.Channel.ToString(),
                CellularType = response.DeviceDetail.CellularType,
                CellularProvider = response.DeviceDetail.CellularProvider,
                CellularSignalStrength = response.DeviceDetail.CellularSignalStrength,
                WifiProvider = response.DeviceDetail.WifiProvider,
                WifiSignalStrength = response.DeviceDetail.WifiSignalStrength,
                BatteryPercentage = response.DeviceDetail.BatteryPercentage,
                VolumePercentage = response.DeviceDetail.VolumePercentage
            });
        }

        private void RunQuery<T>(string query, T data)
        {
            try
            {
                using (var connection = new SqlConnection(config.GetConnectionString(ConnectionStringNameStorage)))
                {
                    connection.Query(query, data);
                }
            }catch(Exception e)
            {
                logger.LogError($"Cannot execute query: {e.Message}");
            }
        }
    }
}
