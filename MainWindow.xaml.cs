
using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using TS_PEACE_Client.Windows;
using TS_PEACE_Client.Windows.Game_windows;
using TS_PEACE_Client.Windows.Settings_windows;
using TS_PEACE_Client.Windows.Tutorial_windows;

namespace TS_PEACE_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

           
        }

        private void Bunkers(object sender, RoutedEventArgs e)
        {
            Bunkerscontrol win = new Bunkerscontrol();
            win.Show();
            this.Close();
        }

        private void RandomBunkers(object sender, RoutedEventArgs e)
        {
            loading l = new loading();

            Thread newWindowThread = new Thread(new ThreadStart(l.ThreadStartingPoint));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();

            RandomManiaBunkers win = new RandomManiaBunkers();
            win.Show();

            var dis = System.Windows.Threading.Dispatcher.FromThread(newWindowThread);
            dis.InvokeShutdown();
           
           this.Close();
            
        }

        private void Settings(object sender, RoutedEventArgs e)
        {
            Settingswindow win = new Settingswindow();
            win.Show();
            this.Close();
        }

        private void Modportal(object sender, RoutedEventArgs e)
        {
            var uri = "https://terrasymposium.com/";
            var psi = new System.Diagnostics.ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = uri;
            System.Diagnostics.Process.Start(psi);
        }

        private void Tutorial(object sender, RoutedEventArgs e)
        {
            Tutorialpart1 win = new Tutorialpart1();
            win.Show();
            this.Close();
        }

        class loading
        {
            public CancellationToken token;
            public void ThreadStartingPoint()
            {

                loadingwindow tempWindow = new loadingwindow();
                tempWindow.Show();
                System.Windows.Threading.Dispatcher.Run();
                
            }
            

        }

       

    }
}
