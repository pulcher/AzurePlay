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
        static string connectionString = "HostName=ddnugIotHub.azure-devices.net;DeviceId=uwpDevice;SharedAccessKey=lSqRxjSGYYNrwpbGkQXSjG9CA/MY/u9yqosnLYAvD44=";
        //static string iotHubUri = "pulcherIotHub.azure-devices.net";
        static string deviceId = "uwpDevice";
        //static string deviceKey = "ERWU6n6lZVzNqw+42k3Vip0tOmmJGr1OiSSgYzp5j5Q=";
        static double lightLevel, x, y, z, temp, analog;
        static DeviceClient _deviceClient;

        public MainPage()
        {
            this.InitializeComponent();

            Setup();

            //SendDeviceToCloudMessagesAsync();
        }

        private async void Setup()
        {
            try
            {
                _hat = await FEZHAT.CreateAsync();
            }
            catch (Exception)
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
            if (_hat != null)
            {
                
                lightLevel = _hat.GetLightLevel();
                temp = _hat.GetTemperature();
                analog = _hat.ReadAnalog(FEZHAT.AnalogPin.Ain1);

                _hat.GetAcceleration(out x, out y, out z);
            }

            SendDeviceToCloudMessagesAsync();
            UpdateScreen();
        }

        private void UpdateScreen()
        {
            LightTextBox.Text = lightLevel.ToString("P2");
            TempBox.Text = temp.ToString("N3");
            AnalogBox.Text = analog.ToString("N2");
            AccelBox.Text = $"({x:N2}, {y:N2}, {z:N2})";
        }

        static async void SendDeviceToCloudMessagesAsync()
        {
            //var deviceClient = DeviceClient.Create(iotHubUri,
            //    AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),
            //    TransportType.Http1);
            var deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Http1);

            var package = $"{{lightLevel: {lightLevel}, temp: {temp:N3}, analog: {analog:N2}, x: {x}, y: {y}, z: {z} }}";
            var message = new Message(Encoding.ASCII.GetBytes(package));

            await deviceClient.SendEventAsync(message);
        }
    }
}
