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

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace PowerBox2.Put
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class СellSelection : Page
    {
        private Box box;
        private Button[] buttons;

        public СellSelection()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled; //сохранение состояния страницы
            buttons = new Button[] { button, button1, button2, button3, button4, button5, button6, button7 };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Box)
            {
                box = (Box)e.Parameter;
            }
            base.OnNavigatedTo(e);

            FingerPrintScaner.User[] user = null;
            try
            {
                user = box.scaner.getUserNumbersAndPrivilege();
            }
            catch (Exception ex)
            {
                textBlock.Text = ex.Message;
                return;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].IsEnabled = true;
            }

            for (int i = 0; i < user.Length; i++)
            {
                if (user[i].getID() < 9)
                {
                    buttons[user[i].getID()].IsEnabled = false;
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            box.numberCell = ((Button)sender).TabIndex;
            this.Frame.Navigate(typeof(Scanning), box);
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Welcome), box);
        }
    }
}
