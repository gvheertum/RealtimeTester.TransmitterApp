using System;
using System.Linq;
using System.Threading;
using MCListener.Shared;
using MCListener.Shared.Helpers;
using MCListener.TestTool.Configuration;
using MCListener.TestTool.Entities;
using MCListener.TestTool.Firebase;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MCListener.TestTool
{
    public interface IRoundtripTester
    {
        void Start();
    }

    public class RoundtripTester : IRoundtripTester
    {
        private TesterConfiguration configuration;
        private ILogger<RoundtripTester> logger;
        private IPingDiagnosticContainer container;
        private IMulticastClient multicastClient;
        private string sessionIdentifier;
        private IPingDiagnosticMessageTransformer transformer;
        private IFirebaseChannel firebaseChannel;

        public RoundtripTester(IMulticastClient multicast, IOptions<Configuration.TesterConfiguration> configuration, IFirebaseChannel firebaseChannel, IPingDiagnosticContainer container, IPingDiagnosticMessageTransformer transformer, ILogger<RoundtripTester> logger)
        {
            this.configuration = configuration.Value;
            this.configuration.AssertValidity();

            this.logger = logger;
            this.container = container;
            multicastClient = multicast;
            this.sessionIdentifier = GenerateIdentifier();
            this.transformer = transformer;
            this.firebaseChannel = firebaseChannel;
        }

        public void Start()
        {
            //Registering receiver
            logger.LogDebug("Starting reader");
            if (configuration.TestMulticast) { multicastClient.StartListening((s) => ProcessPongResponse(s, PingDiagnosticResponseChannel.Multicast)); }
            if (configuration.TestFirebase) { firebaseChannel.StartReceiving((s) => ProcessPongResponse(s, PingDiagnosticResponseChannel.Firebase)); }

            //Doing send
            logger.LogDebug($"Starting writer with sessionid: {sessionIdentifier}");
            while(true)
            {
                var pingIdentifier = GenerateIdentifier();                
                logger.LogDebug($"Ping: {pingIdentifier}");
                
                // Send the outgoing message
                var tripData = container.RegisterTripStart(sessionIdentifier, pingIdentifier);
                TransmitPing(tripData);

                HandleFinalizeOfPing(tripData, configuration.WaitMS); //Schedule the way for resolve thread
                
                //Sleep until sending the netxt ping.
                Thread.Sleep(configuration.IntervalMS);
            }
        }

        private void TransmitPing(PingDiagnostic ping)
        {
            //TODO: The order of execution might influence how soon everything is processed, however this might be a bit overdone to make this a set of parallel threads
            try
            {
                if (configuration.TestFirebase) { firebaseChannel.WritePing(ping); }
                if (configuration.TestMulticast) { multicastClient.SendMessage($"MCPING|{ping.SessionIdentifier}|{ping.PingIdentifier}"); }
            }catch(Exception e)
            {
                logger.LogWarning($"Failed to transmit ping: {e.Message}");
            }
        }

        private void HandleFinalizeOfPing(PingDiagnostic roundtrip, int sleep)
        {
            new Thread(() => {
                Thread.Sleep(sleep);
                logger.LogDebug($"Waking up to resolve ping: {roundtrip.SessionIdentifier}");
                CleanupPingResult(roundtrip);
                OutputPingResult(roundtrip);
            }).Start();
        }

        private bool ProcessPongResponse(string response, PingDiagnosticResponseChannel channel)
        {
            var msgData = transformer.TranslateMessage(response, channel);
            logger.LogDebug($"Handing Firebase request: {msgData.SessionIdentifier}|{msgData.PingIdentifier}");
            if (string.IsNullOrWhiteSpace(msgData?.PingIdentifier)) { return false; }
            if (msgData.SessionIdentifier != this.sessionIdentifier) { logger.LogDebug($"Found responses for other session: {msgData.SessionIdentifier} "); return false; }

            container.RegisterTripResponse(msgData);
            return true;
        }
        private void OutputPingResult(PingDiagnostic roundtrip)
        {
            OutputPingToLog(roundtrip);

        }

        private void CleanupPingResult(PingDiagnostic roundtrip)
        {
            firebaseChannel.DisposePing(roundtrip).GetAwaiter().GetResult();
            container.PurgeTripResponse(roundtrip);
        }

        private void OutputPingToLog(PingDiagnostic roundtrip)
        {
            if (roundtrip.IsSuccess)
            {
                string formattedReplies = String.Join("|", roundtrip.Responders.Select(r => FormatReply(r)));
                logger.LogInformation($"{{{roundtrip.StartTime.ToString("HH:mm:ss.fff")}|{sessionIdentifier}|{roundtrip.PingIdentifier}|SUCCESS|{{{formattedReplies}}}}}");
            }
            else
            {
                logger.LogCritical($"{{{roundtrip.StartTime.ToString("HH:mm:ss.fff")}|{sessionIdentifier}|{roundtrip.PingIdentifier}|FAILED}}");
            }
        }


        private string FormatReply(PingDiagnosticResponse r)
        {
            string resp = $"{{{r.ReceiveTime.ToString("HH:mm:ss.fff")}|{r.ReceiverIdentifier}";
            resp += $"|channel:{r.Channel.ToString()}";
            resp += $"|gsm:{r.DeviceDetail.CellularType}:{r.DeviceDetail.CellularProvider}:{r.DeviceDetail.CellularSignalStrength}";
            resp += $"|wifi:{r.DeviceDetail.WifiProvider}:{r.DeviceDetail.WifiSignalStrength}";
            resp += $"|batt:{r.DeviceDetail.BatteryPercentage}";
            resp += $"|vol:{r.DeviceDetail.VolumePercentage}}}";
            return resp;
        }

        private string GenerateIdentifier()
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }
    }
}