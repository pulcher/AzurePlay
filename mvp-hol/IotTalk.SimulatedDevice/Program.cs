using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Azure.Devices.Client;

namespace IotTalk.SimulatedDevice
{
    class Program
    {
        static DeviceClient deviceClient;
        static string connectionString = "HostName=pulcher.azure-devices.net;DeviceId=p-virtual-sender;SharedAccessKey=txb5Kob1N6IWGCiWmhPPJwuxa5etzr2cCgzlgryZNPM=";
        //static string iotHubUri = "ddnugIotHub.azure-devices.net";
        //static string deviceKey = "m230f0zWdI+WC2YImQte63zeEpgEVU6TB8jUcIbsnwU=";
        static string deviceId = "p-virtual-send";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device:\n");

            deviceClient = DeviceClient.CreateFromConnectionString(connectionString);
            //deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));

            SendDeviceToCloudMessagesAsync();

            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            var avgWindSpeed = 10d;
            var rand = new Random();

            while (true)
            {
                var currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;

                var telemetryDataPoint = new
                {
                    deviceId = deviceId,
                    windoSpeed = currentWindSpeed
                };

                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);

                Console.WriteLine("{0} > sending message: {1}", DateTime.Now, messageString);

                Thread.Sleep(1000);
            }
        }
    }
}
