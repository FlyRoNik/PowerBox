using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        public MainPage()
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //проверка закрытости ячеек
            for (int i = 0; i < /*box.mcu.Length*/2; i++)
            {
                if (box.mcu[i].getDataMicrocontroller()[1] == 0)
                {
                    int n = i + 1;
                    textBlock.Text = "Ячейка №" + n + " не закрыта";
                    return;
                }
            }

            this.Frame.Navigate(typeof(Put.Welcome), box);
        }
    }
}
