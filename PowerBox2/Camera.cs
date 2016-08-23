using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.Threading.Tasks;

namespace PowerBox
{
    class Camera
    {
        private MediaCapture mediaCapture;
        private StorageFile photoFile;
        private StorageFile recordStorageFile;
        private StorageFile audioFile;
        private readonly string PHOTO_FILE_NAME = "photo.jpg";
        private readonly string VIDEO_FILE_NAME = "video.mp4";
        private readonly string AUDIO_FILE_NAME = "audio.mp3";
        private bool isRecording;
        private DispatcherTimer timer;

        private string status;

        public Camera()
        {
            isRecording = false;
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
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

                status = "Initializing camera to capture audio and video...";
                // Use default initialization
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                // Set callbacks for failure and recording limit exceeded
                status = "Device successfully initialized for video recording!";
                mediaCapture.Failed += new MediaCaptureFailedEventHandler(mediaCapture_Failed);
                mediaCapture.RecordLimitationExceeded += new RecordLimitationExceededEventHandler(mediaCapture_RecordLimitExceeded);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to initialize camera for audio/video mode: " + ex.Message);
            }
        }

        /// 'Initialize Audio Only' button action function
        /// Dispose existing MediaCapture object and set it up for audio only
        /// Enable or disable appropriate buttons
        /// - DISABLE 'Initialize Audio Only' 
        /// - DISABLE 'Start Video Record'
        /// - DISABLE 'Take Photo'
        /// - ENABLE 'Initialize Audio and Video'
        /// - ENABLE 'Start Audio Record'        
        public async void initAudioOnly()
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

                status = "Initializing camera to capture audio only...";
                mediaCapture = new MediaCapture();
                var settings = new MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = StreamingCaptureMode.Audio;
                settings.MediaCategory = MediaCategory.Other;
                settings.AudioProcessing = Windows.Media.AudioProcessing.Default;
                await mediaCapture.InitializeAsync(settings);

                // Set callbacks for failure and recording limit exceeded
                status = "Device successfully initialized for audio recording!" + "\nPress \'Start Audio Record\' to record";
                mediaCapture.Failed += new MediaCaptureFailedEventHandler(mediaCapture_Failed);
                mediaCapture.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(mediaCapture_RecordLimitExceeded);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to initialize camera for audio mode: " + ex.Message);
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
            }
            catch (Exception ex)
            {
                Cleanup();
                throw new Exception(ex.Message);
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
                    status = "Initialize video recording";
                    String fileName;
                    fileName = VIDEO_FILE_NAME;

                    recordStorageFile = await KnownFolders.VideosLibrary.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

                    status = "Video storage file preparation successful";

                    MediaEncodingProfile recordProfile = null;
                    recordProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

                    await mediaCapture.StartRecordToStorageFileAsync(recordProfile, recordStorageFile);

                    isRecording = true;
                    status = "Video recording in progress... press \'Stop Video Record\' to stop";
                }
                else
                {
                    status = "Stopping video recording...";
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;
                }
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException)
                {
                    throw new Exception("Unable to play recorded video; video recorded successfully to: " + recordStorageFile.Path);
                }
                else
                {
                    Cleanup();
                    throw new Exception(ex.Message);
                }
            }
        }

        // 'Start Audio Record' button click action function
        // Button name is changes to 'Stop Audio Record' once recording is started
        // Records audio to a file in the default account video folder
        public async void recordAudio()
        {
            try
            {
                if (!isRecording)
                {
                    audioFile = await KnownFolders.VideosLibrary.CreateFileAsync(AUDIO_FILE_NAME, CreationCollisionOption.GenerateUniqueName);

                    status = "Audio storage file preparation successful";

                    MediaEncodingProfile recordProfile = null;
                    recordProfile = MediaEncodingProfile.CreateM4a(AudioEncodingQuality.Auto);

                    await mediaCapture.StartRecordToStorageFileAsync(recordProfile, audioFile);

                    isRecording = true;
                    status = "Audio recording in progress... press \'Stop Audio Record\' to stop";
                }
                else
                {
                    status = "Stopping audio recording...";

                    await mediaCapture.StopRecordAsync();

                    isRecording = false;
                }
            }
            catch (Exception ex)
            {
                Cleanup();
                throw new Exception(ex.Message);
            }
        }

        // Callback function for any failures in MediaCapture operations
        private async void mediaCapture_Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            await Task.Run(async () => {
                try
                {
                    status = "MediaCaptureFailed: " + currentFailure.Message;

                    if (isRecording)
                    {
                        await mediaCapture.StopRecordAsync();
                        status += "\n Recording Stopped";
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    status += "\nCheck if camera is diconnected. Try re-launching the app";
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
                            status = "Stopping Record on exceeding max record duration";
                            await mediaCapture.StopRecordAsync();
                            isRecording = false;
                            if (mediaCapture.MediaCaptureSettings.StreamingCaptureMode == StreamingCaptureMode.Audio)
                            {
                                status = "Stopped record on exceeding max record duration: " + audioFile.Path;
                            }
                            else
                            {
                                status = "Stopped record on exceeding max record duration: " + recordStorageFile.Path;
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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
