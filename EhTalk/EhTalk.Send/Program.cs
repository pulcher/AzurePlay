using System;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus.Messaging;

namespace EhTalk.Send
{
    class Program
    {
        static string eventHubName = "rpidemo";
        static string connectionString = "Endpoint=sb://rpidemo-ns.servicebus.windows.net/;SharedAccessKeyName=SendRule;SharedAccessKey=OvP3AP8dXkR8Wm2hSGOTaAcV2FamGU3WC2VN6wJqGE8=";

        static void Main(string[] args)
        {
            Console.WriteLine("Press Ctrl-C to stop the sender process");
            Console.WriteLine("Press Enter to start now");
            Console.ReadLine();
            SendingRandomMessages();
        }

        static void SendingRandomMessages()
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);
            while (true)
            {
                try
                {
                    var message = Guid.NewGuid().ToString();
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, message);
                    eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                    Console.ReadLine();
                }

                Thread.Sleep(200);
            }
        }
    }
}
