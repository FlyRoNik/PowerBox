using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Box)
            {
                box = (Box)e.Parameter;
            }
            base.OnNavigatedTo(e);
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

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            box.addAdmin();
            this.Frame.Navigate(typeof(Put.Scanning), box);
        }
    }
}
