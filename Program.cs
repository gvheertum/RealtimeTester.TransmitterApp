using System;
using System.Net;
using System.Net.Sockets;

namespace MCListener
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                int mcastPort = 30011;
                IPAddress mcastAddress = IPAddress.Parse("236.99.250.121");                
                IPAddress localIPAddr = IPAddress.Parse("172.16.23.158");                
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, mcastPort);          
                mcastSocket.Bind(localEP);

                MulticastOption option = new MulticastOption(mcastAddress, localIPAddr);              
                mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, option);

                EndPoint remoteEp = new IPEndPoint(localIPAddr, mcastPort);

                byte[] arr = new byte[4096];
                
                while (true)
                {
                    var receivedBytes = mcastSocket.ReceiveFrom(arr, ref remoteEp);
                    var str = System.Text.Encoding.ASCII.GetString(arr).Substring(0, receivedBytes);
                    Console.WriteLine($"Got data {receivedBytes} bytes: {str}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
