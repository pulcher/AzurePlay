using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Azure.Devices.Client;
using System.Text;
using GHIElectronics.UWP.Shields;

namespace IoTTalk.Uwp
{
    public sealed partial class MainPage : Page
    {
        private FEZHAT _hat;
        private DispatcherTimer _timer;
        static string connectionString = "HostName=pulcher.azure-devices.net;DeviceId=p-rpi3-demo;SharedAccessKey=Vj6zwPb3Ht1mbY3R7i/weLYzafDT2A0VU+1/keX0i5Q=";
        //static string iotHubUri = "pulcherIotHub.azure-devices.net";
        static string deviceId = "p-rpi3-demo";
        //static string deviceKey = "ERWU6n6lZVzNqw+42k3Vip0tOmmJGr1OiSSgYzp5j5Q=";
        static double lightLevel, x, y, z, temp, analog;
        static DeviceClient _deviceClient;
        static string receivedCommand = "blah";

        public MainPage()
        {
            this.InitializeComponent();

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
            // _deviceClient = DeviceClient.CreateFromConnectionString(connectionString,);

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
            LightTextBox.Text = lightLevel.ToString("P2");
            TempBox.Text = temp.ToString("N3");
            AnalogBox.Text = analog.ToString("N2");
            AccelBox.Text = $"({x:N2}, {y:N2}, {z:N2})";

            ErrorBox.Text = receivedCommand;
        }

        static async void SendDeviceToCloudMessagesAsync(DeviceClient deviceClient)
        {
            //var deviceClient = DeviceClient.Create(iotHubUri,
            //    AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),
            //    TransportType.Http1);
            //var deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Http1);

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
                receivedCommand = messageData;
                await deviceClient.CompleteAsync(receivedMessage);
            }
            else
            {
                //receivedCommand = "No Command";
            }

        }
    }
}
