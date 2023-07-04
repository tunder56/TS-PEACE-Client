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
        public class city
        {
            public string name;
            public int x;
            public int y;
            public string owner;
        }
        List<city> citylist = new List<city>();
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

        TimeSpan Attak1_timerVar;
        TimeSpan Attak2_timerVar;
        TimeSpan Attak3_timerVar;
        TimeSpan Attak4_timerVar;
        public Tutorialpart2()
        {
            InitializeComponent();

            // attack selection 

            // once a city is targeted, it can be attacked.

            // using what you learned before, target an enemy city

            // if clicked own city
            // that is not a enemy city, try again

            // good job,

            // now see the attack selection boxes, they are to the right of the targeting list and below the map

            // In order for an attack to be selected, it needs to be alavlible, this is shown by the color of the box 

            // red == unavailbe
            // green == ready to use


            // select a Green / ready to use attack

            // if red attack selected
            // that attack is not avalibe , slect a green attack

            // good job , this is how you select an attack

            // to lanch an attack, click the attack button to the left of the attack selection buttons

            // lanch an attack now!!

            InitializeComponent();





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
            setup();
            Timertick1();
            Part1();





        }
        // map and targeting




        // if clicked on own landmass
        // no , that is one of your cites try again 

        // since you cliked on the city, there is a targeting ring around it, and its name appears in the targeted cites list

        // clicking the city again will remove the city from targeting, give it a go


        // that is how you target citys in the simulation
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

        private void Timercheck()
        {
            // check if timers are above zero and if they are active, if so make sure they tick

            if (Attak1_timerVar > new TimeSpan(00, 00, 00) && Attack1_status == true)
            {
                Timertick1();

            }
            if (Attak2_timerVar > new TimeSpan(00, 00, 00) && Attack2_status == true)
            {
                Timertick2();
            }
            if (Attak3_timerVar > new TimeSpan(00, 00, 00) && Attack3_status == true)
            {
                Timertick3();
            }
            if (Attak4_timerVar > new TimeSpan(00, 00, 00) && Attack4_status == true)
            {
                Timertick4();
            }
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
