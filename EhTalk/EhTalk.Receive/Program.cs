using System;
using EhTalk.Common;
using Microsoft.ServiceBus.Messaging;

namespace EhTalk.Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            string eventHubConnectionString = "Endpoint=sb://ehcool.servicebus.windows.net/;SharedAccessKeyName=ReaderDude;SharedAccessKey=4vf2vvtPVwUF39feTGI6bUaixNGL2K1kgtvfUTS+RWo=";
            string eventHubName = "coolness";
            string storageAccountName = "pulchereventhubstorage";
            string storageAccountKey = "7DBdiHW1JI8eIOiaZam37J8VVOvyrvrc+rTP5rIz2w0JDshbybv/+fbjwM63xEPQ5px+ejelThoIPVJHer5 + Gg == ";
            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
            storageAccountName, storageAccountKey);

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>().Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
