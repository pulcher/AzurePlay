using System;
using EhTalk.Common;
using Microsoft.ServiceBus.Messaging;

namespace EhTalk.Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            string eventHubConnectionString = "Endpoint=sb://trustme.servicebus.windows.net/;SharedAccessKeyName=reader;SharedAccessKey=jmNsxYKoYw3fV2XdfuDdWE5x2EdT3BTu5UKkR+8gFH8=";
            string eventHubName = "nddtest";
            string storageAccountName = "pulcherstorage1";
            string storageAccountKey = "8KAT7HqDpKNKltJ5u/OXEyMHPeyKUueEDhr5Lw3uEe3Xp4jqO201Pk2ZrH2/XvegXOkfSytm4R8APmgNfNastg==";
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
