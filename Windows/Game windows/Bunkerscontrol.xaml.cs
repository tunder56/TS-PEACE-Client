
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Xsl;


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
        public SolidColorBrush Disabledfill = Brushes.Red;
        public SolidColorBrush Disabledfilltext = Brushes.Red;
        public SolidColorBrush Activefill = Brushes.Green;
        public SolidColorBrush ActivefillSelected = Brushes.YellowGreen;
        public SolidColorBrush ActivefillSelectedBox = Brushes.YellowGreen;
        public SolidColorBrush TargetedBorder = Brushes.LightPink;
        public SolidColorBrush Untargeted = Brushes.Black;
        public SolidColorBrush Attack1Hitfill = Brushes.Turquoise;
        public SolidColorBrush Attack2Hitfill = Brushes.PapayaWhip;
        public SolidColorBrush Attack3Hitfill = Brushes.Cornsilk;
        public SolidColorBrush Attack4Hitfill = Brushes.BlanchedAlmond;
        public SolidColorBrush OwnLand = Brushes.Blue;
        public SolidColorBrush Elseland = Brushes.Violet;



        // status variables
        bool Attack1_status = false;
        bool Attack2_status = false;
        bool Attack3_status = false;
        bool Attack4_status = false;

        // localise HubConnection class
        Microsoft.AspNetCore.SignalR.Client.HubConnection connection;

        // targeting list variable

        List<string> targeting = new List<string>();


        // string variables
        string Selfuser = "testuser";
        string pastattack = null;
        string currentattack = null;

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
            setup();
            Timertick1();
            Timertick2();
            Timertick3();
            Timertick4();

            // connect to Signal R hub
            connection = new HubConnectionBuilder()
                .WithUrl(url: "https://21ftszzr-7190.aue.devtunnels.ms/Bunkershub")
                .Build();



            connection.On<string, string>(methodName: "reciveMessage", (user, message) => reciveMessage(user, message));
            connection.On<List<string>, string, string>(methodName: "incommingattack", (incomAttack, Attacker, Method) => incommingattack(incomAttack, Attacker, Method));
            connection.StartAsync();


        }

        private void setup()
        {
            Timersetup();
            setuparray();
            Setuptooltips();
        }

        public void setuparray()
        {
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
        }

        public void Setuptooltips()
        {
            IEnumerable<Ellipse> Circle = map.Children.OfType<Ellipse>();
            foreach (var city in Circle)
            {

                TextBlock text = new TextBlock();

                text.FontFamily = new FontFamily("Avara");

                text.FontSize = 18;

                char[] chars = city.Name.ToCharArray();



                text.HorizontalAlignment = HorizontalAlignment.Center;

                text.TextWrapping = TextWrapping.Wrap;

                text.Text = city.Name;

                ToolTip tootip = new ToolTip();

                tootip.Content = text;

                ToolTipService.SetInitialShowDelay(tootip, 2);

                ToolTipService.SetToolTip(city, tootip);

            }

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


        private async void Send_attack(object sender, RoutedEventArgs e)
        {
            if (currentattack == "attack1" && Attack1_status == true)
            {
                List<string> sendingattack = new List<string>();

                foreach (string cityname in Targeting_list_display.Items)
                {
                    sendingattack.Add(cityname);
                }


                try
                {
                    await connection.InvokeAsync(methodName: "AttackMessage", currentattack, sendingattack, Selfuser);

                }
                catch (Exception ex)
                {
                    messagedisplay_box.Items.Insert(0, ex.Message);
                }


                sendingattack.Clear();
                Targeting_list_display.Items.Clear();
            }

            if (currentattack == "attack2" && Attack2_status == true)
            {
                List<string> sendingattack = new List<string>();

                foreach (string cityname in Targeting_list_display.Items)
                {
                    sendingattack.Add(cityname);
                }


                try
                {
                    await connection.InvokeAsync(methodName: "AttackMessage", currentattack, sendingattack, Selfuser);

                }
                catch (Exception ex)
                {
                    messagedisplay_box.Items.Insert(0, ex.Message);
                }


                sendingattack.Clear();
                Targeting_list_display.Items.Clear();
            }

            if (currentattack == "attack3" && Attack3_status == true)
            {
                List<string> sendingattack = new List<string>();

                foreach (string cityname in Targeting_list_display.Items)
                {
                    sendingattack.Add(cityname);
                }


                try
                {
                    await connection.InvokeAsync(methodName: "AttackMessage", currentattack, sendingattack, Selfuser);

                }
                catch (Exception ex)
                {
                    messagedisplay_box.Items.Insert(0, ex.Message);
                }


                sendingattack.Clear();
                Targeting_list_display.Items.Clear();
            }

            if (currentattack == "attack4" && Attack4_status == true)
            {
                List<string> sendingattack = new List<string>();

                foreach (string cityname in Targeting_list_display.Items)
                {
                    sendingattack.Add(cityname);
                }


                try
                {
                    await connection.InvokeAsync(methodName: "AttackMessage", currentattack, sendingattack, Selfuser);

                }
                catch (Exception ex)
                {
                    messagedisplay_box.Items.Insert(0, ex.Message);
                }


                sendingattack.Clear();
                Targeting_list_display.Items.Clear();
            }



        }

        private void AttackMode_button_Click(object sender, RoutedEventArgs e)
        {
            // get attack slected , remove previous attack unless previous attack and selected attack are the same


            pastattack = currentattack;
            currentattack = null;

            var trigger = (Button)sender;
            if (trigger.Name == "Attack1_button" && Attack1_status == true)
            {
                Attack1_box.Fill = ActivefillSelectedBox;
                Attack1_name.Foreground = ActivefillSelected;
                Attack1_timer.Foreground = ActivefillSelected;
                currentattack = "attack1";


            }
            if (pastattack == "attack1" && Attack1_status == true)
            {
                Attack1_box.Fill = Activefill;
                Attack1_name.Foreground = Disabledfilltext;
                Attack1_timer.Foreground = Disabledfilltext;

            }
            if (trigger.Name == "Attack2_button" && Attack2_status == true)
            {
                Attack2_box.Fill = ActivefillSelectedBox;
                Attack2_name.Foreground = ActivefillSelected;
                Attack2_timer.Foreground = ActivefillSelected;
                currentattack = "attack2";

            }
            if (pastattack == "attack2" && Attack2_status == true)
            {
                Attack2_box.Fill = Activefill;
                Attack2_name.Foreground = Disabledfilltext;
                Attack2_timer.Foreground = Disabledfilltext;
            }
            if (trigger.Name == "Attack3_button" && Attack3_status == true)
            {
                Attack3_box.Fill = ActivefillSelectedBox;
                Attack3_name.Foreground = ActivefillSelected;
                Attack3_timer.Foreground = ActivefillSelected;
                currentattack = "attack3";

            }
            if (pastattack == "attack3" && Attack3_status == true)
            {
                Attack3_box.Fill = Activefill;
                Attack3_name.Foreground = Disabledfilltext;
                Attack3_timer.Foreground = Disabledfilltext;
            }
            if (trigger.Name == "Attack4_button" && Attack4_status == true)
            {
                Attack4_box.Fill = ActivefillSelectedBox;
                Attack4_name.Foreground = ActivefillSelected;
                Attack4_timer.Foreground = ActivefillSelected;
                currentattack = "attack4";

            }
            if (pastattack == "attack4" && Attack4_status == true)
            {
                Attack4_box.Fill = Activefill;
                Attack4_name.Foreground = Disabledfilltext;
                Attack4_timer.Foreground = Disabledfilltext;
            }
            if (pastattack == currentattack)
            {
                currentattack = null;
            }

        }

        private async void messagesend_button_Click(object sender, RoutedEventArgs e)
        {
            // check if setting team, gather messeage and send to hub

            if (message_textbox.Text == "Team:Team1")
            {
                Selfuser = "Team1";

                path151.Stroke = Elseland;
                path137.Stroke = Elseland;
                path141.Stroke = Elseland;



                path139.Stroke = OwnLand;
                path143.Stroke = OwnLand;
                path147.Stroke = OwnLand;
                path145.Stroke = OwnLand;
                message_textbox.Text = "team1 has been set";
                try
                {
                    await connection.InvokeAsync(methodName: "SendMessage", Selfuser, message_textbox.Text);
                    message_textbox.Clear();
                }
                catch (Exception ex)
                {
                    messagedisplay_box.Items.Insert(0, ex.Message);
                }
                return;
            }
            if (message_textbox.Text == "Team:Team2")
            {
                Selfuser = "Team2";

                // colour own contries blue
                path151.Stroke = OwnLand;
                path137.Stroke = OwnLand;
                path141.Stroke = OwnLand;

                // color others red

                path139.Stroke = Elseland;
                path143.Stroke = Elseland;
                path147.Stroke = Elseland;
                path145.Stroke = Elseland;


                message_textbox.Text = "team2 has been set";
                try
                {
                    await connection.InvokeAsync(methodName: "SendMessage", Selfuser, message_textbox.Text);
                    message_textbox.Clear();
                }
                catch (Exception ex)
                {
                    messagedisplay_box.Items.Insert(0, ex.Message);
                }
                return;
            }
            try
            {
                await connection.InvokeAsync(methodName: "SendMessage", Selfuser, message_textbox.Text);
                message_textbox.Clear();
            }
            catch (Exception ex)
            {
                messagedisplay_box.Items.Insert(0, ex.Message);
            }

            message_textbox.Clear();

        }

        // Recival Methods
        private async void reciveMessage(string user, string message)
        {
            // recive message, create new object, apply colour and wrap, send message to display

            this.Dispatcher.Invoke(() =>
            {
                TextBlock messegeout = new TextBlock();
                messegeout.TextWrapping = TextWrapping.Wrap;

                if (user == Selfuser)
                {
                    messegeout.Foreground = OwnLand;

                }
                else
                {
                    messegeout.Foreground = Elseland;
                }
                messegeout.Text = $"{user} says {message}";
                messagedisplay_box.Items.Insert(0, messegeout);

            });

        }

        private async void incommingattack(List<string> incomAttack, string Attacker, string Method)
        {
            // find attack method, unpack each city from list, apply hit fill accourding to attack , create and display message to attack feed


            if (Method == "attack1")
            {
                int attaknum = 1;
                foreach (string city in incomAttack)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var hitting = (Ellipse)FindName(city);
                        hitting.Fill = Attack1Hitfill;
                    });



                    this.Dispatcher.Invoke(() =>
                    {
                        TextBlock toinsert = new TextBlock();
                        if (Attacker == Selfuser)
                        {
                            toinsert.Foreground = OwnLand;
                        }
                        else
                        {
                            toinsert.Foreground = Elseland;
                        }
                        toinsert.TextWrapping = TextWrapping.Wrap;
                        toinsert.text = $"{Attacker} has hit {city} with {Method}";
                        stikefeeddisplay_box.Items.Insert(0, toinsert);
                        attaknum++;
                    });

                }
            }
            if (Method == "attack2")
            {
                int attaknum = 200;
                foreach (string city in incomAttack)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var hitting = (Ellipse)FindName(city);
                        hitting.Fill = Attack2Hitfill;
                    });



                    this.Dispatcher.Invoke(() =>
                    {
                        TextBlock toinsert = new TextBlock();
                        if (Attacker == Selfuser)
                        {
                            toinsert.Foreground = OwnLand;
                        }
                        else
                        {
                            toinsert.Foreground = Elseland;
                        }
                        toinsert.TextWrapping = TextWrapping.Wrap;
                        toinsert.Text = $"{Attacker} has hit {city} with {Method}";
                        stikefeeddisplay_box.Items.Insert(0, toinsert);
                        attaknum++;
                    });

                }
            }
            if (Method == "attack3")
            {
                int attaknum = 300;
                foreach (string city in incomAttack)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var hitting = (Ellipse)FindName(city);
                        hitting.Fill = Attack3Hitfill;
                    });



                    this.Dispatcher.Invoke(() =>
                    {
                        TextBlock toinsert = new TextBlock();
                        if (Attacker == Selfuser)
                        {
                            toinsert.Foreground = OwnLand;
                        }
                        else
                        {
                            toinsert.Foreground = Elseland;
                        }
                        
                        toinsert.TextWrapping = TextWrapping.Wrap;
                       

                        toinsert.Text = $"{Attacker} has hit {city} with {Method}";
                        stikefeeddisplay_box.Items.Insert(0, toinsert);
                        attaknum++;
                    });

                }
            }
            if (Method == "attack4")
            {
                int attaknum = 400;
                foreach (string city in incomAttack)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var hitting = (Ellipse)FindName(city);
                        hitting.Fill = Attack4Hitfill;
                    });



                    this.Dispatcher.Invoke(() =>
                    {
                        TextBlock toinsert = new TextBlock();
                        if (Attacker == Selfuser)
                        {
                            toinsert.Foreground = OwnLand;
                        }
                        else
                        {
                            toinsert.Foreground = Elseland;
                        }
                        toinsert.TextWrapping = TextWrapping.Wrap;
                        toinsert.Text = $"{Attacker} has hit {city} with {Method}";
                        stikefeeddisplay_box.Items.Insert(0, toinsert);
                        attaknum++;
                    });

                }
            }
            if (Attacker == Selfuser)
            {
                if (currentattack == "attack1")
                {
                    Random rnd = new Random();
                    Attak1_timerVar = Attak1_timerVar + new TimeSpan(0, rnd.Next(2, 3), 0);
                }
                if (currentattack == "attack2")
                {
                    Random rnd = new Random();
                    Attak2_timerVar = Attak2_timerVar + new TimeSpan(0, rnd.Next(2, 3), 0);
                }
                if (currentattack == "attack3")
                {
                    Random rnd = new Random();
                    Attak3_timerVar = Attak3_timerVar + new TimeSpan(0, rnd.Next(3, 4), 0);
                }
                if (currentattack == "attack4")
                {
                    Random rnd = new Random();
                    Attak4_timerVar = Attak4_timerVar + new TimeSpan(0, rnd.Next(4, 5), 0);
                }

            }
            else
            {
                if (Method == "attack1" || Method == "attack2")
                {
                    Random rnd = new Random();
                    Attak1_timerVar = Attak1_timerVar + new TimeSpan(0, rnd.Next(2, 3), 0);
                    Attak2_timerVar = Attak2_timerVar + new TimeSpan(0, rnd.Next(2, 3), 0);

                }
                if (Method == "attack3")
                {
                    Random rnd = new Random();
                    Attak3_timerVar = Attak3_timerVar + new TimeSpan(0, rnd.Next(3, 4), 0);
                    Attak1_timerVar = Attak1_timerVar + new TimeSpan(0, rnd.Next(2, 3), 0);
                    Attak2_timerVar = Attak2_timerVar + new TimeSpan(0, rnd.Next(2, 3), 0);
                }
                if (Method == "attack4")
                {
                    Random rnd = new Random();
                    Attak4_timerVar = Attak4_timerVar + new TimeSpan(0, rnd.Next(4, 5), 0);
                    Attak3_timerVar = Attak3_timerVar + new TimeSpan(0, rnd.Next(3, 4), 0);
                    Attak1_timerVar = Attak1_timerVar + new TimeSpan(0, rnd.Next(2, 3), 0);
                    Attak2_timerVar = Attak2_timerVar + new TimeSpan(0, rnd.Next(2, 3), 0);
                }

            }
            Timercheck();
        }

        //Message logic methods

        private void entercheck(object sender, KeyEventArgs e)
        {
            // see if enter key has been hit when entering message into message box
            if (e.Key == Key.Enter)
            {
                messagesend_button_Click(sender, e);
            }
        }

        private void message_textbox_LostFocus(object sender, RoutedEventArgs e)
        {

            // apply new text to textbox if textbox is clicked off of

            TextBox textBox = sender as TextBox;
            if (textBox.Text == "") { textBox.Text = "Enter your text here..."; }



        }

        private void messegge_textbox_focus(object sender, RoutedEventArgs e)
        {
            // remove placeholder text from textbox

            TextBox textBox = sender as TextBox;
            if (textBox.Text == "Enter your text here...")
            {
                textBox.Text = "";
            }
        }
    }
}
