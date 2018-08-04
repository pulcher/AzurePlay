using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Azure.Devices.Client;
using System.Text;
using GHIElectronics.UWP.Shields;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Newtonsoft.Json;

namespace IoTTalk.Uwp
{
    public sealed partial class MainPage : Page
    {
        static string connectionString = "HostName=pulcher.azure-devices.net;DeviceId=p-rpi3-demo;SharedAccessKey=Vj6zwPb3Ht1mbY3R7i/weLYzafDT2A0VU+1/keX0i5Q=";
        static string deviceId = "p-rpi3-demo";
        static double lightLevel, x, y, z, temp, analog;
        static DeviceClient _deviceClient;
        static bool _nightMode = false;
        static FEZHAT.Color _ledColor = FEZHAT.Color.Green;
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

                if(_ledColor.G == 255)
                {
                    _ledColor = FEZHAT.Color.Magneta;
                    _hat.D2.Color = FEZHAT.Color.Magneta;
                    _hat.D3.Color = FEZHAT.Color.Green;
                }
                else
                {
                    _ledColor = FEZHAT.Color.Green;
                    _hat.D2.Color = FEZHAT.Color.Green;
                    _hat.D3.Color = FEZHAT.Color.Magneta;
                }
                
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
            var package = $"{{LightLevel: {lightLevel}, Temp: {temp:N3}, Analog: {analog:N2}, X: {x}, Y: {y}, Z: {z} }}";
            var sensorPayload = new SensorPayload
            {
                LightLevel = lightLevel,
                Temp = temp,
                Analog = analog,
                X = x,
                Y = y,
                Z = z
            };

            var jsonPackage = JsonConvert.SerializeObject(sensorPayload);

            var message = new Message(Encoding.ASCII.GetBytes(jsonPackage));

            if(deviceClient != null)
                await deviceClient.SendEventAsync(message);
        }

        // Receive messages from IoT Hub
        static async void ReceiveCommands(DeviceClient deviceClient)
        {
            // sample JSON {"Message":"this is a thing","Mode": 0}

            Message receivedMessage = null;
            string data;

                if (deviceClient != null)
                    receivedMessage = await deviceClient.ReceiveAsync();
               
            if(receivedMessage != null)
            {
                data = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                try
                {
                    var otherData = JsonConvert.DeserializeObject<CommandPayload>(data);

                    if (!string.IsNullOrEmpty(otherData.Message))
                        _nightMode = otherData.Mode;

                    _receivedCommand = data;
                }
                catch (Exception ex)
                {
                    var test = ex;
                    // eating this for now.  someone sent in some trash
                }

                await deviceClient.CompleteAsync(receivedMessage);
            }
            else
            {
                //receivedCommand = "No Command";
            }

        }

        public class CommandPayload
        {
            public string Message {
                get; set;
            }
            public bool Mode {
                get; set;
            }
        }

        public class SensorPayload
        {
            public double LightLevel {
                get; set;
            }
            public double Temp {
                get; set;
            }
            public double Analog {
                get; set;
            }
            public double X {
                get; set;
            }
            public double Y {
                get; set;
            }
            public double Z {
                get; set;
            }
        }
    }
}
