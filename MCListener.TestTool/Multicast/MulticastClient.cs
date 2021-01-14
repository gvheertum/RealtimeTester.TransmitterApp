using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MCListener.TestTool
{
    public class MulticastClient
    {
        public int port;
        private ILogger<MulticastClient> logger;
        public string mcIP;
        
        public MulticastClient(string ip, int port, ILogger<MulticastClient> logger)
        {
            this.logger = logger;
            logger.LogDebug($"Starting client: {ip}:{port}");
            this.mcIP = ip;
            this.port = port;
        }

        public void StartListening(Action<string> callback)
        {
            new Thread(() =>{
                try
                {
                    //var mcastSocketC = GetSocketOther();
                    var mcastSocket = GetMulticastSocket(); //The other socket fails

                    
                    //Do binding here (only on reading port)
                    IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
                    mcastSocket.Bind(localEP); 

                    byte[] arr = new byte[4096];

                    while (true)
                    {
                        var receivedBytes = mcastSocket.Receive(arr);
                        logger.LogDebug($"{mcIP}:{port}.Received -> {receivedBytes} bytes");

                        var str = System.Text.Encoding.ASCII.GetString(arr).Substring(0, receivedBytes);
                        logger.LogDebug($"{mcIP}:{port}.Received -> {str}");

                        callback(str);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e.ToString());
                }
            }).Start();
        }


        public void SendMessage(string message)
        {
            var s = GetMulticastSocket();

            logger.LogDebug($"{mcIP}:{port}.Write -> {message}");

            s.Connect(new IPEndPoint(IPAddress.Parse(mcIP), port)); //We need to explicitly connect to the port before sending
            var b = System.Text.Encoding.ASCII.GetBytes(message);
            s.Send(b, b.Length, SocketFlags.None);
            s.Close();
        }

        private Socket GetMulticastSocket()
        {
            var mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress mcastAddress = IPAddress.Parse(mcIP);                
            

            MulticastOption option = new MulticastOption(mcastAddress, IPAddress.Any);              
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, option);
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 100);

            //EndPoint remoteEp = new IPEndPoint(IPAddress.Any, port);
            return mcastSocket;
        }
    }
}