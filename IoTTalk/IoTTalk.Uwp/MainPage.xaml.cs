using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Azure.Devices.Client;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IoTTalk.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            SendDeviceToCloudMessagesAsync();
        }

        static async void SendDeviceToCloudMessagesAsync()
        {
            var iotHubUri = "pulcherIotHub.azure-devices.net";
            var deviceId = "uwpDevice";
            var deviceKey = "ERWU6n6lZVzNqw+42k3Vip0tOmmJGr1OiSSgYzp5j5Q=";
            
            //var deviceClient = DeviceClient.CreateFromConnectionString(connectionString, deviceId, TransportType.Http1);
            var deviceClient = DeviceClient.Create(iotHubUri,
                AuthenticationMethodFactory.CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),
                TransportType.Http1);

            var str = "Hello, from uwp";
            var message = new Message(Encoding.ASCII.GetBytes(str));

            await deviceClient.SendEventAsync(message);
        }
    }
}
