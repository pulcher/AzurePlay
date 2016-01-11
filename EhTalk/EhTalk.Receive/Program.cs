using System;
using EhTalk.Common;
using Microsoft.ServiceBus.Messaging;

namespace EhTalk.Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            string eventHubConnectionString = "<reader connection string>";
            string eventHubName = "<hub name>";
            string storageAccountName = "<storage account name";
            string storageAccountKey = "<storage account key>";
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
