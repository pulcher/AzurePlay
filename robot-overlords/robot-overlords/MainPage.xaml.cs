using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace robot_overlords
{
    public sealed partial class MainPage : Page
    {
        private string _subscriptionKey = " 85ce8f79af0248a6910259be8eab3931";
        private readonly string PHOTO_FILE_NAME = "roboPhoto.jpg";

        private MediaCapture mediaCapture;
        private StorageFile photoFile;
        private BitmapImage bitmapImage;
        private IRandomAccessStream photoStream;

        public MainPage()
        {
            InitializeComponent();

            InitializeCamera();
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
                    PHOTO_FILE_NAME, CreationCollisionOption.GenerateUniqueName);
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
            // surprise
            if (topScore < emotion.Scores.Surprise)
            {
                topScore = emotion.Scores.Surprise;
                topEmotion = "Surprise";
            }

            retString = "is expressing " + topEmotion + " with " + topScore.ToString() + " certainty.";
            return retString;
        }


        public async void DrawFaceRectangle(Emotion[] emotionResult, BitmapImage bitMapImage, String urlString)
        {


            if (emotionResult != null && emotionResult.Length > 0)
            {
                IRandomAccessStream stream = await RandomAccessStreamReference.CreateFromUri(new Uri(urlString)).OpenReadAsync();


                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);


                //double resizeFactorH = ImageCanvas.Height / decoder.PixelHeight;
                //double resizeFactorW = ImageCanvas.Width / decoder.PixelWidth;


                foreach (var emotion in emotionResult)
                {

                    Microsoft.ProjectOxford.Common.Rectangle faceRect = emotion.FaceRectangle;

                    Image Img = new Image();
                    BitmapImage BitImg = new BitmapImage();
                    // open the rectangle image, this will be our face box
                    IRandomAccessStream box = await RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/rectangle.png", UriKind.Absolute)).OpenReadAsync();

                    BitImg.SetSource(box);

                    //rescale each facebox based on the API's face rectangle
                    //var maxWidth = faceRect.Width * resizeFactorW;
                    //var maxHeight = faceRect.Height * resizeFactorH;

                    var origHeight = BitImg.PixelHeight;
                    var origWidth = BitImg.PixelWidth;


                    //var ratioX = maxWidth / (float)origWidth;
                    //var ratioY = maxHeight / (float)origHeight;
                    //var ratio = Math.Min(ratioX, ratioY);
                    //var newHeight = (int)(origHeight * ratio);
                    //var newWidth = (int)(origWidth * ratio);

                    //BitImg.DecodePixelWidth = newWidth;
                    //BitImg.DecodePixelHeight = newHeight;

                    // set the starting x and y coordiantes for each face box
                    Thickness margin = Img.Margin;

                    //margin.Left = faceRect.Left * resizeFactorW;
                    //margin.Top = faceRect.Top * resizeFactorH;

                    Img.Margin = margin;

                    Img.Source = BitImg;
                    //ImageCanvas.Children.Add(Img);
                }
            }
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
