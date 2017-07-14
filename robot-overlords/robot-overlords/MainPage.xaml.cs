using GoPiGo;
using GoPiGo.Sensors;
using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace robot_overlords
{
    public sealed partial class MainPage : Page
    {
        private string _subscriptionKey = " 85ce8f79af0248a6910259be8eab3931";
        private readonly string _photoFileName = "roboPhoto.jpg";
        private readonly int _defaultSpeed = 96;
        private readonly int _slowSpeed = 64;

        private MediaCapture mediaCapture;
        private StorageFile photoFile;
        private BitmapImage bitmapImage;
        private IRandomAccessStream photoStream;
        private IBuildGoPiGoDevices deviceFactory;
        private IGoPiGo goPiGo;
        private ILed led1;
        private ILed led2;

        public MainPage()
        {
            InitializeComponent();

            InitializeGoPiGo();

            InitializeCamera();
        }

        private void InitializeGoPiGo()
        {
            deviceFactory = DeviceFactory.Build;

            goPiGo = deviceFactory.BuildGoPiGo();

            led1 = deviceFactory.BuildLed(Pin.LedLeft);
            led2 = deviceFactory.BuildLed(Pin.LedRight);

            // clean up in case something went bad
            goPiGo.MotorController().Stop();

            goPiGo.MotorController().SetRightMotorSpeed(_defaultSpeed);
            goPiGo.MotorController().SetLeftMotorSpeed(_defaultSpeed);

            BlinkAll(500, 3);
        }

        private void BlinkAll(int duration, int count)
        {
            for (int i = 0; i < count; i++)
            {
                led1.ChangeState(SensorStatus.On);
                led2.ChangeState(SensorStatus.On);
                Task.Delay(duration);
                led1.ChangeState(SensorStatus.Off);
                led2.ChangeState(SensorStatus.Off);
                Task.Delay(duration/2);

            }
        }

        private async void InitializeCamera()
        {
            try
            {
                status.Text = "Initializing camera to capture audio and video...";
                // Use default initialization
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                // Set callbacks for failure and recording limit exceeded
                status.Text = "Device successfully initialized for video recording!";
                mediaCapture.Failed += new MediaCaptureFailedEventHandler(mediaCapture_Failed);
                
                // Start Preview                
                ImageCapture.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                status.Text = "Camera preview succeeded";
            }
            catch (Exception ex)
            {
                status.Text = "Unable to initialize camera for audio/video mode: " + ex.Message;
            }
        }

        private async void mediaCapture_Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    status.Text = "MediaCaptureFailed: " + currentFailure.Message;
                }
                catch (Exception)
                {
                }
                finally
                {
                    status.Text += "\nCheck if camera is diconnected. Try re-launching the app";
                }
            });
        }

        private async void button_Clicked(object sender, RoutedEventArgs e)
        {

            detectionStatus.Text = "Detecting...";

            try
            {
                photoFile = await KnownFolders.PicturesLibrary.CreateFileAsync(
                    _photoFileName, CreationCollisionOption.GenerateUniqueName);
                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
                await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);
                status.Text = "Take Photo succeeded: " + photoFile.Path;

                photoStream = await photoFile.OpenReadAsync();
                bitmapImage = new BitmapImage();
                bitmapImage.SetSource(photoStream);
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }

            Emotion[] emotionResult = await UploadAndDetectEmotions();

            detectionStatus.Text = "Detection Done";

            displayParsedResults(emotionResult);
            displayAllResults(emotionResult);

            await actOnResults(emotionResult);
        }

        private async Task actOnResults(Emotion[] emotionResults)
        {
            int index = 0;
            foreach (Emotion emotion in emotionResults)
            {
                var emotionRank = emotion.Scores.ToRankedList();

                var currentEmotion = emotionRank.First().Key;

                // indicate which person it is responding to
                BlinkAll(500, index + 1);
                await Task.Delay(20);  // I think I have a timing issue here

                switch (currentEmotion)
                {
                    case "Anger":
                        goPiGo.MotorController().MoveBackward();
                        await Task.Delay(2000);
                        break;
                    case "Happiness":
                        goPiGo.MotorController().RotateLeft();
                        await Task.Delay(500);
                        goPiGo.MotorController().RotateRight();
                        await Task.Delay(1000);
                        goPiGo.MotorController().RotateLeft();
                        await Task.Delay(500);
                        break;
                    case "Sadness":
                        goPiGo.MotorController().SetLeftMotorSpeed(_slowSpeed);
                        await Task.Delay(20);
                        goPiGo.MotorController().SetRightMotorSpeed(_slowSpeed);
                        await Task.Delay(20);
                        goPiGo.MotorController().MoveForward();
                        await Task.Delay(2000);
                        goPiGo.MotorController().SetLeftMotorSpeed(_defaultSpeed);
                        await Task.Delay(20);
                        goPiGo.MotorController().SetRightMotorSpeed(_defaultSpeed);
                        await Task.Delay(20);
                        break;
                    default:
                        break;
                }

                goPiGo.MotorController().Stop();
                await Task.Delay(20);

                index++;
            }
        }

        private void displayAllResults(Emotion[] resultList)
        {
            int index = 0;
            foreach (Emotion emotion in resultList)
            {
                ResultBox.Items.Add("\nFace #" + index
                + "\nAnger: " + emotion.Scores.Anger
                + "\nContempt: " + emotion.Scores.Contempt
                + "\nDisgust: " + emotion.Scores.Disgust
                + "\nFear: " + emotion.Scores.Fear
                + "\nHappiness: " + emotion.Scores.Happiness
                + "\nNeutral: " + emotion.Scores.Neutral
                + "\nSadness: " + emotion.Scores.Sadness
                + "\nSurprise: " + emotion.Scores.Surprise);

                index++;
            }
        }

        private async void displayParsedResults(Emotion[] resultList)
        {
            int index = 0;
            string textToDisplay = "";
            foreach (Emotion emotion in resultList)
            {
                string emotionString = parseResults(emotion);
                textToDisplay += "Person number " + index.ToString() + " " + emotionString + "\n";
                index++;
            }
            ResultBox.Items.Add(textToDisplay);
        }

        private string parseResults(Emotion emotion)
        {
            float topScore = 0.0f;
            string topEmotion = "";
            string retString = "";
            //anger
            topScore = emotion.Scores.Anger;
            topEmotion = "Anger";
            // contempt
            if (topScore < emotion.Scores.Contempt)
            {
                topScore = emotion.Scores.Contempt;
                topEmotion = "Contempt";
            }
            // disgust
            if (topScore < emotion.Scores.Disgust)
            {
                topScore = emotion.Scores.Disgust;
                topEmotion = "Disgust";
            }
            // fear
            if (topScore < emotion.Scores.Fear)
            {
                topScore = emotion.Scores.Fear;
                topEmotion = "Fear";
            }
            // happiness
            if (topScore < emotion.Scores.Happiness)
            {
                topScore = emotion.Scores.Happiness;
                topEmotion = "Happiness";
            }
            // Neutral
            if (topScore < emotion.Scores.Neutral)
            {
                topScore = emotion.Scores.Neutral;
                topEmotion = "Neutral";
            }
            // sadness
            if (topScore < emotion.Scores.Sadness)
            {
                topScore = emotion.Scores.Sadness;
                topEmotion = "Sadness";
            }
            // surprise
            if (topScore < emotion.Scores.Surprise)
            {
                topScore = emotion.Scores.Surprise;
                topEmotion = "Surprise";
            }

            retString = "is expressing " + topEmotion + "\n with " + topScore.ToString() + " certainty.";
            return retString;
        }

        private async Task<Emotion[]> UploadAndDetectEmotions()
        {
            Debug.WriteLine("EmotionServiceClient is created");

            //
            // Create Project Oxford Emotion API Service client
            //
            EmotionServiceClient emotionServiceClient = new EmotionServiceClient(_subscriptionKey);

            Debug.WriteLine("Calling EmotionServiceClient.RecognizeAsync()...");
            try
            {
                //var stream = await File.OpenRead(photoFile.Path);

                //
                // Detect the emotions in the URL
                //

                // make sure the stream is at the start
                photoStream.Seek(0);

                Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(photoStream.AsStreamForRead());
                return emotionResult;
            }
            catch (ClientException ex)
            {
                Debug.WriteLine($"Detection failed. ClientExpection thrown {ex.Message}");
                Debug.WriteLine(ex.ToString());
                return null;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Detection failed. Please make sure that you have the right subscription key and proper URL to detect.");
                Debug.WriteLine(exception.ToString());
                return null;
            }
        }
    }
}
