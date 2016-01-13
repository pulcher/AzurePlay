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
        static string iotHubUri = "pulcherIotHub.azure-devices.net";
        static string deviceKey = "gFjOKTCukqFGP/icY4BoGTae7M0pTF5fNY2DBjSgHvQ=";
        static string deviceId = "myIotDevice";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device:\n");

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));

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

/*
<Page x:Class="FEZHATDemo.MainPage" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:local="using:App1" xmlns:d="http://schemas.microsoft.com/expression/blend/2008" xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" mc:Ignorable="d">
    <StackPanel Orientation="Horizontal" Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
        <StackPanel Width="75">
            <TextBlock Text="Light: " />
            <TextBlock Text="Temp: " />
            <TextBlock Text="Accel: " />
            <TextBlock Text="Button 18: " />
            <TextBlock Text="Button 22: " />
            <TextBlock Text="Leds: " />
            <TextBlock Text="Analog: " />
        </StackPanel>
        <StackPanel>
            <TextBlock Name="LightTextBox" />
            <TextBlock Name="TempTextBox" />
            <TextBlock Name="AccelTextBox" />
            <TextBlock Name="Button18TextBox" />
            <TextBlock Name="Button22TextBox" />
            <TextBlock Name="LedsTextBox" />
            <TextBlock Name="AnalogTextBox" />
        </StackPanel>
    </StackPanel>
</Page
    */
