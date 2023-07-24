using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
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
    /// Interaction logic for Tutorialpart1.xaml
    /// </summary>
    public partial class Tutorialpart1 : Window
    {
        // tutorial part variable
        int tutorialpart = 1;
        
        // city class
        public class city
        {
            public string name;
            public int x;
            public int y;
            public string owner;
        }
        // variables to handle username and colors
        
        string Selfuser = "Team1";
        SolidColorBrush OwnLand = new SolidColorBrush();
        SolidColorBrush Elseland = new SolidColorBrush();
        SolidColorBrush TargetedBorder = new SolidColorBrush();
        SolidColorBrush Untargeted = new SolidColorBrush();

        //initalise city list
        List<city> citylist = new List<city>();

        // main line code
        public Tutorialpart1()
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

            Part1();





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
                $" This is Tutorial 1 of 3" +
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


                messegeout.Text = $" To the left of here is the map" +
                $" you can see that your land mass is coloured in blue" +
                $" and you enemies in red" +
                $" Try clicking on one of your enemies cites, they are the Black dots within the red landmass";
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

                targetingrec.Opacity = 0;

                messegeout.Text = $" Good job!" +
                $" Since you clicked on a city, you can see there is a targeting ring around it" +
                $"" +
                $" As well as this its name appears in the targeting list in the lower left corner" +
                $"" +
                $" Clicking the city again will remove it from the targeting list" +
                $"" +
                $" Try clicking the city that you targeted again";
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

                targetingrec.Opacity = 0;

                messegeout.Text = $" Good job!" +
                $"  Now you know how to target an ememy city" +
                $" Hit finish to continue";
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
                    Targeting_list_display.Items.Remove(trigger.Name);
                    trigger.Stroke = Untargeted;
                    Button_Click();

                }
                else if( tutorialpart !=2 && tutorialpart !=3 && tutorialpart <4)
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
                else if(tutorialpart < 4)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        messagedisplay_box.Items.Clear();
                        TextBlock messegeout = new TextBlock();
                        messegeout.TextWrapping = TextWrapping.Wrap;



                        messegeout.Text = $" Please click the correct city, its the one highlighted by the targeting ring";
                        messagedisplay_box.Items.Insert(0, messegeout);

                    });
                }


            }

            

            


        }

        // methods to handle button clicks
         void Finsh()
        {
            Window win = new Tutorialpart2();

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
            if ( tutorialpart < 4)
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

    }

   
}
