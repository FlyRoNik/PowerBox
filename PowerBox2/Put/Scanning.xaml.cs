﻿using System;
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

        private Type previousPageType;

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

            previousPageType = Frame.BackStack.Last().SourcePageType;

            Task thread = new Task(() => {
                add(box.privilege);
            });
            thread.Start();

            base.OnNavigatedTo(e);
        }

        private void add(FingerPrintScaner.Privilege privilege)
        {
            string status;

            while (flag)
            {
                status = Progres(FingerPrintScaner.Times.First, privilege);
                if (status == "Operation successfully")
                {
                    dispatch(() => { textBlock1.Text = status; });
                    status = Progres(FingerPrintScaner.Times.Second, privilege);
                    if (status == "Operation successfully")
                    {
                        dispatch(() => { textBlock1.Text = status; });
                        status = Progres(FingerPrintScaner.Times.Third, privilege);
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
                    return;
                }
            }
            _pool2.TryRelease();

            if (previousPageType != typeof(СellSelection))
            {
                dispatch(() => { this.Frame.Navigate(previousPageType, box); });
            }
            else
            {
                dispatch(() => { this.Frame.Navigate(typeof(PutDevice), box); });
            }
        }

        private string Progres(FingerPrintScaner.Times times, FingerPrintScaner.Privilege privilege)
        {
            dispatch(() =>
            {
                progress1.IsActive = true;
            });
            string st = "";

            _pool3.Wait();
            if (!inter)
            {
                st = box.scaner.addFingerPrint(box.numberCell, privilege, times);
            }
            _pool3.TryRelease();

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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                inter = true;
                if (_pool3.count == 0)
                {
                    box.scaner.genExcept(); //генерация ошибки для освобождения потока
                }
            }
            catch (Exception) { }
            _pool2.Wait();

            if (flag)
            {
                if (previousPageType != typeof(СellSelection))
                {
                    this.Frame.Navigate(previousPageType, box);
                }
                else
                {
                    this.Frame.Navigate(typeof(СellSelection), box);
                }
            }
        }
    }
}
