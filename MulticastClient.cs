using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace MCListener
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
                    // var mcastSocketC = GetMulticastSocket();
                    // var mcastSocket = mcastSocketC.socket;
                    // var remoteEp = mcastSocketC.ep;

                    // byte[] arr = new byte[4096];
                    
                    // while (true)
                    // {
                    //     var receivedBytes = mcastSocket.Receive(arr);//, ref remoteEp);
                    //     logger.LogDebug("Received bytes");
                    //     var str = System.Text.Encoding.ASCII.GetString(arr).Substring(0, receivedBytes);
                    //     callback(str);
                    // }

            logger.LogDebug("Received data");

                    var udpclient = GetUdpClient();
                    udpclient.BeginReceive(new AsyncCallback((ar) => ReceivedCallback(ar, udpclient, callback)), null);
                    while(true)
                    {
                        Thread.Sleep(1000); //Keep me alive!
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e.ToString());
                }
            }).Start();
        }

        private void ReceivedCallback(IAsyncResult ar, UdpClient udpclient, Action<string> callback)
        {
            logger.LogDebug("Received data");
            if (udpclient != null)
            {
                try
                {
                    // Get received data.
                    IPEndPoint sender = new IPEndPoint(0, 0);
                    byte[] receivedBytes = udpclient.EndReceive(ar, ref sender);
                logger.LogDebug("ending receive listen");
                    string receivedString = Encoding.ASCII.GetString(receivedBytes);

                    // Deserialize string
                    callback(receivedString);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while handling received multicast message");
                }

                //Restart listening
                logger.LogDebug("restart listen");
                udpclient.BeginReceive(new AsyncCallback((ar) => ReceivedCallback(ar, udpclient, callback)), null);
            }
        }

        public void SendMessage(string message)
        {
            Socket s= GetSocketOther();
            var b = System.Text.Encoding.ASCII.GetBytes(message);            
            s.Send(b,b.Length,SocketFlags.None);
            s.Close();            
        }

        private Socket GetSocketOther()
        {
            Socket s=new Socket(AddressFamily.InterNetwork, 
				SocketType.Dgram, ProtocolType.Udp);
            IPAddress ip=IPAddress.Parse(mcIP);
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip));
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 100);
            IPEndPoint ipep=new IPEndPoint(ip, port);
            s.Connect(ipep);
            return s;
        }

        private UdpClient GetUdpClient()
        {
            var multicastIPaddress = IPAddress.Parse(mcIP);
            var localIPaddress = IPAddress.Any;
            
            var    remoteEndPoint = new IPEndPoint(multicastIPaddress, port);
            var    localEndPoint = new IPEndPoint(localIPaddress, port);

               var udpclient = new UdpClient();
                udpclient.ExclusiveAddressUse = false;
                udpclient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpclient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
                udpclient.Client.Bind(localEndPoint);
                
                udpclient.JoinMulticastGroup(multicastIPaddress, localIPaddress);
                
            return udpclient;

        }

        private (Socket socket, EndPoint ep) GetMulticastSocket()
        {
            var mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress mcastAddress = IPAddress.Parse(mcIP);                
            //IPAddress localIPAddr = IPAddress.Parse("127.0.0.1");    
            //IPAddress localIPAddr = IPAddress.Parse("192.168.249.49");    

            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);          
            mcastSocket.Bind(localEP);

            MulticastOption option = new MulticastOption(mcastAddress, IPAddress.Any);              
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, option);
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 100);

            EndPoint remoteEp = new IPEndPoint(IPAddress.Any, port);
            return (mcastSocket, remoteEp);



        }
    }
}