using System;
using System.Text;
using System.Threading.Tasks;
using IotTalk.Common;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace IoTTalk.DeviceToCloud
{
    class Program
    {
        static string connectionString = "HostName=marsiot.azure-devices.net;SharedAccessKeyName=coffeeclient;SharedAccessKey=FkwDl0J3LAI31zo0Q2ThLAXgIUlIhIY3kQUaIUDHgmU=";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Receive messages:\n");

            //eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            //var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            foreach (var partition in d2cPartitions)
            {
                ReceiveMessagesFromDeviceAsync(partition);
            }

            Console.ReadLine();
        }

        private async static Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = eventHubClient.GetConsumerGroup("team06").CreateReceiver(partition, DateTime.Now);

            while(true)
            {
                var eventData = await eventHubReceiver.ReceiveAsync();

                if (eventData == null) continue;

                var data = Encoding.UTF8.GetString(eventData.GetBytes());

                //var otherData = JsonConvert.DeserializeObject<SensorPayload>(data); //new Message(eventData.GetBytes());

                //var sysproperties = new StringBuilder();

                //foreach(var item in eventData.SystemProperties)
                //{
                //    sysproperties.Append($"key: {item.Key} ");
                //    sysproperties.Append($"value: {item.Value}\n");
                //}
                //var deviceId = eventData.SystemProperties["iothub-connection-device-id"];

                Console.WriteLine("Message received. Partition: {0} Data: {1}", partition, data);
                //Console.WriteLine($"Device Id: {deviceId}");
                //Console.WriteLine(sysproperties.ToString());
            }
        }
    }
}
