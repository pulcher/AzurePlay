using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace IoTTalk.DeviceToCloud
{
    class Program
    {
        static string connectionString = "HostName=ddnugIotHub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=RBkZQjQzBm5HiqqRyDUfIbNGX2yeoCDroBtarhRP3/c=";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Receive messages:\n");

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            foreach(var partition in d2cPartitions)
            {
                ReceiveMessagesFromDeviceAsync(partition);
            }

            Console.ReadLine();
        }

        private async static Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);

            while(true)
            {
                var eventData = await eventHubReceiver.ReceiveAsync();

                if (eventData == null) continue;

                var data = Encoding.UTF8.GetString(eventData.GetBytes());

                Console.WriteLine("Message received. Partition: {0} Data: {1}", partition, data);
            }
        }
    }
}
