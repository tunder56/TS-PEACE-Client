using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TS_PEACE_Client.Windows.Game_windows
{
    /// <summary>
    /// Interaction logic for Bunkerscontrol.xaml
    /// </summary>
    public partial class Bunkerscontrol : Window
    {
        // Timer variables
        TimeSpan Attak1_timerVar;
        TimeSpan Attak2_timerVar;
        TimeSpan Attak3_timerVar;
        TimeSpan Attak4_timerVar;

        // colour variables
        SolidColorBrush Disabledfill = Brushes.Red;
        SolidColorBrush Disabledfilltext = Brushes.Red;
        SolidColorBrush Activefill = Brushes.Green;
        SolidColorBrush ActivefillSelected = Brushes.YellowGreen;
        SolidColorBrush ActivefillSelectedBox = Brushes.YellowGreen;
        SolidColorBrush TargetedBorder = Brushes.LightPink;
        SolidColorBrush Untargeted = Brushes.Black;
     


        // status variables
        bool Attack1_status = false;
        bool Attack2_status = false;
        bool Attack3_status = false;
        bool Attack4_status = false;


        // targeting list variable

        List<string> targeting = new List<string>();


        // string variables
        string Selfuser = "testuser";
        
        // city class
        public class city
        {
            public string name;
            public int x;
            public int y;
            public string owner;
        }


        List<city> citylist = new List<city>();

        public Bunkerscontrol()
        {
            InitializeComponent();
            Timersetup();
            Timertick1();
            Timertick2();
            Timertick3();
            Timertick4();
        }


        // timer methods

        private void Timersetup()
        {
            // Set Starting timers and set them to tick

            Attak1_timerVar = new TimeSpan(00, 00, 1);
            this.Attack1_timer.Text = Attak1_timerVar.ToString();
            Attak2_timerVar = new TimeSpan(00, 00, 1);
            this.Attack2_timer.Text = Attak2_timerVar.ToString();
            Attak3_timerVar = new TimeSpan(00, 00, 1);
            this.Attack3_timer.Text = Attak3_timerVar.ToString();
            Attak4_timerVar = new TimeSpan(00, 00, 1);
            this.Attack4_timer.Text = Attak4_timerVar.ToString();
        }


        public async Task Timertick1()
        {
            // Disable attack 1 and set attack timer 1 to tick, when zero re-enable

            Attack1_status = false;
            this.Dispatcher.Invoke(() =>
            {
                Attack1_box.Fill = Disabledfill;
                Attack1_name.Foreground = Disabledfilltext;
                Attack1_timer.Foreground = Disabledfilltext;
            });


            while (Attak1_timerVar > new TimeSpan())
            {
                await Task.Delay(1000);
                Attak1_timerVar = Attak1_timerVar.Subtract(new TimeSpan(0, 0, 1));
                this.Dispatcher.Invoke(() =>
                {
                    this.Attack1_timer.Text = Attak1_timerVar.ToString();
                });

            }
            this.Dispatcher.Invoke(() =>
            {
                Attack1_box.Fill = Activefill;
                Attack1_name.Foreground = ActivefillSelected;
                Attack1_timer.Foreground = ActivefillSelected;
            });

            Attack1_status = true;
        }

        public async Task Timertick2()
        {
            // Disable attack 2 and set attack timer 2 to tick, when zero re-enable

            Attack2_status = false;
            this.Dispatcher.Invoke(() =>
            {
                Attack2_box.Fill = Disabledfill;
                Attack2_name.Foreground = Disabledfilltext;
                Attack2_timer.Foreground = Disabledfilltext;
            });


            while (Attak2_timerVar > new TimeSpan())
            {
                await Task.Delay(1000);
                Attak2_timerVar = Attak2_timerVar.Subtract(new TimeSpan(0, 0, 1));
                this.Dispatcher.Invoke(() =>
                {
                    this.Attack2_timer.Text = Attak2_timerVar.ToString();
                });

            }
            this.Dispatcher.Invoke(() =>
            {
                Attack2_box.Fill = Activefill;
                Attack2_name.Foreground = ActivefillSelected;
                Attack2_timer.Foreground = ActivefillSelected;
            });

            Attack2_status = true;
        }

        public async Task Timertick3()
        {
            // Disable attack 3 and set attack timer 3 to tick, when zero re-enable

            Attack3_status = false;
            this.Dispatcher.Invoke(() =>
            {
                Attack3_box.Fill = Disabledfill;
                Attack3_name.Foreground = Disabledfilltext;
                Attack3_timer.Foreground = Disabledfilltext;
            });


            while (Attak3_timerVar > new TimeSpan())
            {
                await Task.Delay(1000);
                Attak3_timerVar = Attak3_timerVar.Subtract(new TimeSpan(0, 0, 1));
                this.Dispatcher.Invoke(() =>
                {
                    this.Attack3_timer.Text = Attak3_timerVar.ToString();
                });

            }
            this.Dispatcher.Invoke(() =>
            {
                Attack3_box.Fill = Activefill;
                Attack3_name.Foreground = ActivefillSelected;
                Attack3_timer.Foreground = ActivefillSelected;
            });

            Attack3_status = true;
        }

        public async Task Timertick4()
        {
            // Disable attack 4 and set attack timer 4 to tick, when zero re-enable

            Attack4_status = false;
            this.Dispatcher.Invoke(() =>
            {
                Attack4_box.Fill = Disabledfill;
                Attack4_name.Foreground = Disabledfilltext;
                Attack4_timer.Foreground = Disabledfilltext;
            });


            while (Attak4_timerVar > new TimeSpan())
            {
                await Task.Delay(1000);
                Attak4_timerVar = Attak4_timerVar.Subtract(new TimeSpan(0, 0, 1));
                this.Dispatcher.Invoke(() =>
                {
                    this.Attack4_timer.Text = Attak4_timerVar.ToString();
                });

            }
            this.Dispatcher.Invoke(() =>
            {
                Attack4_box.Fill = Activefill;
                Attack4_name.Foreground = ActivefillSelected;
                Attack4_timer.Foreground = ActivefillSelected;
            });

            Attack4_status = true;
        }


        // targeting methods

        private void map_click(object sender, RoutedEventArgs e)
        {
            // get the name of the ellipse clicked, add to targeting list, unless allready in list then remove

            var trigger = (Ellipse)sender;

            var cityclicked = citylist.Find(i => i.name == trigger.Name);

            if (cityclicked.owner != Selfuser)
            {
                if (!Targeting_list_display.Items.Contains(trigger.Name))
                {
                    Targeting_list_display.Items.Insert(0, trigger.Name);
                    trigger.Stroke = TargetedBorder;

                }
                else
                {
                    Targeting_list_display.Items.Remove(trigger.Name);
                    trigger.Stroke = Untargeted;
                }
            }
        }

    }
}
