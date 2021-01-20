using MCListener.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCListener.Shared.Helpers
{
    public interface IPingDiagnosticMessageTransformer
    {
        PingDiagnosticResponse TranslateMessage(string message, PingDiagnosticResponseChannel channel);
    }

    public class PingDiagnosticMessageTransformer : IPingDiagnosticMessageTransformer
    {
        private ILogger<PingDiagnosticMessageTransformer> logger;

        public PingDiagnosticMessageTransformer(ILogger<PingDiagnosticMessageTransformer> logger)
        {
            this.logger = logger;
        }

        public PingDiagnosticResponse TranslateMessage(string message, PingDiagnosticResponseChannel channel)
        {
            logger.LogTrace($"Received message: {message}");

            if (message.StartsWith("MCPONG|"))
            {
                logger.LogDebug("Recognized as a pong command");
                return TranslateResponseData(message, channel);
            }

            //Fallback
            logger.LogDebug($"{message} is not a valid pong, ignoring");
            return null;
        }

        private int ResponseIdxMessageType = 0;
        private int ResponseIdxSessionId = 1;
        private int ResponseIdxPingId = 2;
        private int ResponseIdxDeviceId = 3;
        private int ResponseIdxCellularType = 4;
        private int ResponseIdxCellularProvider = 5;
        private int ResponseIdxCellularSignal = 6;
        private int ResponseIdxWifiProvider = 7;
        private int ResponseIdWifiStrength = 8;
        private int ResponseIdxBattery = 9;
        private int ResponseIdxVolume = 10;

        private PingDiagnosticResponse TranslateResponseData(string response, PingDiagnosticResponseChannel channel)
        {
            string[] spl = response.Split("|");
           
            return new PingDiagnosticResponse()
            {
                Channel = channel,
                SessionIdentifier = spl.GetFromIndex(ResponseIdxSessionId),
                PingIdentifier = spl.GetFromIndex(ResponseIdxPingId),
                ReceiverIdentifier = spl.GetFromIndex(ResponseIdxDeviceId),
                ReceiveTime = DateTime.Now,
                DeviceDetail = new PingDiagnosticResponseDeviceDetail() 
                {
                    CellularType = spl.GetFromIndex(ResponseIdxCellularType),
                    CellularProvider = spl.GetFromIndex(ResponseIdxCellularProvider),
                    CellularSignalStrength = spl.GetIntFromIndex(ResponseIdxCellularSignal),
                    WifiProvider = spl.GetFromIndex(ResponseIdxWifiProvider),
                    WifiSignalStrength = spl.GetIntFromIndex(ResponseIdWifiStrength),
                    BatteryPercentage = spl.GetIntFromIndex(ResponseIdxBattery),
                    VolumePercentage = spl.GetIntFromIndex(ResponseIdxVolume),
                }
            };
        }

    }
}
