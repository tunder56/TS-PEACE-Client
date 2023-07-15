using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

namespace TS_PEACE_Client.Windows.Tutorial_windows
{
    /// <summary>
    /// Interaction logic for Tutorialpart2.xaml
    /// </summary>
    public partial class Tutorialpart2 : Window
    {
        // status variables
        bool Attack1_status = false;
        bool Attack2_status = false;
        bool Attack3_status = false;
        bool Attack4_status = false;
        int tutorialpart = 1;

        // city class
        public class city
        {
            public string name;
            public int x;
            public int y;
            public string owner;
        }

        //initalise city list
        List<city> citylist = new List<city>();

        // variables to handle username and colors
        string Selfuser = "Team1";
        SolidColorBrush Disabledfill = new SolidColorBrush();
        SolidColorBrush Disabledfilltext = new SolidColorBrush();
        SolidColorBrush Activefill = new SolidColorBrush();
        SolidColorBrush ActivefillSelected = new SolidColorBrush();
        SolidColorBrush ActivefillSelectedBox = new SolidColorBrush();
        SolidColorBrush TargetedBorder = new SolidColorBrush();
        SolidColorBrush Untargeted = new SolidColorBrush();
        SolidColorBrush Attack1Hitfill = new SolidColorBrush();
        SolidColorBrush Attack2Hitfill = new SolidColorBrush();
        SolidColorBrush Attack3Hitfill = new SolidColorBrush();
        SolidColorBrush Attack4Hitfill = new SolidColorBrush();
        SolidColorBrush OwnLand = new SolidColorBrush();
        SolidColorBrush Elseland = new SolidColorBrush();

        // timer variables
        TimeSpan Attak1_timerVar;
        TimeSpan Attak2_timerVar;
        TimeSpan Attak3_timerVar;
        TimeSpan Attak4_timerVar;

        // mainline code
        public Tutorialpart2()
        {
            InitializeComponent();



            // set colors

            OwnLand = new SolidColorBrush(Color.FromRgb(90, 105, 236));
            TargetedBorder.Color = Color.FromArgb(80, 255, 178, 178);

            Untargeted.Color = Color.FromArgb(0, 0, 0, 0);
            Elseland = new SolidColorBrush(Color.FromRgb(215, 94, 67));
            path151.Stroke = Elseland;
            path137.Stroke = Elseland;
            path141.Stroke = Elseland;

            Attack2_box.Fill = Disabledfill;
            Attack3_box.Fill= Disabledfill;
            Attack4_box.Fill= Disabledfill;


            path139.Stroke = OwnLand;
            path143.Stroke = OwnLand;
            path147.Stroke = OwnLand;
            path145.Stroke = OwnLand;

            // get all citys on map and add them to the city list with owners and locations
            IEnumerable<Ellipse> Circle = map.Children.OfType<Ellipse>();

            foreach (var c in Circle)
            {
                string ownerout = "none";
                if (c.Uid == "2")
                {
                    ownerout = "Team2";
                }
                else
                {
                    ownerout = "Team1";
                }
                citylist.Add(new city { name = c.Name, x = Convert.ToInt32(c.GetValue(Canvas.LeftProperty)), y = Convert.ToInt32(c.GetValue(Canvas.TopProperty)), owner = ownerout });
            }

            // set up timer
            setup();
            Timertick1();
            Part1();





        }

