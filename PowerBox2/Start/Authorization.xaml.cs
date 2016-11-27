using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerBox2.Start
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Authorization : Page
    {
        private Box box;

        public Authorization()
        {
            this.InitializeComponent();
            try
            {
                box = new Box();
            }
            catch (Exception ex)
            {
                textBlock.Text = ex.Message;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Box)
            {
                box = (Box)e.Parameter;
            }
            base.OnNavigatedTo(e);

            try
            {
                string respon = box.scaner.setTimeoutValue(5);
                if (respon != "Operation successfully")
                {
                    throw new Exception(respon);
                }
            }
            catch (Exception ex)
            {
                box.debag.WriteSD_Debag(ex.Message);
            }

            Task thread = new Task(() => {
                scanning();
            });
            thread.Start();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            string respon = box.scaner.setTimeoutValue(0);
            if (respon != "Operation successfully")
            {
                box.debag.WriteSD_Debag(respon);
            }
            base.OnNavigatingFrom(e);
        }

        private async void scanning()
        {
            try
            {
                FingerPrintScaner.Person user = null;
                try
                {
                    user = box.scaner.compareOneToMore();
                }
                catch (Exception e)
                {
                    box.debag.WriteSD_Debag(e.Message);
                }
                if (user != null && user.getPrivilege() == FingerPrintScaner.Privilege.ADMIN)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        this.Frame.Navigate(typeof(MainPage), box);
                    });
                }
                else
                {
                    while (true)
                    {
                        int n = isClosed();
                        if (n == 0)
                        {
                            break;
                        }
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            textBlock.Text = "Ячейка №" + n + " не закрыта! Закройте пожалуйста.";
                        });
                        Task.Delay(-1).Wait(1000);
                    }
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        this.Frame.Navigate(typeof(Put.Welcome), box);
                    });
                }
            }
            catch (Exception ex)
            {
                box.debag.WriteSD_Debag(ex.Message);
            }
        }

        private int isClosed()
        {
            //проверка закрытости ячеек
            for (int i = 0; i < /*box.mcu.Length*/2; i++)
            {
                if (box.mcu[i].getDataMicrocontroller()[1] == 0)
                {
                    return i + 1;
                }
            }
            return 0;
        }
    }
}
