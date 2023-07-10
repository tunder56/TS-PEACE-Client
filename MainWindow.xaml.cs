
using System;
using System.Collections.Generic;
using System.Linq;
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
            throw new NotImplementedException();
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
