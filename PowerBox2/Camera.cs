using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace PowerBox2
{
    class Camera
    {
        private MediaCapture mediaCapture;
        private StorageFile photoFile;
        private StorageFile recordStorageFile;
        private BitmapImage bitmap;
        private IRandomAccessStreamWithContentType stream;
        private readonly string PHOTO_FILE_NAME = "photo.jpg";
        private readonly string VIDEO_FILE_NAME = "video.mp4";
        private readonly string AUDIO_FILE_NAME = "audio.mp3";
        private bool isRecording;
        private bool isPreviewing;
        private DispatcherTimer timer;
        private Debag debag;

        private Action<string> delegatePrint;
        private Action delegateFailed;
        private Action delegateRecordLimitExceeded;

        private string status;

        private Camera()
        {
            isRecording = false;
            isPreviewing = false;
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
        }

        public Camera(Debag debag) : this()
        {
            this.debag = debag;
        }

        public Camera(Debag debag,
            Action<string> delegatePrint, 
            Action delegateFailed, 
            Action delegateRecordLimitExceeded) : this()
        {
            this.debag = debag;
            this.delegatePrint = delegatePrint;
            this.delegateFailed = delegateFailed;
            this.delegateRecordLimitExceeded = delegateRecordLimitExceeded;
        }

        public async void Cleanup()
        {
            if (mediaCapture != null)
            {
                // Cleanup MediaCapture object
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                    isPreviewing = false;
                }
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
            if (delegatePrint == null)
            {
                debag.WriteSD_Debag(status);
            }
            else
            {
                delegatePrint(status);
            }
        }

        public bool getIsRecording()
        {
            return isRecording;
        }

        public StorageFile getRecordStorageFile()
        {
            return recordStorageFile;
        }

        public BitmapImage getBitmap()
        {
            return bitmap;
        }

        public bool getIsPreviewing()
        {
            return isPreviewing;
        }

        public MediaCapture getMediaCapture()
        {
            return mediaCapture;
        }

        public IRandomAccessStreamWithContentType getStream()
        {
            return stream;
        }

         //'Initialize Audio and Video' button action function
         //Dispose existing MediaCapture object and set it up for audio and video
         //Enable or disable appropriate buttons
         //- DISABLE 'Initialize Audio and Video' 
         //- DISABLE 'Start Audio Record'
         //- ENABLE 'Initialize Audio Only'
         //- ENABLE 'Start Video Record'
         //- ENABLE 'Take Photo'
        public async void initVideo(CaptureElement previewElement = null)
        {
            try
            {
                if (mediaCapture != null)
                {
                    // Cleanup MediaCapture object
                    if (isPreviewing)
                    {
                        await mediaCapture.StopPreviewAsync();
                        isPreviewing = false;
                    }
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
                mediaCapture.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(mediaCapture_RecordLimitExceeded);

                if (previewElement == null)
                {
                    previewElement.Source = mediaCapture;
                    await mediaCapture.StartPreviewAsync();
                }

                isPreviewing = true;
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

                IRandomAccessStream photoStream = await photoFile.OpenReadAsync();
                bitmap = new BitmapImage();
                bitmap.SetSource(photoStream);

                printStatus("Take Photo succeeded: " + photoFile.Path);
            }
            catch (Exception ex)
            {
                printStatus(ex.Message);
                Cleanup();
                bitmap = null;
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
                    String fileName;
                    fileName = VIDEO_FILE_NAME;

                    recordStorageFile = await debag.getFolderWatch().CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                    printStatus("Video storage file preparation successful");

                    MediaEncodingProfile recordProfile = null;
                    recordProfile = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);

                    await mediaCapture.StartRecordToStorageFileAsync(recordProfile, recordStorageFile);
                    isRecording = true;
                    printStatus("Video recording in progress... press \'Stop Video Record\' to stop");
                }
                else
                {
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;

                    stream = await recordStorageFile.OpenReadAsync();
                    printStatus("Stopping video recording...");
                }
            }
            catch (Exception ex)
            {
                if (!(ex is System.UnauthorizedAccessException))
                {
                    Cleanup();
                }
                throw new Exception(ex.Message, ex);
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
                    delegateFailed();
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
                            delegateRecordLimitExceeded();
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
