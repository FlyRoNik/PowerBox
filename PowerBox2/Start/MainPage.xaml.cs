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
        }

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
    }
}
