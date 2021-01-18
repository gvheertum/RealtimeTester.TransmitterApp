using MCListener.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCListener.Shared.Helpers
{
    public interface IPingDiagnosticMessageTransformer
    {
        PingDiagnosticResponse TranslateMessage(string message);
    }

    public class PingDiagnosticMessageTransformer : IPingDiagnosticMessageTransformer
    {
        private ILogger<PingDiagnosticMessageTransformer> logger;

        public PingDiagnosticMessageTransformer(ILogger<PingDiagnosticMessageTransformer> logger)
        {
            this.logger = logger;
        }

        public PingDiagnosticResponse TranslateMessage(string message)
        {
            logger.LogTrace($"Received message: {message}");

            if (message.StartsWith("MCPONG|"))
            {
                logger.LogDebug("Recognized as a pong command");
                return TranslateResponseData(message);
            }

            //Fallback
            logger.LogDebug($"{message} is not a valid pong, ignoring");
            return null;
        }

        private PingDiagnosticResponse TranslateResponseData(string response)
        {
            string[] spl = response.Split("|");
            string sessionId = spl.GetFromIndex(1);
            string pingId = spl.GetFromIndex(2);
            string deviceId = spl.GetFromIndex(3);

            //TODO: Fill device detail
            return new PingDiagnosticResponse()
            {
                SessionIdentifier = sessionId,
                PingIdentifier = pingId,
                ReceiverIdentifier = deviceId,
                ReceiveTime = DateTime.Now,
                DeviceDetail = new PingDiagnosticResponseDeviceDetail() { }
            };
        }

    }
}
