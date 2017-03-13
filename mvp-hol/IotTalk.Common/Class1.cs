namespace IotTalk.Common
{
    public class CommandPayload
    {
        public string Message { get; set; }
        public bool Mode { get; set; }
    }

    public class SensorPayload
    {
        public double LightLevel { get; set; }
        public double Temp { get; set; }
        public double Analog { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
