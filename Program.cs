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
            int portTest = 30022;
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
            else if(args?.Any(a => a == "test") == true)
            {
                System.Console.WriteLine("Start test");
                new MulticastRoundtripTester(ip, portTest, 2000, 5000).Start();
            }
            else
            {
                System.Console.WriteLine("Use the following params:");
                System.Console.WriteLine("listen    To read from the MC channel");
                System.Console.WriteLine("write     To write to the MC channel");
                System.Console.WriteLine("test      Start the tester");
            }
        }
    }
}
