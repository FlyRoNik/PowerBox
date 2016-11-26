using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Core;
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
    public sealed partial class AreYouStillHere : Page
    {
        private const int TIMEOUT = 20000;

        MyTime time = new MyTime();

        Timer TheTimer = null;
        object LockObject = new object();
        double ProgressAmount = 0;
        int timeOut = TIMEOUT;

        public AreYouStillHere()
        {
            this.InitializeComponent();
            TheGrid.DataContext = time;
            time.GetProgress = 20;

            this.Loaded += (sender, e) =>
            {
                TimerCallback tcb = HandleTimerTick;
                TheTimer = new Timer(HandleTimerTick, null, 0, 50);

            };

            this.Unloaded += (sender, e) =>
            {
                lock (LockObject)
                {
                    if (TheTimer != null)
                        TheTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    TheTimer = null;
                }
            };
        }

        public void HandleTimerTick(Object state)
        {
            lock (LockObject)
            {
                SetBarLength(ProgressAmount);
                ProgressAmount += 0.0025;
                if (ProgressAmount > 1.0)
                    ProgressAmount = 0.0;
                timeOut -= 50;
                if (timeOut == 0)
                    timeOut = TIMEOUT;
            }
        }

        public void SetBarLength(double Value)
        {
            double Angle = 2 * Math.PI * Value;

            double X = 50 + Math.Sin(Angle) * 50;
            double Y = 50 - Math.Cos(Angle) * 50;
            if (Value > 0 && (int)X == 50 && (int)Y == 0)
                X += 0.01; // Never make the end the same as the start!

            // Run this on the UI thread because the IsLargeArc and Point values need to get set only in that thread.
            IAsyncAction TheTask =
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    TheSegment.IsLargeArc = Angle >= Math.PI;
                    TheSegment.Point = new Point(X, Y);
                    if (timeOut % 1000 == 0)
                    {
                        time.GetProgress = timeOut / 1000;
                    }
                });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

    class MyTime : INotifyPropertyChanged
    {
        private int progress;

        public int GetProgress
        {
            set
            {
                if (value != progress)
                {
                    progress = value;
                    NotifyPropertyChanged();
                }
            }
            get { return progress; }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
