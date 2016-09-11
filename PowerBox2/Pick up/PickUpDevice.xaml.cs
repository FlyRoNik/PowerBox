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

namespace PowerBox2.Pick_up
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class PickUpDevice : Page
    {
        private Box box;

        public PickUpDevice()
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

            box.mcu[box.numberCell].setPinDigital(6, 1); // открыть ячейку

            Task thread = new Task(() => {
                waitingClosing();
            });
            thread.Start();
        }

        private void waitingClosing()
        {
            while (true)
            {
                if (box.mcu[box.numberCell].getDataMicrocontroller()[1] == 1)
                {
                    box.mcu[box.numberCell].setPinDigital(6, 0); // закрыть ячейку
                    break;
                }
                Task.Delay(-1).Wait(100);
            }
            dispatch(() => { this.Frame.Navigate(typeof(Goodbye), box); });
        }

        private async void dispatch(DispatchedHandler agileCallback)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, agileCallback);
        }
    }
}
