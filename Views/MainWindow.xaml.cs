using PUBattleGroundsCircleTimer.Enumerables;
using PUBattleGroundsCircleTimer.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PUBattleGroundsCircleTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<int> listSeconds;
        private BackgroundWorker bgwTimer;
        private SoundPlayer player;

        #region Properties

        private int IndexTimer { get; set; }

        private int SecondsLeft { get; set; }

        private TimerStatus TimerStatus { get; set; }
        
        private bool SoundsEnabled
        {
            get { return Settings.Default.SoundsEnabled; }
            set
            {
                Settings.Default.SoundsEnabled = value;
                Settings.Default.Save();
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            player = new SoundPlayer(Properties.Resources.drumsticks);
            bgwTimer = new BackgroundWorker();
            bgwTimer.WorkerSupportsCancellation = true;
            bgwTimer.WorkerReportsProgress = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTimers();
            TimerStatus = TimerStatus.Paused;
            txtToggleSound.Text = SoundsEnabled ? "SONIDO: ON" : "SONIDO: OFF";
        }

        #region Component Events

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            switch (TimerStatus)
            {
                case TimerStatus.Playing:
                    ResetTimer();
                    break;
                case TimerStatus.Paused:
                    PlayTimer();
                    break;
            }
        }

        private void btnToggleSound_Click(object sender, RoutedEventArgs e)
        {
            SoundsEnabled = !SoundsEnabled;
            txtToggleSound.Text = SoundsEnabled ? "SONIDO: ON" : "SONIDO: OFF";
        }

        #endregion

        #region Private Methods

        private void PlayTimer()
        {
            try
            {
                TimerStatus = TimerStatus.Playing;
                txtPlayPause.Text = "PAUSE";
                bgwTimer.DoWork += BgwTimer_DoWork;
                bgwTimer.ProgressChanged += BgwTimer_ProgressChanged;
                bgwTimer.RunWorkerCompleted += BgwTimer_RunWorkerCompleted;
                if (!bgwTimer.IsBusy)
                {
                    bgwTimer.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                ResetTimer();
            }
        }

        private void ResetTimer()
        {
            try
            {
                TimerStatus = TimerStatus.Paused;
                txtPlayPause.Text = "PLAY";
                if (bgwTimer != null)
                {
                    if (bgwTimer.WorkerSupportsCancellation)
                    {
                        bgwTimer.CancelAsync();
                    }
                    bgwTimer.DoWork -= BgwTimer_DoWork;
                    bgwTimer.ProgressChanged -= BgwTimer_ProgressChanged;
                    bgwTimer.RunWorkerCompleted -= BgwTimer_RunWorkerCompleted;
                }
                InitializeTimers();
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }
        }

        private void InitializeTimers()
        {
            lblMainTimer.Text = "02:00";
            lblWaitOne.Text = "05:00";
            lblWaitTwo.Text = "03:30";
            lblWaitThree.Text = "02:30";
            lblWaitFour.Text = "02:00";
            lblWaitFive.Text = "02:00";
            lblCloseOne.Text = "05:00";
            lblCloseTwo.Text = "02:30";
            lblCloseThree.Text = "01:30";
            lblCloseFour.Text = "01:30";
            lblCloseFive.Text = "01:00";

            listSeconds = new List<int> { 120, 300, 300, 210, 150, 150, 90, 120, 90, 120, 60 };
            IndexTimer = 0;
            SecondsLeft = listSeconds[IndexTimer];
            lblMainTimer.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void BgwTimer_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (IndexTimer > 0)
            {
                Dispatcher.Invoke(() => { lblMainTimer.Foreground = new SolidColorBrush((IndexTimer % 2 != 0) ? Colors.Red : Colors.Blue); });
            }

            int auxSeconds = SecondsLeft;
            for (int i = auxSeconds; i >= 0; i--)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    SecondsLeft = i;
                    var perc = 100 - ((SecondsLeft * 100) / auxSeconds);
                    worker.ReportProgress(perc);

                    Thread.Sleep(1000);
                }
            }
        }

        private void BgwTimer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TimeSpan timer = new TimeSpan(0, 0, SecondsLeft);
            var sTimer = string.Format("{0}:{1}", timer.Minutes.ToString("00"), timer.Seconds.ToString("00"));
            lblMainTimer.Text = sTimer;
            switch (IndexTimer)
            {
                case 1:
                    lblWaitOne.Text = sTimer;
                    break;
                case 2:
                    lblCloseOne.Text = sTimer;
                    break;
                case 3:
                    lblWaitTwo.Text = sTimer;
                    break;
                case 4:
                    lblCloseTwo.Text = sTimer;
                    break;
                case 5:
                    lblWaitThree.Text = sTimer;
                    break;
                case 6:
                    lblCloseThree.Text = sTimer;
                    break;
                case 7:
                    lblWaitFour.Text = sTimer;
                    break;
                case 8:
                    lblCloseFour.Text = sTimer;
                    break;
                case 9:
                    lblWaitFive.Text = sTimer;
                    break;
                case 10:
                    lblCloseFive.Text = sTimer;
                    break;
            }

            if (IndexTimer % 2 != 0)
            {
                switch (SecondsLeft)
                {
                    case 60:
                        PlaySound(1);
                        break;
                    case 30:
                        PlaySound(2);
                        break;
                    case 0:
                        PlaySound(5);
                        break;
                }
            }
        }

        private void BgwTimer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //ERROR
            }
            else if (e.Cancelled)
            {
                //CANCELLED
                ResetTimer();
            }
            else
            {
                if (IndexTimer < 10)
                {
                    IndexTimer++;
                    SecondsLeft = listSeconds[IndexTimer];
                    if (!bgwTimer.IsBusy)
                    {
                        bgwTimer.RunWorkerAsync();
                    }
                }
                else
                {
                    ResetTimer();
                }
            }
        }

        private void PlaySound(int repeat)
        {
            if (SoundsEnabled)
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < repeat; i++)
                    {
                        player.PlaySync();
                    }
                });
            }
        }

        #endregion
    }
}
