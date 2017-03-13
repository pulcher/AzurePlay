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
        static string connectionString = "HostName=pulcher.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=3XFxFitmMLMFCCmr0MfQYfsQ1L2C3GtGSAXbM0wC6Bc=";

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            AddDeviceAsync().Wait();

            Console.ReadLine();
        }

        private async static Task AddDeviceAsync()
        {
            var simulatedDeviceId = "myIotDeviceHTF";
            Device simulatedDevice;

            var uwpDeviceId = "uwpDeviceHTF";
            Device uwpDevice;

            try
            {
                simulatedDevice = await registryManager.AddDeviceAsync(new Device(simulatedDeviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                simulatedDevice = await registryManager.GetDeviceAsync(simulatedDeviceId);
                Console.WriteLine("Retrieving existing device id");
            }

            try
            {
                uwpDevice = await registryManager.AddDeviceAsync(new Device(uwpDeviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                uwpDevice = await registryManager.GetDeviceAsync(uwpDeviceId);
                Console.WriteLine("Retrievig existing device id");
            }

            Console.WriteLine("Simulated Device key: {0}", simulatedDevice.Authentication.SymmetricKey.PrimaryKey);
            Console.WriteLine("Uwp Device key: {0}", uwpDevice.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
