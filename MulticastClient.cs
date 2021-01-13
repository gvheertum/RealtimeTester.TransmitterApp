using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MCListener
{
    public class MulticastClient
    {
        public int port;
        public string mcIP;
        
        public MulticastClient(string ip, int port)
        {
            System.Console.WriteLine($"Starting client: {ip}:{port}");
            this.mcIP = ip;
            this.port = port;
        }

        public void StartListening(Action<string> callback)
        {
            new Thread(() =>{
                try
                {
                    var mcastSocketC = GetMulticastSocket();
                    var mcastSocket = mcastSocketC.socket;
                    var remoteEp = mcastSocketC.ep;

                    byte[] arr = new byte[4096];
                    
                    while (true)
                    {
                        var receivedBytes = mcastSocket.ReceiveFrom(arr, ref remoteEp);
                        System.Console.WriteLine("Received bytes");
                        var str = System.Text.Encoding.ASCII.GetString(arr).Substring(0, receivedBytes);
                        callback(str);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }).Start();
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

        private (Socket socket, EndPoint ep) GetMulticastSocket()
        {
            var mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress mcastAddress = IPAddress.Parse(mcIP);                
            IPAddress localIPAddr = IPAddress.Parse("127.0.0.1");    

            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);          
            mcastSocket.Bind(localEP);

            MulticastOption option = new MulticastOption(mcastAddress, IPAddress.Any);              
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, option);
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 100);

            EndPoint remoteEp = new IPEndPoint(localIPAddr, port);
            return (mcastSocket, remoteEp);
        }
    }
}