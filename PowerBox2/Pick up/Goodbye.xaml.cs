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
    public sealed partial class Goodbye : Page
    {
        private Box box;

        public Goodbye()
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
            Debag.Write("G42 ");
            Task thread = new Task(() => {
                next();
            });
            thread.Start();
        }

        private async void next()
        {
            Debag.Write("G51 ");
            Task.Delay(-1).Wait(2000);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Debag.Write("G55 ");
                this.Frame.Navigate(typeof(Put.Welcome), box);
            });
        }
    }
}
