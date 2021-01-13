using System;
using System.Linq;

namespace MCListener
{
    class Program
    {
        static void Main(string[] args)
        {
            string ip = "236.99.250.121";
            int port = 30011;
            System.Console.WriteLine($"Starting MC client on {ip}:{port}");
            var mcc = new MulticastClient(ip, port);
            if(args?.Any(a => a == "listen") == true)
            {
                System.Console.WriteLine("Start listening, press ctrl+c to terminate");
                mcc.StartListening((r) => 
                {
                    Console.WriteLine($"Got data: {r}");
                });            
            }
            else if(args?.Any(a => a == "write") == true)
            {
                System.Console.WriteLine("Start writing");
                mcc.SendMessage("This is test");
            }
            else
            {
                System.Console.WriteLine("Use the following params:");
                System.Console.WriteLine("listen  To read from the MC channel");
                System.Console.WriteLine("write  To write to the MC channel");
            }
        }
    }
}
