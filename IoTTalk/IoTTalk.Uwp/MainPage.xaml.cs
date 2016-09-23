﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Azure.Devices.Client;
using System.Text;
using GHIElectronics.UWP.Shields;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace IoTTalk.Uwp
{
    public sealed partial class MainPage : Page
    {
        //const string BgDay = "Honeydew";
        //const string BgNight = "Indigo";
        //const string FgDay = "DarkViolet";
        //const string FgNight = "Gold";

        static string connectionString = "HostName=pulcher.azure-devices.net;DeviceId=p-rpi3-demo;SharedAccessKey=Vj6zwPb3Ht1mbY3R7i/weLYzafDT2A0VU+1/keX0i5Q=";
        static string deviceId = "p-rpi3-demo";
        static double lightLevel, x, y, z, temp, analog;
        static DeviceClient _deviceClient;
        static bool _nightMode = false;
        static string _receivedCommand = "blah";

        private FEZHAT _hat;
        private DispatcherTimer _timer;

        public MainPage()
        {
            InitializeComponent();

            Setup();

            SendDeviceToCloudMessagesAsync(_deviceClient);
        }

        private async void Setup()
        {
            try
            {
                _hat = await FEZHAT.CreateAsync();
            }
            catch(Exception)
            {

                ErrorBox.Text = "Could not initialize Hat";
            }

            _deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Http1);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(2000);
            _timer.Tick += this.OnTick;
            _timer.Start();
        }

        private void OnTick(object sender, object e)
        {
            if(_hat != null)
            {

                lightLevel = _hat.GetLightLevel();
                temp = _hat.GetTemperature();
                analog = _hat.ReadAnalog(FEZHAT.AnalogPin.Ain1);

                _hat.GetAcceleration(out x, out y, out z);
            }

            SendDeviceToCloudMessagesAsync(_deviceClient);
            ReceiveCommands(_deviceClient);
            UpdateScreen();
        }

        private void UpdateScreen()
        {
            if(_nightMode)
            {
                LightTextBox.Foreground = new SolidColorBrush(Colors.Gold);
                TempBox.Foreground      = new SolidColorBrush(Colors.Gold);
                AnalogBox.Foreground    = new SolidColorBrush(Colors.Gold);
                AccelBox.Foreground     = new SolidColorBrush(Colors.Gold);

                LblLightTextBox.Foreground = new SolidColorBrush(Colors.Gold);
                LblTempBox.Foreground      = new SolidColorBrush(Colors.Gold);
                LblAnalogBox.Foreground    = new SolidColorBrush(Colors.Gold);
                LblAccelBox.Foreground     = new SolidColorBrush(Colors.Gold);

                MainGrid.Background     = new SolidColorBrush(Colors.Indigo);
            }
            else
            {
                LightTextBox.Foreground = new SolidColorBrush(Colors.DarkViolet);
                TempBox.Foreground      = new SolidColorBrush(Colors.DarkViolet);
                AnalogBox.Foreground    = new SolidColorBrush(Colors.DarkViolet);
                AccelBox.Foreground     = new SolidColorBrush(Colors.DarkViolet);

                LblLightTextBox.Foreground = new SolidColorBrush(Colors.DarkViolet);
                LblTempBox.Foreground      = new SolidColorBrush(Colors.DarkViolet);
                LblAnalogBox.Foreground    = new SolidColorBrush(Colors.DarkViolet);
                LblAccelBox.Foreground     = new SolidColorBrush(Colors.DarkViolet);

                MainGrid.Background = new SolidColorBrush(Colors.Honeydew);
            }

            var f = (temp * 9)/ 5.0 + 32;

            LightTextBox.Text = lightLevel.ToString("P2");
            TempBox.Text = $"{temp.ToString("N2")}C ({f.ToString("N2")}F)";
            AnalogBox.Text = analog.ToString("N2");
            AccelBox.Text = $"({x:N2}, {y:N2}, {z:N2})";

            ErrorBox.Text = _receivedCommand;
        }

        static async void SendDeviceToCloudMessagesAsync(DeviceClient deviceClient)
        {
            var package = $"{{lightLevel: {lightLevel}, temp: {temp:N3}, analog: {analog:N2}, x: {x}, y: {y}, z: {z} }}";
            var message = new Message(Encoding.ASCII.GetBytes(package));

            if(deviceClient != null)
                await deviceClient.SendEventAsync(message);
        }

        // Receive messages from IoT Hub
        static async void ReceiveCommands(DeviceClient deviceClient)
        {
            Message receivedMessage = null;
            string messageData;

                if (deviceClient != null)
                    receivedMessage = await deviceClient.ReceiveAsync();
               
            if(receivedMessage != null)
            {
                messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                _receivedCommand = messageData;
                await deviceClient.CompleteAsync(receivedMessage);
            }
            else
            {
                //receivedCommand = "No Command";
            }

        }
    }
}
