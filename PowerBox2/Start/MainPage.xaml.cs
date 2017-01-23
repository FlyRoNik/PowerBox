using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PowerBox2
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Box box;
        private Camera_FaceDetect camFaceDet;

        #region Constructor, lifecycle and navigation
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled; //сохранение состояния страницы

            //ObservableCollection<FontFamily> Address = new ObservableCollection<FontFamily>();
            //for (int i = 0; i < box.mcu.Length; i++)
            //{
            //    Address.Add(new FontFamily(box.mcu[i].getI2C_Slave_Address().ToString()));
            //}
            //comboBox.DataContext = Address;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Box)
            {
                box = (Box)e.Parameter;
            }
            init();
                        
            base.OnNavigatedTo(e);
        }

        private void init()
        {
            if (box.scaner.getRepeatMode() == FingerPrintScaner.Value.ON)
            {
                allowSamePrints.IsEnabled = false;
            }
            else
            {
                prohibitSamePrints.IsEnabled = false;
            }

            privilege.Items.Add(FingerPrintScaner.Privilege.USER);
            privilege.Items.Add(FingerPrintScaner.Privilege.ADMIN);
            privilege.Items.Add(FingerPrintScaner.Privilege.VIP);

            SetInitButtonVisibility(Action.ENABLE);
            SetVideoButtonVisibility(Action.DISABLE);

            isRecording = false;
            isPreviewing = false;
        }

        #endregion Constructor, lifecycle and navigation

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //проверка закрытости ячеек
            for (int i = 0; i < /*box.mcu.Length*/2; i++)
            {
                if (box.mcu[i].getDataMicrocontroller()[1] == 0)
                {
                    int n = i + 1;
                    textBlock.Text = "Ячейка №" + n + " не закрыта! Закройте пожалуйста.";
                    return;
                }
            }

            this.Frame.Navigate(typeof(Put.Welcome), box);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                box = new Box();
            }
            catch (Exception ex)
            {
                textBlock.Text = ex.Message;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            camFaceDet = new Camera_FaceDetect(delegteFaceDetect, box.debag);

            camFaceDet.Application_Resuming();
        }

        private async void button4_Click(object sender, RoutedEventArgs e)
        {
            await camFaceDet.TakePhotoAsync();
        }

        private async void button3_Click(object sender, RoutedEventArgs e)
        {
            if (!camFaceDet._isRecording)
            {
                await camFaceDet.StartRecordingAsync();
            }
            else
            {
                await camFaceDet.StopRecordingAsync();
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            camFaceDet.CleanupUi();
        }

        private async void button6_Click(object sender, RoutedEventArgs e)
        {
            if (camFaceDet._faceDetectionEffect == null || !camFaceDet._faceDetectionEffect.Enabled)
            {
                await camFaceDet.CreateFaceDetectionEffectAsync();
            }
            else
            {
                camFaceDet.CleanUpFaceDetectionEffectAsync();
            }
        }

        public async void delegteFaceDetect(int count)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                textBlock.Text = count.ToString();
            });
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            box.debag.dellFolderSD(Debag.ValueFolder.FolderDebags);
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Start.AreYouStillHere), box);
        }

        #region MCU
        private void resetScan_Click(object sender, RoutedEventArgs e)
        {
            if (box.scaner.getReset() == FingerPrintScaner.Value.ON)
            {
                box.scaner.setReset(FingerPrintScaner.Value.OFF);
            }
            else
            {
                box.scaner.setReset(FingerPrintScaner.Value.ON);
            }
        }

        private void blinkScan_Click(object sender, RoutedEventArgs e)
        {
            if (box.scaner.getBlink() == FingerPrintScaner.Value.ON)
            {
                box.scaner.setBlink(FingerPrintScaner.Value.OFF);
            }
            else
            {
                box.scaner.setBlink(FingerPrintScaner.Value.ON);
            }
        }

        private void waitScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.sleep();
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void allowSamePrints_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.setRepeatMode(FingerPrintScaner.Value.ON);
                prohibitSamePrints.IsEnabled = true;
                allowSamePrints.IsEnabled = false;
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void prohibitSamePrints_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.setRepeatMode(FingerPrintScaner.Value.OFF);
                prohibitSamePrints.IsEnabled = false;
                allowSamePrints.IsEnabled = true;
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void addPerson_Click(object sender, RoutedEventArgs e)
        {
            box.privilege = (FingerPrintScaner.Privilege)privilege.SelectedItem;
            box.numberCell = Int32.Parse(textBox.Text);
            this.Frame.Navigate(typeof(Put.Scanning), box);
        }

        private void dellPerson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.deleteUser(Int32.Parse(textBox.Text));
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void dellAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.deleteAllUser();
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void compareOneToOne_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.compareOneToOne(Int32.Parse(textBox.Text));
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void getPrivilagePerson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.getUserPrivilege(Int32.Parse(textBox.Text)).ToString();
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void setComparisonLevel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.setComparisonLevel(Int32.Parse(textBox.Text));
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void setTimeOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.setTimeoutValue(Int32.Parse(textBox.Text));
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void getComparisonLevel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.getComparisonLevel().ToString();
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void getPrivilageAndIdAllPerson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FingerPrintScaner.Person[] users = box.scaner.getUserNumbersAndPrivilege();
                message.Text = "";
                for (int i = 0; i < users.Length; i++)
                {
                    message.Text += users[i].ToString() + "| ";
                }
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void compareOwnToMore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FingerPrintScaner.Person person = box.scaner.compareOneToMore();
                message.Text = person.getID().ToString() + ": " + person.getPrivilege().ToString();
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void getTimeOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.getTimeoutValue().ToString();
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }

        private void getNumberPersons_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                message.Text = box.scaner.getNumberOfUsers().ToString();
            }
            catch (Exception ex)
            {
                message.Text = ex.Message;
            }
        }
        #endregion MCU

        #region web camera and face detection.private MediaCapture mediaCapture;
        private MediaCapture mediaCapture;
        private StorageFile photoFile;
        private StorageFile recordStorageFile;
        private readonly string PHOTO_FILE_NAME = "photo.jpg";
        private readonly string VIDEO_FILE_NAME = "video.mp4";
        private bool isPreviewing;
        private bool isRecording;

        // Receive notifications about rotation of the device and UI and apply any necessary rotation to the preview stream and UI controls
        private readonly DisplayInformation _displayInformation = DisplayInformation.GetForCurrentView();

        // Receive notifications about rotation of the device and UI and apply 
        //any necessary rotation to the preview stream and UI controls
        private DisplayOrientations _displayOrientation = DisplayOrientations.Portrait;

        // Rotation metadata to apply to the preview stream and recorded videos (MF_MT_VIDEO_ROTATION)
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        // MediaCapture and its state variables
        private IMediaEncodingProperties previewProperties;

        // Information about the camera device
        private bool _mirroringPreview;
        private bool _externalCamera;

        private FaceDetectionEffect _faceDetectionEffect;

        #region HELPER_FUNCTIONS
        enum Action
        {
            ENABLE,
            DISABLE
        }
        /// <summary>
        /// Helper function to enable or disable Initialization buttons
        /// </summary>
        /// <param name="action">enum Action</param>
        private void SetInitButtonVisibility(Action action)
        {
            if (action == Action.ENABLE)
            {
                video_init.IsEnabled = true;
                cleanup.IsEnabled = false;
            }
            else
            {
                video_init.IsEnabled = false;
                cleanup.IsEnabled = true;
            }
        }

        /// <summary>
        /// Helper function to enable or disable video related buttons (TakePhoto, Start Video Record)
        /// </summary>
        /// <param name="action">enum Action</param>
        private void SetVideoButtonVisibility(Action action)
        {
            if (action == Action.ENABLE)
            {
                takePhoto.IsEnabled = true;
                takePhoto.Visibility = Visibility.Visible;

                recordVideo.IsEnabled = true;
                recordVideo.Visibility = Visibility.Visible;

                faceDetect.IsEnabled = true;;
            }
            else
            {
                takePhoto.IsEnabled = false;
                takePhoto.Visibility = Visibility.Collapsed;

                recordVideo.IsEnabled = false;
                recordVideo.Visibility = Visibility.Collapsed;

                faceDetect.IsEnabled = false;
            }
        }
        #endregion
        private async void Cleanup()
        {
            if (mediaCapture != null)
            {
                // Cleanup MediaCapture object
                if (_faceDetectionEffect != null)
                {
                    await CleanUpFaceDetectionEffectAsync();
                }

                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                    captureImage.Source = null;
                    playbackElement.Source = null;
                    isPreviewing = false;
                }
                if (isRecording)
                {
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;
                }


                if (mediaCapture != null)
                {
                    mediaCapture.Dispose();
                    mediaCapture = null;
                }
            }
            SetInitButtonVisibility(Action.ENABLE);
        }

        /// <summary>
        /// 'Initialize Audio and Video' button action function
        /// Dispose existing MediaCapture object and set it up for audio and video
        /// Enable or disable appropriate buttons
        /// - DISABLE 'Initialize Audio and Video' 
        /// - DISABLE 'Start Audio Record'
        /// - ENABLE 'Initialize Audio Only'
        /// - ENABLE 'Start Video Record'
        /// - ENABLE 'Take Photo'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void initVideo_Click(object sender, RoutedEventArgs e)
        {
            // Disable all buttons until initialization completes

            SetInitButtonVisibility(Action.DISABLE);
            SetVideoButtonVisibility(Action.DISABLE);

            try
            {
                if (mediaCapture != null)
                {
                    // Cleanup MediaCapture object
                    if (isPreviewing)
                    {
                        await mediaCapture.StopPreviewAsync();
                        captureImage.Source = null;
                        playbackElement.Source = null;
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

                status.Text = "Initializing camera to capture audio and video...";

                // Attempt to get the front camera if one is available, but use any camera device if not
                var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Front);

                if (cameraDevice == null)
                {
                    status.Text = "No camera device found!";
                    return;
                }

                // Use default initialization
                mediaCapture = new MediaCapture();

                // Set callbacks for failure and recording limit exceeded
                mediaCapture.Failed += new MediaCaptureFailedEventHandler(mediaCapture_Failed);
                mediaCapture.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(mediaCapture_RecordLimitExceeded);

                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                await mediaCapture.InitializeAsync(settings);

                status.Text = "Device successfully initialized for video recording!";

                // Figure out where the camera is located
                if (cameraDevice.EnclosureLocation == null || cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Unknown)
                {
                    // No information on the location of the camera, assume it's an external camera, not integrated on the device
                    _externalCamera = true;
                }
                else
                {
                    // Camera is fixed on the device
                    _externalCamera = false;

                    // Only mirror the preview if the camera is on the front panel
                    _mirroringPreview = (cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
                }

                previewElement.Source = mediaCapture;
                previewElement.FlowDirection = _mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

                // Start Preview                
                await mediaCapture.StartPreviewAsync();
                previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);

                // Initialize the preview to the current orientation
                if (previewProperties != null)
                {
                    _displayOrientation = _displayInformation.CurrentOrientation;

                    await SetPreviewRotationAsync();
                }

                isPreviewing = true;
                status.Text = "Camera preview succeeded";

                // Enable buttons for video and photo capture
                SetVideoButtonVisibility(Action.ENABLE);

                UpdateCaptureControls();
            }
            catch (Exception ex)
            {
                status.Text = "Unable to initialize camera for audio/video mode: " + ex.Message;
            }
        }

        /// <summary>
        /// Gets the current orientation of the UI in relation to the device (when AutoRotationPreferences cannot be honored) and applies a corrective rotation to the preview
        /// </summary>
        private async Task SetPreviewRotationAsync()
        {
            // Only need to update the orientation if the camera is mounted on the device
            if (_externalCamera) return;

            // Calculate which way and how far to rotate the preview
            int rotationDegrees = ConvertDisplayOrientationToDegrees(_displayOrientation);

            // The rotation direction needs to be inverted if the preview is being mirrored
            if (_mirroringPreview)
            {
                rotationDegrees = (360 - rotationDegrees) % 360;
            }

            // Add rotation metadata to the preview stream to make sure the aspect ratio / dimensions match when rendering and getting preview frames
            var props = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, rotationDegrees);
            await mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
        }

        /// <summary>
        /// Attempts to find and return a device mounted on the panel specified, and on failure to find one it will return the first device listed
        /// </summary>
        /// <param name="desiredPanel">The desired panel on which the returned device should be mounted, if available</param>
        /// <returns></returns>
        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        private void cleanup_Click(object sender, RoutedEventArgs e)
        {
            SetInitButtonVisibility(Action.DISABLE);
            Cleanup();
            UpdateCaptureControls();
            SetVideoButtonVisibility(Action.DISABLE);
        }

        /// <summary>
        /// 'Take Photo' button click action function
        /// Capture image to a file in the default account photos folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void takePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                takePhoto.IsEnabled = false;
                recordVideo.IsEnabled = false;
                captureImage.Source = null;

                photoFile = await box.debag.getFolderWatch().CreateFileAsync(PHOTO_FILE_NAME, 
                    Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
                await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);
                status.Text = "Take Photo succeeded: " + photoFile.Path;

                IRandomAccessStream photoStream = await photoFile.OpenReadAsync();
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(photoStream);
                captureImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
                Cleanup();
            }
            finally
            {
                UpdateCaptureControls();
                takePhoto.IsEnabled = true;
                recordVideo.IsEnabled = true;
            }
        }

        /// <summary>
        /// 'Start Video Record' button click action function
        /// Button name is changed to 'Stop Video Record' once recording is started
        /// Records video to a file in the default account videos folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void recordVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                takePhoto.IsEnabled = false;
                recordVideo.IsEnabled = false;
                playbackElement.Source = null;

                if (!isRecording)
                {
                    takePhoto.IsEnabled = false;
                    status.Text = "Initialize video recording";

                    recordStorageFile = await box.debag.getFolderWatch().CreateFileAsync(VIDEO_FILE_NAME, 
                        Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                    status.Text = "Video storage file preparation successful";

                    MediaEncodingProfile recordProfile = null;
                    recordProfile = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);

                    await mediaCapture.StartRecordToStorageFileAsync(recordProfile, recordStorageFile);
                    recordVideo.IsEnabled = true;
                    isRecording = true;
                    status.Text = "Video recording in progress... press \'Stop Video Record\' to stop";
                }
                else
                {
                    takePhoto.IsEnabled = true;
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;

                    var stream = await recordStorageFile.OpenReadAsync();
                    playbackElement.AutoPlay = true;
                    playbackElement.SetSource(stream, recordStorageFile.FileType);
                    playbackElement.Play();
                    status.Text = "Playing recorded video" + recordStorageFile.Path;
                }
            }
            catch (Exception ex)
            {
                if (ex is System.UnauthorizedAccessException)
                {
                    status.Text = "Unable to play recorded video; video recorded successfully to: " + recordStorageFile.Path;
                }
                else
                {
                    status.Text = ex.Message;
                    Cleanup();
                }
            }
            finally
            {
                UpdateCaptureControls();
                recordVideo.IsEnabled = true;
            }
        }

        /// <summary>
        /// Callback function for any failures in MediaCapture operations
        /// </summary>
        /// <param name="currentCaptureObject"></param>
        /// <param name="currentFailure"></param>
        private async void mediaCapture_Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    status.Text = "MediaCaptureFailed: " + currentFailure.Message;

                    if (isRecording)
                    {
                        await mediaCapture.StopRecordAsync();
                        status.Text += "\n Recording Stopped";
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    SetInitButtonVisibility(Action.DISABLE);
                    SetVideoButtonVisibility(Action.DISABLE);
                    status.Text += "\nCheck if camera is diconnected. Try re-launching the app";
                }
            });
        }

        /// <summary>
        /// Callback function if Recording Limit Exceeded
        /// </summary>
        /// <param name="currentCaptureObject"></param>
        public async void mediaCapture_RecordLimitExceeded(Windows.Media.Capture.MediaCapture currentCaptureObject)
        {
            try
            {
                if (isRecording)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            status.Text = "Stopping Record on exceeding max record duration";
                            await mediaCapture.StopRecordAsync();
                            isRecording = false;
                            status.Text = "Stopped record on exceeding max record duration: " + recordStorageFile.Path;
                        }
                        catch (Exception e)
                        {
                            status.Text = e.Message;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                status.Text = e.Message;
            }
        }

        #region Face detection helpers

        private async void faceDetect_Click(object sender, RoutedEventArgs e)
        {
            if (_faceDetectionEffect == null || !_faceDetectionEffect.Enabled)
            {
                // Clear any rectangles that may have been left over from a previous instance of the effect
                FacesCanvas.Children.Clear();

                await CreateFaceDetectionEffectAsync();
            }
            else
            {
                await CleanUpFaceDetectionEffectAsync();
            }

            UpdateCaptureControls();
        }

        /// <summary>
        /// Adds face detection to the preview stream, registers for its events, enables it, and gets the FaceDetectionEffect instance
        /// </summary>
        /// <returns></returns>
        private async Task CreateFaceDetectionEffectAsync()
        {
            // Create the definition, which will contain some initialization settings
            var definition = new FaceDetectionEffectDefinition();

            // To ensure preview smoothness, do not delay incoming samples
            definition.SynchronousDetectionEnabled = false;

            // In this scenario, choose detection speed over accuracy
            definition.DetectionMode = FaceDetectionMode.HighPerformance;

            // Add the effect to the preview stream
            _faceDetectionEffect = (FaceDetectionEffect)await mediaCapture.AddVideoEffectAsync(definition, MediaStreamType.VideoPreview);

            // Register for face detection events
            _faceDetectionEffect.FaceDetected += FaceDetectionEffect_FaceDetected;

            // Choose the shortest interval between detection events
            _faceDetectionEffect.DesiredDetectionInterval = TimeSpan.FromMilliseconds(33);

            // Start detecting faces
            _faceDetectionEffect.Enabled = true;
        }

        private async void FaceDetectionEffect_FaceDetected(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
            // Ask the UI thread to render the face bounding boxes
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HighlightDetectedFaces(args.ResultFrame.DetectedFaces));
        }

        /// <summary>
        ///  Disables and removes the face detection effect, and unregisters the event handler for face detection
        /// </summary>
        /// <returns></returns>
        private async Task CleanUpFaceDetectionEffectAsync()
        {
            // Disable detection
            _faceDetectionEffect.Enabled = false;

            // Unregister the event handler
            _faceDetectionEffect.FaceDetected -= FaceDetectionEffect_FaceDetected;

            // Remove the effect (see ClearEffectsAsync method to remove all effects from a stream)
            await mediaCapture.ClearEffectsAsync(MediaStreamType.VideoPreview);

            // Clear the member variable that held the effect instance
            _faceDetectionEffect = null;
        }

        /// <summary>
        /// This method will update the icons, enable/disable and show/hide the photo/video buttons depending on the current state of the app and the capabilities of the device
        /// </summary>
        private void UpdateCaptureControls()
        {
            // The buttons should only be enabled if the preview started sucessfully
            takePhoto.IsEnabled = previewProperties != null;
            recordVideo.IsEnabled = previewProperties != null;
            faceDetect.IsEnabled = previewProperties != null;

            // Update the face detection icon depending on whether the effect exists or not
            FaceDetectionDisabledIcon.Visibility = (_faceDetectionEffect == null || !_faceDetectionEffect.Enabled) ? Visibility.Visible : Visibility.Collapsed;
            FaceDetectionEnabledIcon.Visibility = (_faceDetectionEffect != null && _faceDetectionEffect.Enabled) ? Visibility.Visible : Visibility.Collapsed;

            // Hide the face detection canvas and clear it
            FacesCanvas.Visibility = (_faceDetectionEffect != null && _faceDetectionEffect.Enabled) ? Visibility.Visible : Visibility.Collapsed;

            // Update recording button to show "Stop" icon instead of red "Record" icon when recording
            StartRecordingIcon.Visibility = isRecording ? Visibility.Collapsed : Visibility.Visible;
            StopRecordingIcon.Visibility = isRecording ? Visibility.Visible : Visibility.Collapsed;

            if (!mediaCapture.MediaCaptureSettings.ConcurrentRecordAndPhotoSupported)
            {
                takePhoto.IsEnabled = !isRecording;

                // Make the button invisible if it's disabled, so it's obvious it cannot be interacted with
                takePhoto.Opacity = takePhoto.IsEnabled ? 1 : 0;
            }
        }

        /// <summary>
        /// Iterates over all detected faces, creating and adding Rectangles to the FacesCanvas as face bounding boxes
        /// </summary>
        /// <param name="faces">The list of detected faces from the FaceDetected event of the effect</param>
        private void HighlightDetectedFaces(IReadOnlyList<DetectedFace> faces)
        {
            // Remove any existing rectangles from previous events
            FacesCanvas.Children.Clear();

            // For each detected face
            for (int i = 0; i < faces.Count; i++)
            {
                // Face coordinate units are preview resolution pixels, which can be a different scale from our display resolution, so a conversion may be necessary
                Rectangle faceBoundingBox = ConvertPreviewToUiRectangle(faces[i].FaceBox);

                // Set bounding box stroke properties
                faceBoundingBox.StrokeThickness = 2;

                // Highlight the first face in the set
                faceBoundingBox.Stroke = (i == 0 ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.DeepSkyBlue));

                // Add grid to canvas containing all face UI objects
                FacesCanvas.Children.Add(faceBoundingBox);
            }

            // Update the face detection bounding box canvas orientation
            SetFacesCanvasRotation();
        }

        /// <summary>
        /// Takes face information defined in preview coordinates and returns one in UI coordinates, taking
        /// into account the position and size of the preview control.
        /// </summary>
        /// <param name="faceBoxInPreviewCoordinates">Face coordinates as retried from the FaceBox property of a DetectedFace, in preview coordinates.</param>
        /// <returns>Rectangle in UI (CaptureElement) coordinates, to be used in a Canvas control.</returns>
        private Rectangle ConvertPreviewToUiRectangle(BitmapBounds faceBoxInPreviewCoordinates)
        {
            var result = new Rectangle();
            var previewStream = previewProperties as VideoEncodingProperties;

            // If there is no available information about the preview, return an empty rectangle, as re-scaling to the screen coordinates will be impossible
            if (previewStream == null) return result;

            // Similarly, if any of the dimensions is zero (which would only happen in an error case) return an empty rectangle
            if (previewStream.Width == 0 || previewStream.Height == 0) return result;

            double streamWidth = previewStream.Width;
            double streamHeight = previewStream.Height;

            // For portrait orientations, the width and height need to be swapped
            if (_displayOrientation == DisplayOrientations.Portrait || _displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                streamHeight = previewStream.Width;
                streamWidth = previewStream.Height;
            }

            // Get the rectangle that is occupied by the actual video feed
            var previewInUI = GetPreviewStreamRectInControl(previewStream, previewElement);

            // Scale the width and height from preview stream coordinates to window coordinates
            result.Width = (faceBoxInPreviewCoordinates.Width / streamWidth) * previewInUI.Width;
            result.Height = (faceBoxInPreviewCoordinates.Height / streamHeight) * previewInUI.Height;

            // Scale the X and Y coordinates from preview stream coordinates to window coordinates
            var x = (faceBoxInPreviewCoordinates.X / streamWidth) * previewInUI.Width;
            var y = (faceBoxInPreviewCoordinates.Y / streamHeight) * previewInUI.Height;
            Canvas.SetLeft(result, x);
            Canvas.SetTop(result, y);

            return result;
        }

        /// <summary>
        /// Calculates the size and location of the rectangle that contains the preview stream within the preview control, when the scaling mode is Uniform
        /// </summary>
        /// <param name="previewResolution">The resolution at which the preview is running</param>
        /// <param name="previewControl">The control that is displaying the preview using Uniform as the scaling mode</param>
        /// <returns></returns>
        public Rect GetPreviewStreamRectInControl(VideoEncodingProperties previewResolution, CaptureElement previewElement)
        {
            var result = new Rect();

            // In case this function is called before everything is initialized correctly, return an empty result
            if (previewElement == null || previewElement.ActualHeight < 1 || previewElement.ActualWidth < 1 ||
                previewResolution == null || previewResolution.Height == 0 || previewResolution.Width == 0)
            {
                return result;
            }

            var streamWidth = previewResolution.Width;
            var streamHeight = previewResolution.Height;

            // For portrait orientations, the width and height need to be swapped
            if (_displayOrientation == DisplayOrientations.Portrait || _displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                streamWidth = previewResolution.Height;
                streamHeight = previewResolution.Width;
            }

            // Start by assuming the preview display area in the control spans the entire width and height both (this is corrected in the next if for the necessary dimension)
            result.Width = previewElement.ActualWidth;
            result.Height = previewElement.ActualHeight;

            // If UI is "wider" than preview, letterboxing will be on the sides
            if ((previewElement.ActualWidth / previewElement.ActualHeight > streamWidth / (double)streamHeight))
            {
                var scale = previewElement.ActualHeight / streamHeight;
                var scaledWidth = streamWidth * scale;

                result.X = (previewElement.ActualWidth - scaledWidth) / 2.0;
                result.Width = scaledWidth;
            }
            else // Preview stream is "wider" than UI, so letterboxing will be on the top+bottom
            {
                var scale = previewElement.ActualWidth / streamWidth;
                var scaledHeight = streamHeight * scale;

                result.Y = (previewElement.ActualHeight - scaledHeight) / 2.0;
                result.Height = scaledHeight;
            }

            return result;
        }


        /// <summary>
        /// Converts the given orientation of the app on the screen to the corresponding rotation in degrees
        /// </summary>
        /// <param name="orientation">The orientation of the app on the screen</param>
        /// <returns>An orientation in degrees</returns>
        private static int ConvertDisplayOrientationToDegrees(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.Portrait:
                    return 90;
                case DisplayOrientations.LandscapeFlipped:
                    return 180;
                case DisplayOrientations.PortraitFlipped:
                    return 270;
                case DisplayOrientations.Landscape:
                default:
                    return 0;
            }
        }

        private void SetFacesCanvasRotation()
        {
            // Calculate how much to rotate the canvas
            int rotationDegrees = ConvertDisplayOrientationToDegrees(_displayOrientation);

            // The rotation direction needs to be inverted if the preview is being mirrored, just like in SetPreviewRotationAsync
            if (_mirroringPreview)
            {
                rotationDegrees = (360 - rotationDegrees) % 360;
            }

            // Apply the rotation
            var transform = new RotateTransform { Angle = rotationDegrees };
            FacesCanvas.RenderTransform = transform;

            var previewArea = GetPreviewStreamRectInControl(previewProperties as VideoEncodingProperties, previewElement);

            // For portrait mode orientations, swap the width and height of the canvas after the rotation, so the control continues to overlap the preview
            if (_displayOrientation == DisplayOrientations.Portrait || _displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                FacesCanvas.Width = previewArea.Height;
                FacesCanvas.Height = previewArea.Width;

                // The position of the canvas also needs to be adjusted, as the size adjustment affects the centering of the control
                Canvas.SetLeft(FacesCanvas, previewArea.X - (previewArea.Height - previewArea.Width) / 2);
                Canvas.SetTop(FacesCanvas, previewArea.Y - (previewArea.Width - previewArea.Height) / 2);
            }
            else
            {
                FacesCanvas.Width = previewArea.Width;
                FacesCanvas.Height = previewArea.Height;

                Canvas.SetLeft(FacesCanvas, previewArea.X);
                Canvas.SetTop(FacesCanvas, previewArea.Y);
            }

            // Also mirror the canvas if the preview is being mirrored
            FacesCanvas.FlowDirection = _mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
        #endregion

        #endregion web camera and face detection

        private void previousPhoto_Click(object sender, RoutedEventArgs e)
        {

        }

        private void nextPhoto_Click(object sender, RoutedEventArgs e)
        {

        }

        private void previousVideo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void playOrStopVideo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void nextVideo_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
