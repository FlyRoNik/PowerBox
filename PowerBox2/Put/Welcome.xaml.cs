using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class Welcome : Page
    {
        private Box box;
        private bool flag;

        private MySemaphore _pool = new MySemaphore(1, 2);
        private MySemaphore _pool2 = new MySemaphore(1, 2);

        public Welcome()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled; //сохранение состояния страницы

            flag = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _pool.Wait();
                flag = true;
                box.scaner.genExcept(); //генерация ошибки для освобождения потока
            }
            catch (Exception) { }
            finally
            {
                _pool.TryRelease();
            }
            _pool2.Wait();
            _pool2.TryRelease();
            this.Frame.Navigate(typeof(СellSelection), box);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Box)
            {
                box = (Box)e.Parameter;
            }
            base.OnNavigatedTo(e);

            Task thread = new Task(() => {
                scanning();
            });
            thread.Start();
        }

        private async void scanning()
        {
            while (true)
            {
                try
                {
                    _pool2.Wait();
                    box.numberCell = box.scaner.compareOneToMore().getID();
                    _pool.Wait();
                    if (flag)
                    {
                        flag = false;
                        return;
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (flag)
                    {
                        flag = false;
                        return;
                    }
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        // This now works, because it's running on the UI thread:
                        textBlock1.Text = ex.Message;
                    });
                }
                finally
                {
                    _pool.TryRelease();
                    _pool2.TryRelease();
                }
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.Frame.Navigate(typeof(Pick_up.PickUpDevice), box);
            });
        }
    }
}
