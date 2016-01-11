using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace IoTTalk.CreateDevice
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=pulcherIotHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=UJEaQv58HeIEyO6iwqUkgl0s2LIuEbTHjdLVBTxAUMI=";

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            AddDeviceAsync().Wait();

            Console.ReadLine();
        }

        private async static Task AddDeviceAsync()
        {
            var deviceId = "myIotDevice";
            Device device;

            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
                Console.WriteLine("Retrievig existing device id");
            }

            Console.WriteLine("Device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