        private void Coloursetup()
        {
            // Modify Global Colours
            Disabledfill.Color = Color.FromArgb(100, 177, 0, 0);
            Disabledfill.Opacity = 20;

            Disabledfilltext.Color = Colors.White;


            Activefill.Color = Color.FromArgb(50, 0, 190, 0);
            Activefill.Opacity = 5;

            ActivefillSelected.Color = Color.FromArgb(255, 0, 190, 0);

            ActivefillSelectedBox.Color = Color.FromArgb(100, 255, 255, 255);



            TargetedBorder.Color = Color.FromArgb(80, 255, 178, 178);

            Untargeted.Color = Color.FromArgb(0, 0, 0, 0);

            Attack1Hitfill.Color = Color.FromArgb(80, 116, 0, 129);

            Attack2Hitfill.Color = Color.FromArgb(77, 1, 139, 0);

            Attack3Hitfill.Color = Color.FromArgb(77, 141, 95, 0);

            Attack4Hitfill.Color = Color.FromArgb(77, 161, 184, 14);

            OwnLand = new SolidColorBrush(Color.FromRgb(90, 105, 236));

            Elseland = new SolidColorBrush(Color.FromRgb(215, 94, 67));

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

        private void setup()
        {
            Coloursetup();
            Timersetup();
           
        }
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


        //methods to handle tutorial parts
        public void Part1()
        {
            this.Dispatcher.Invoke(() =>
            {
                TextBlock messegeout = new TextBlock();
                messegeout.TextWrapping = TextWrapping.Wrap;

                messagedisplay_box.Items.Clear();

                messegeout.Text = $" Welcome to the Bunker Tutorial" +
                $" This is Tutorial 2 of 3" +
                $" Hit the next button to continue";
                messagedisplay_box.Items.Insert(0, messegeout);

            });
        }

        public void Part2()
        {
            this.Dispatcher.Invoke(() =>
            {
                messagedisplay_box.Items.Clear();
                TextBlock messegeout = new TextBlock();
                messegeout.TextWrapping = TextWrapping.Wrap;


                messegeout.Text = $"This tutorial covers attack selection " +
                $" After you target a city you can select an attack" +
                $" Using what youve learnt, click on an enemy city to attack";
               
                messagedisplay_box.Items.Insert(0, messegeout);

            });
        }

        public void Part3()
        {
            this.Dispatcher.Invoke(() =>
            {
                messagedisplay_box.Items.Clear();
                TextBlock messegeout = new TextBlock();
                messegeout.TextWrapping = TextWrapping.Wrap;
                
                Attack_border.Opacity = 0;
                
                messegeout.Text = $" Good job!" +
                $" Now youve targeted a city " +
                $"" +
                $" See in the how next to the targeting list, see the attack box is now avalible" +
                $"" +
                $" this box allows you to select an attack, an attack can be slected if it is green. If it is red it is unable to be slected " +
                $"" +
                $"Select an active attack (the green box)";
                messagedisplay_box.Items.Insert(0, messegeout);


            });
        }

        public void Part4()
        {
            this.Dispatcher.Invoke(() =>
            {
                messagedisplay_box.Items.Clear();
                TextBlock messegeout = new TextBlock();
                messegeout.TextWrapping = TextWrapping.Wrap;

                Attack1_button.IsEnabled= false;
                Attack2_button.IsEnabled= false;
                Attack3_button.IsEnabled= false;
                Attack4_button.IsEnabled= false;
                targetingrec.Opacity = 0;

                messegeout.Text = $" Good job!" +
                $"  Now you know how to select an attack" +
                $" Click Finish to Continue";
                messagedisplay_box.Items.Insert(0, messegeout);

            });
        }

        // modified map click method to handle tutorial
        public void map_click(object sender, RoutedEventArgs e)
        {
            // get the name of the ellipse clicked, add to targeting list, unless allready in list then remove

            var trigger = (Ellipse)sender;

            var cityclicked = citylist.Find(i => i.name == trigger.Name);





            if (cityclicked.owner != Selfuser && tutorialpart == 2)
            {
                if (!Targeting_list_display.Items.Contains(trigger.Name))
                {
                    Targeting_list_display.Items.Insert(0, trigger.Name);
                    trigger.Stroke = TargetedBorder;

                    Button_Click();

                }


            }
            else if (cityclicked.owner != Selfuser)
            {

                if (Targeting_list_display.Items.Contains(trigger.Name))
                {
                    
                    Button_Click();

                }
                else if (tutorialpart != 2 && tutorialpart <3)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        messagedisplay_box.Items.Clear();
                        TextBlock messegeout = new TextBlock();
                        messegeout.TextWrapping = TextWrapping.Wrap;



                        messegeout.Text = $" Click when instructed";
                        messagedisplay_box.Items.Insert(0, messegeout);

                    });
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        
                    });
                }


            }






        }

        // methods to handle button clicks
        public void Finsh()
        {
            Window win = new Tutorialpart3();
            win.Show();
            this.Close();
        }
        void Button_Click(object sender, RoutedEventArgs e)
        {

            Button trigger = sender as Button;

            if (trigger.Name == "nextbutton")
            {
                tutorialpart = tutorialpart + 1;
            }
            if (trigger.Name == "Attack1_button")
            {
                tutorialpart = tutorialpart + 1;
            }
            if (trigger.Name == "lastbutton")
            {
                tutorialpart = tutorialpart - 1;
                nextbutton.Content = "Next";

            }

            switch (tutorialpart)
            {
                case 0:
                    Part1();
                    break;

                case 1:
                    Part1();
                    break;


                case 2:
                    Part2();
                    break;


                case 3:
                    Part3();
                    break;

                case 4:
                    Part4();
                    nextbutton.Content = "Finish";
                    break;

                case 5:
                    tutorialpart = tutorialpart - 1;
                    Finsh();
                    break;

            }
        }

        void Button_Click()
        {
            if (tutorialpart < 3)
            {
                tutorialpart = tutorialpart + 1;
            }
                
            
            

            switch (tutorialpart)
            {
                case 0:
                    Part1();
                    break;

                case 1:
                    Part1();
                    break;


                case 2:
                    Part2();
                    break;


                case 3:
                    Part3();
                    break;

                case 4:
                    nextbutton.Content = "Finish";
                    Part4();
                    break;

                case 5:
                    Finsh();
                    tutorialpart = tutorialpart - 1;
                    break;

            }



        }

        

        private void Attack2_button_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                messagedisplay_box.Items.Clear();
                TextBlock messegeout = new TextBlock();
                messegeout.TextWrapping = TextWrapping.Wrap;



                messegeout.Text = $" please click a green box";
                messagedisplay_box.Items.Insert(0, messegeout);

            });
        }
    }
}
