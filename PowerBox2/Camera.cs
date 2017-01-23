using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.Threading.Tasks;

namespace PowerBox2
{
    class Camera
    {
        private MediaCapture mediaCapture;
        private StorageFile photoFile;
        private StorageFile recordStorageFile;
        private readonly string PHOTO_FILE_NAME = "photo.jpg";
        private readonly string VIDEO_FILE_NAME = "video.mp4";
        private bool isRecording;
        private DispatcherTimer timer;
        private Debag debag;

        public Camera(Debag debag)
        {
            isRecording = false;
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            this.debag = debag;
        }

        public async void Cleanup()
        {
            if (mediaCapture != null)
            {
                if (isRecording)
                {
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;
                }
                mediaCapture.Dispose();
                mediaCapture = null;
            }
        }

        private void printStatus(string status)
        {
            debag.WriteSD_Debag(status);
        }

        //'Initialize Audio and Video' button action function
        //Dispose existing MediaCapture object and set it up for audio and video
        //Enable or disable appropriate buttons
        //- DISABLE 'Initialize Audio and Video' 
        //- DISABLE 'Start Audio Record'
        //- ENABLE 'Initialize Audio Only'
        //- ENABLE 'Start Video Record'
        //- ENABLE 'Take Photo'
        public async void initVideo()
        {
            try
            {
                if (mediaCapture != null)
                {
                    if (isRecording)
                    {
                        await mediaCapture.StopRecordAsync();
                        isRecording = false;
                    }
                    mediaCapture.Dispose();
                    mediaCapture = null;
                }

                printStatus("Initializing camera to capture audio and video...");
                // Use default initialization
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                // Set callbacks for failure and recording limit exceeded
                printStatus("Device successfully initialized for video recording!");
                mediaCapture.Failed += new MediaCaptureFailedEventHandler(mediaCapture_Failed);
                mediaCapture.RecordLimitationExceeded += new RecordLimitationExceededEventHandler(mediaCapture_RecordLimitExceeded);
                printStatus("Camera preview succeeded");
            }
            catch (Exception ex)
            {
                printStatus("Unable to initialize camera for audio/video mode: " + ex.Message);
            }
        }

        // 'Take Photo' button click action function
        // Capture image to a file in the default account photos folder
        public async void takePhoto()
        {
            try
            {
                photoFile = await KnownFolders.PicturesLibrary.CreateFileAsync(
                    PHOTO_FILE_NAME, CreationCollisionOption.GenerateUniqueName);
                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
                await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);
                printStatus("Take Photo succeeded: " + photoFile.Path);
            }
            catch (Exception ex)
            {
                printStatus(ex.Message);
                Cleanup();
            }
        }

        // 'Start Video Record' button click action function
        // Button name is changed to 'Stop Video Record' once recording is started
        // Records video to a file in the default account videos folder
        public async void recordVideo()
        {
            try
            {
                if (!isRecording)
                {
                    printStatus("Initialize video recording");
                    String fileName;
                    fileName = VIDEO_FILE_NAME;

                    recordStorageFile = await debag.getFolderWatch().CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                    printStatus("Video storage file preparation successful");

                    MediaEncodingProfile recordProfile = null;
                    recordProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

                    await mediaCapture.StartRecordToStorageFileAsync(recordProfile, recordStorageFile);

                    isRecording = true;
                    printStatus("Video recording in progress... press \'Stop Video Record\' to stop");
                }
                else
                {
                    printStatus("Stopping video recording...");
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;
                }
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException)
                {
                    printStatus("Unable to play recorded video; video recorded successfully to: " + recordStorageFile.Path);
                }
                else
                {
                    printStatus(ex.Message);
                    Cleanup();
                }
            }
        }

        // Callback function for any failures in MediaCapture operations
        private async void mediaCapture_Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            await Task.Run(async () => {
                try
                {
                    printStatus("MediaCaptureFailed: " + currentFailure.Message);

                    if (isRecording)
                    {
                        await mediaCapture.StopRecordAsync();
                        printStatus("Recording Stopped");
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    printStatus("Check if camera is diconnected. Try re-launching the app");
                }
            });
        }

        // Callback function if Recording Limit Exceeded
        private async void mediaCapture_RecordLimitExceeded(MediaCapture currentCaptureObject)
        {
            try
            {
                if (isRecording)
                {
                    await Task.Run(async() => {
                        try
                        {
                            printStatus("Stopping Record on exceeding max record duration");
                            await mediaCapture.StopRecordAsync();
                            isRecording = false;
                            printStatus("Stopped record on exceeding max record duration: " + recordStorageFile.Path);
                        }
                        catch (Exception e)
                        {
                            printStatus(e.Message);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                printStatus(e.Message);
            }
        }

        public void takePhotoFrequency(int frequency)
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
            else
            {
                timer.Interval = TimeSpan.FromMilliseconds(frequency * 1000);
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            takePhoto();
        }

    }
}
