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

        public Scanning()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled; //сохранение состояния страницы
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Box)
            {
                box = (Box)e.Parameter;
            }
            base.OnNavigatedTo(e);

            Task thread = new Task(() => {
                addUser();
            });
            thread.Start();
        }

        private void addUser()
        {
            string status;

            bool flag = true;

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
            }
            dispatch(() =>{ this.Frame.Navigate(typeof(PutDevice), box); });
        }

        private string Progres(FingerPrintScaner.Times times)
        {
            dispatch(() =>
            {
                progress1.IsActive = true;
            });

            string st = box.scaner.addFingerPrint(box.numberCell, FingerPrintScaner.Privilege.USER, times);

            dispatch(() =>
            {
                progress1.IsActive = false;
            });
            return st;
        }

        private async void dispatch(DispatchedHandler agileCallback)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, agileCallback);
        }

    }
}
