using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

            box.cam.Cleanup();
            box.cam = new Camera(box.debag, 
                delegatePrint, 
                delegateFailed, 
                delegateRecordLimitExceeded);
            SetInitButtonVisibility(Action.ENABLE);
            SetVideoButtonVisibility(Action.DISABLE);
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

            box.cam = new Camera(box.debag);

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
            if (!camFaceDet._isRecording)
            {
                await camFaceDet.StartRecordingAsync();
            }
            else
            {
                await camFaceDet.StopRecordingAsync();
            }
        }

        private async void button3_Click(object sender, RoutedEventArgs e)
        {
            await camFaceDet.TakePhotoAsync();
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

        #region web camera and face detection
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
            }
            else
            {
                video_init.IsEnabled = false;
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
            }
            else
            {
                takePhoto.IsEnabled = false;
                takePhoto.Visibility = Visibility.Collapsed;

                recordVideo.IsEnabled = false;
                recordVideo.Visibility = Visibility.Collapsed;
            }
        }

        public async void delegatePrint(string status)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.status.Text = status.ToString();
            });
        }

        public async void delegateFailed()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetInitButtonVisibility(Action.DISABLE);
                SetVideoButtonVisibility(Action.DISABLE);
                this.status.Text = status.ToString();
            });
        }

        public async void delegateRecordLimitExceeded()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                recordVideo.Content = "Start Video Record";
                this.status.Text = status.ToString();
            });
        }
        #endregion

        private async void initVideo_Click(object sender, RoutedEventArgs e)
        {
            // Disable all buttons until initialization completes

            SetInitButtonVisibility(Action.DISABLE);
            SetVideoButtonVisibility(Action.DISABLE);

            if (box.cam.getMediaCapture() != null)
            {
                // Cleanup MediaCapture object
                if (box.cam.getIsPreviewing())
                {
                    captureImage.Source = null;
                    playbackElement.Source = null;
                }
                if (box.cam.getIsRecording())
                {
                    recordVideo.Content = "Start Video Record";
                }
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                box.cam.initVideo(previewElement);
            });

            // Start Preview                
            //previewElement.Source = box.cam.getMediaCapture();
            //await box.cam.getMediaCapture().StartPreviewAsync();

            // Enable buttons for video and photo capture
            SetVideoButtonVisibility(Action.ENABLE);
        }

        private void cleanup_Click(object sender, RoutedEventArgs e)
        {
            if (box.cam.getMediaCapture() != null)
            {
                // Cleanup MediaCapture object
                if (box.cam.getIsPreviewing())
                {
                    captureImage.Source = null;
                    playbackElement.Source = null;
                }
                if (box.cam.getIsRecording())
                {
                    recordVideo.Content = "Start Video Record";
                }
            }

            SetInitButtonVisibility(Action.DISABLE);
            SetVideoButtonVisibility(Action.DISABLE);
            box.cam.Cleanup();
            SetInitButtonVisibility(Action.ENABLE);
        }

        private void takePhoto_Click(object sender, RoutedEventArgs e)
        {
            takePhoto.IsEnabled = false;
            recordVideo.IsEnabled = false;

            captureImage.Source = box.cam.getBitmap();

            takePhoto.IsEnabled = true;
            recordVideo.IsEnabled = true;
        }

        private void recordVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                takePhoto.IsEnabled = false;
                recordVideo.IsEnabled = false;
                playbackElement.Source = null;

                if (recordVideo.Content.ToString() == "Start Video Record")
                {
                    takePhoto.IsEnabled = false;
                    status.Text = "Initialize video recording";

                    box.cam.recordVideo();

                    recordVideo.IsEnabled = true;
                    recordVideo.Content = "Stop Video Record";
                }
                else
                {
                    takePhoto.IsEnabled = true;

                    box.cam.recordVideo();

                    var stream = box.cam.getStream();

                    StorageFile recordStorageFile = box.cam.getRecordStorageFile();

                    playbackElement.AutoPlay = true;
                    playbackElement.SetSource(stream, recordStorageFile.FileType);
                    playbackElement.Play();
                    status.Text = "Playing recorded video" + recordStorageFile.Path;
                    recordVideo.Content = "Start Video Record";
                }
            }
            catch (Exception ex)
            {
                if (ex is System.UnauthorizedAccessException)
                {
                    status.Text = "Unable to play recorded video; video recorded successfully to: " + box.cam.getRecordStorageFile().Path;
                    recordVideo.Content = "Start Video Record";
                }
                else
                {
                    status.Text = ex.Message;
                }
            }
            finally
            {
                recordVideo.IsEnabled = true;
            }
        }

        #endregion web camera and face detection
    }
}
