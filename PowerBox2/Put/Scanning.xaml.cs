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

namespace PowerBox2.Put
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Scanning : Page
    {
        private Box box;
        private bool inter;
        private bool flag;

        private MySemaphore _pool = new MySemaphore(0, 1);
        private MySemaphore _pool2 = new MySemaphore(0, 1);
        private MySemaphore _pool3 = new MySemaphore(1, 1);

        public Scanning()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled; //сохранение состояния страницы
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            inter = false;
            flag = true;

            if (e.Parameter is Box)
            {
                box = (Box)e.Parameter;
            }
            base.OnNavigatedTo(e);
            Debag.Write("S51 ");

            Task thread = new Task(() => {
                addUser();
            });
            thread.Start();
        }

        private void addUser()
        {
            string status;

            Debag.Write("S62 ");
            while (flag)
            {
                
                status = Progres(FingerPrintScaner.Times.First);
                if (status == "Operation successfully")
                {
                    dispatch(() => { textBlock1.Text = status; });
                    status = Progres(FingerPrintScaner.Times.Second);
                    if (status == "Operation successfully")
                    {
                        dispatch(() => { textBlock1.Text = status; });
                        status = Progres(FingerPrintScaner.Times.Third);
                        if (status == "Operation successfully")
                        {
                            dispatch(() => { textBlock1.Text = status; });
                            flag = false;
                        }
                        else
                        {
                            dispatch(() => { textBlock1.Text = status; });
                        }
                    }
                    else
                    {
                        dispatch(() => { textBlock1.Text = status; });
                    }
                }
                else
                {
                    dispatch(() =>{ textBlock1.Text = status; });
                }

                if (inter && flag)
                {
                    inter = false;
                    _pool2.TryRelease();
                    Debag.Write("S100 ");
                    return;
                }
            }
            _pool2.TryRelease();
            Debag.Write("S103 ");
            dispatch(() =>{ this.Frame.Navigate(typeof(PutDevice), box); });
        }

        private string Progres(FingerPrintScaner.Times times)
        {
            dispatch(() =>
            {
                progress1.IsActive = true;
            });
            string st = "";

            Debag.Write("S117 ");
            _pool3.Wait();
            if (!inter)
            {
                Debag.Write("S121 ");
                st = box.scaner.addFingerPrint(box.numberCell, FingerPrintScaner.Privilege.USER, times);
                Debag.Write("S123 ");
            }
            _pool3.TryRelease();

            dispatch(() =>
            {
                progress1.IsActive = false;
            });
            Debag.Write("S131 ");
            return st;
        }

        private async void dispatch(DispatchedHandler agileCallback)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, agileCallback);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Debag.Write("S142 ");
            try
            {
                inter = true;
                if (_pool3.count == 0)
                {
                    box.scaner.genExcept(); //генерация ошибки для освобождения потока
                }
            }
            catch (Exception) { }
            Debag.Write("S152 ");
            _pool2.Wait();

            if (flag)
            {
                Debag.Write("S157 ");
                this.Frame.Navigate(typeof(СellSelection), box);
            }
        }
    }
}
