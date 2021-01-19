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
            multicastClient.StartListening(ProcessResponse);

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
        //TODO: do something with the sessionID

        private void TransmitPing(PingDiagnostic ping)
        {
            firebaseChannel.WritePing(ping).GetAwaiter().GetResult();
            multicastClient.SendMessage($"MCPING|{ping.SessionIdentifier}|{ping.PingIdentifier}");
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

        
        private void ProcessResponse(string response)
        {
            var msgData = transformer.TranslateMessage(response);
            if(string.IsNullOrWhiteSpace(msgData?.PingIdentifier)) { return; } //Invalid message (or not for us, so ignore it)
            if(msgData.SessionIdentifier != this.sessionIdentifier) { logger.LogDebug($"Found responses for other session: {msgData.SessionIdentifier} ");  return; }
            container.RegisterTripResponse(msgData);
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
            string resp = $"{r.ReceiveTime.ToString("HH:mm:ss.fff")}|{r.ReceiverIdentifier}";
            resp += $"|gsm:{r.DeviceDetail.CellularType}:{r.DeviceDetail.CellularProvider}:{r.DeviceDetail.CellularSignalStrength}";
            resp += $"|wifi:{r.DeviceDetail.WifiProvider}:{r.DeviceDetail.WifiSignalStrength}";
            resp += $"|batt:{r.DeviceDetail.BatteryPercentage}";
            resp += $"|vol:{r.DeviceDetail.VolumePercentage}";
            return resp;
        }

        private string GenerateIdentifier()
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }
    }


    //TODO: Split over the MC and FB version
    //TODO: make config file
}