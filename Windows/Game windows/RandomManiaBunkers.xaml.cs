using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RandomNameGeneratorLibrary;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace TS_PEACE_Client.Windows.Game_windows
{
    /// <summary>
    /// Interaction logic for RandomManiaBunkers.xaml
    /// </summary>
    public partial class RandomManiaBunkers : Window
    {
        // Timer variables
        TimeSpan Attak1_timerVar;
        TimeSpan Attak2_timerVar;
        TimeSpan Attak3_timerVar;
        TimeSpan Attak4_timerVar;

        // colour variables
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


        // status variables
        bool Attack1_status = false;
        bool Attack2_status = false;
        bool Attack3_status = false;
        bool Attack4_status = false;


        // hashset for excluding hexagons


        HashSet<int> exclude = new HashSet<int>();

        // hexagon variables
        int total = 0;
        public int gridWidth = 32;  // Number of hexagons per row
        public int gridHeight = 64;
        bool[] offsetrow = new bool[64];
        
        // lists for storing hexagonids according to who they belong to
        List<int> team1landmass = new List<int>();
        List<int> team2landmass = new List<int>();




        // string variables
        string Selfuser = "testuser";
        string pastattack = null;
        string currentattack = null;

        // Create custom routed event
        public static readonly RoutedEvent Clickteam = EventManager.RegisterRoutedEvent(
        name: "Click2",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(Button));

        int landmasize = 300;


        // localise HubConnection class
        HubConnection connection;


        // json classes to handles packing and reciving map data
        [JsonObject]
        public class city
        {
            [JsonProperty("name")] public string name;
            [JsonProperty("x")] public int x;
            [JsonProperty("y")] public int y;
            [JsonProperty("owner")] public string owner;
            [JsonProperty("ID")] public int ID;
            [JsonProperty("Uid")] public int Uid;
        }

        public class citytosend
        {
            [JsonProperty("name")] public string name;
            [JsonProperty("ID")] public int ID;
            [JsonProperty("owner")] public string owner;

        }

        // list of cities
        
        List<city> citylist = new List<city>();

        // variables for handeling city name generation

        string bannedlettersS = ".!@#$%^&*()+=-`~[]{}|;':,./<>?";
        public char[] bannedletters;

        // variable for race condition handeling
        public bool booted = false;

        // Mainline
        public RandomManiaBunkers()
        {



            // startup and setup
            InitializeComponent();
            setup();
            Timertick1();
            Timertick2();
            Timertick3();
            Timertick4();
            HexagonStart();
            bannedletters = bannedlettersS.ToCharArray();
            // run reboottimer in seperate thread
            Task.Run(Reboottimer);
            

            // pick team hexes 
            pickingTeam1points();
            pickingTeam2points();

            // set booted race condition variable to true
            booted = true;

            // setup final things after succcessful boot

            SetclickandName();
            Setuptooltips();
            setuparray();

            Attack1_name.Text = GlobalSettings.Default.Attack1name;
            Attack2_name.Text = GlobalSettings.Default.Attack2name;
            Attack3_name.Text = GlobalSettings.Default.Attack3name;
            Attack4_name.Text = GlobalSettings.Default.Attack4name;


            



            // connect to Signal R hub
            connection = new HubConnectionBuilder()
                .WithUrl(url: "https://serverv2playnice20220123124825.azurewebsites.net/Bunkershub")
                .WithAutomaticReconnect()
                .Build();

            connection.On<string, string>(methodName: "reciveMessage", (user, message) => reciveMessage(user, message));
            connection.On<List<string>, string, string>(methodName: "incommingattack", (incomAttack, Attacker, Method) => incommingattack(incomAttack, Attacker, Method));
            connection.On<string>(methodName: "Syncmap1", (Citysync) => Syncmap1(Citysync));
            connection.On<string>(methodName: "Syncmap2", (Citysync) => Syncmap2(Citysync));
            connection.On<string>(methodName: "Syncmap3", (Citysync) => Syncmap3(Citysync));
            connection.On<string>(methodName: "Syncmap4", (Citysync) => Syncmap4(Citysync));
            connection.On(methodName: "Syncrequest", () => Syncrequest());
            connection.On(methodName: "Loneclient", () => LoneClient());
            



            connection.Reconnecting += error =>
            {
                Debug.Assert(connection.State == HubConnectionState.Reconnecting);
                return Task.CompletedTask;
            };
            connection.Reconnected += connectionId =>
            {
                Debug.Assert(connection.State == HubConnectionState.Connected);
                return Task.CompletedTask;
            };
            // check other uses connected to server
            connectionsolidify();
        }


        //signal R Methods
        public async ValueTask DisposeAsync()
        {
            if (connection is not null)
            {
                await connection.DisposeAsync();
            }
        }


        // Set up methods
        private void setup()
        {
            Coloursetup();
            Timersetup();
            Screenstatesetup();


        }

        public async Task Reboottimer()
        {
            // create timer, if timer runs out, and booted = false, reboot
            TimeSpan boottimer = new TimeSpan(0, 0, 0, 5);
            while (boottimer.TotalSeconds > 0)
            {
                boottimer = boottimer.Subtract(new TimeSpan(0, 0, 0, 1));
                await Task.Delay(1000);
            }
            if (booted == false)
            {
                var s = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = System.IO.Path.GetDirectoryName(s);
                string strSettingsXmlFilePath = System.IO.Path.Combine(strWorkPath, "TS-PEACE-Client.exe");
                
                string messageBoxText = "Generating map took too long, Rebooting";
                string caption = "TS P.E.A.C.E CLient error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult result;

              
                
                if (MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start(strSettingsXmlFilePath);
                    Environment.Exit(1003);
                    return;
                }
                
            }
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



            TargetedBorder.Color = Color.FromArgb(100, 213, 213, 213);

            Untargeted.Color = Color.FromArgb(100, 59, 56, 57);
            
            Attack1Hitfill.Color = Color.FromArgb(GlobalSettings.Default.Attack1color.A, GlobalSettings.Default.Attack1color.R, GlobalSettings.Default.Attack1color.G, GlobalSettings.Default.Attack1color.B);
            Attack2Hitfill.Color = Color.FromArgb(GlobalSettings.Default.Attack2color.A, GlobalSettings.Default.Attack2color.R, GlobalSettings.Default.Attack2color.G, GlobalSettings.Default.Attack2color.B);
            Attack3Hitfill.Color = Color.FromArgb(GlobalSettings.Default.Attack3color.A, GlobalSettings.Default.Attack3color.R, GlobalSettings.Default.Attack3color.G, GlobalSettings.Default.Attack3color.B);
            Attack4Hitfill.Color = Color.FromArgb(GlobalSettings.Default.Attack4color.A, GlobalSettings.Default.Attack4color.R, GlobalSettings.Default.Attack4color.G, GlobalSettings.Default.Attack4color.B);

            OwnLand = new SolidColorBrush(Color.FromRgb(90, 105, 236));

            Elseland = new SolidColorBrush(Color.FromRgb(215, 94, 67));

        }

        private void Screenstatesetup()
        {
            // Apply Fills to elements

            Attack1_box.Fill = Disabledfill;
            Attack2_box.Fill = Disabledfill;
            Attack3_box.Fill = Disabledfill;
            Attack4_box.Fill = Disabledfill;
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

        public void setuparray()
        {
            // set up array for map
            IEnumerable<Polygon> Circle = map.Children.OfType<Polygon>();

            foreach (var c in Circle)
            {
                string ownerout = "none";
                Point point1 = c.Points[0];
                Point point2 = c.Points[3];

                int xave = (int)Canvas.GetLeft(c);
                int yave = (int)Canvas.GetTop(c);

                if (c.Uid == "2")
                {
                    ownerout = "Team2";
                    citylist.Add(new city { name = c.Name, x = xave, y = yave, owner = ownerout, ID = (int)c.Tag });

                }
                else if (c.Uid == "1")
                {
                    ownerout = "Team1";
                    citylist.Add(new city { name = c.Name, x = xave, y = yave, owner = ownerout, ID = (int)c.Tag });

                }
                else
                {
                    ownerout = "none";
                    citylist.Add(new city { name = c.Name, x = xave, y = yave, owner = ownerout, ID = (int)c.Tag });
                    c.Opacity = 0.5;
                }
            }
        }

        public void Setuptooltips()
        {
            // set up tooltips for cities
            IEnumerable<Polygon> Hexs = map.Children.OfType<Polygon>();
            foreach (var city in Hexs)
            {
                if (city.Name != "")
                {
                    TextBlock text = new TextBlock();

                    text.FontFamily = text.TryFindResource("AVB") as FontFamily;
                    text.FontSize = 18;
                    text.HorizontalAlignment = HorizontalAlignment.Center;
                    text.TextWrapping = TextWrapping.Wrap;
                    text.Text = city.Name;

                    ToolTip tootip = new ToolTip();

                    tootip.Content = text;

                    ToolTipService.SetInitialShowDelay(tootip, 2);

                    ToolTipService.SetToolTip(city, tootip);
                }


            }

        }

        // Timer logic methods

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

        // Sending Methods

        private async void Send_attack(object sender, RoutedEventArgs e)
        {
            // gather list of citys being targeted, packagage and send to hub
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

        private async void messagesend_button_Click(object sender, RoutedEventArgs e)
        {
            // check if setting team, gather messeage and send to hub

            if (message_textbox.Text == "Team:Team1")
            {
                Selfuser = "Team1";
                SetcolorTeam1();
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

                SetcolorTeam2();

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
            else
            {
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



        }

        private void SetcolorTeam1()
        {
            // set colors to team 1 color profile
            IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();

            foreach (var c in citylist)
            {
                if (c.owner == "Team1")
                {
                    hex.ElementAt(c.ID).Fill = OwnLand;
                    hex.ElementAt(c.ID).Opacity = 1;
                }
                else if (c.owner == "Team2")
                {
                    hex.ElementAt(c.ID).Fill = Elseland;
                    hex.ElementAt(c.ID).Opacity = 1;
                }
                if (c.owner == "none")
                {
                    hex.ElementAt(c.ID).Fill = Brushes.Black;
                    hex.ElementAt(c.ID).Opacity = 0.5;
                }
                if (c.owner == null)
                {
                    hex.ElementAt(c.ID).Fill = Brushes.Black;
                    hex.ElementAt(c.ID).Opacity = 0.5;
                }
            }
        }

        private void SetcolorTeam2()
        {
            // set colors to team 2 color profile
            IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();

            foreach (var c in citylist)
            {
                if (c.owner == "Team1")
                {
                    hex.ElementAt(c.ID).Fill = Elseland;
                    hex.ElementAt(c.ID).Opacity = 1;
                }
                else if (c.owner == "Team2")
                {
                    hex.ElementAt(c.ID).Fill = OwnLand;
                    hex.ElementAt(c.ID).Opacity = 1;

                }
                if (c.owner == "none")
                {
                    hex.ElementAt(c.ID).Fill = Brushes.Black;
                }
                if (c.owner == null)
                {
                    hex.ElementAt(c.ID).Fill = Brushes.Black;
                }
            }
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

            this.Dispatcher.Invoke(() =>
            {
                IEnumerable<Polygon> Hexs = map.Children.OfType<Polygon>();



                if (Method == "attack1")
                {
                    int attaknum = 1;
                    foreach (string city in incomAttack)
                    {




                        this.Dispatcher.Invoke(async () =>
                        {
                            TextBlock tb1 = new TextBlock();

                            TextBlock tb2 = new TextBlock();

                            TextBlock tb3 = new TextBlock();

                            TextBlock tb35 = new TextBlock();

                            TextBlock tb4 = new TextBlock();


                            tb1.Text = $"{Attacker} Has Hit ";
                            tb2.Text = $"{city} ";
                            tb3.Text = $"with ";
                            tb35.Text = $"{Attack1_name.Text}";

                            tb35.Foreground = Attack1Hitfill;

                            Label toinsert = new Label();
                            if (Attacker == Selfuser)
                            {
                                tb1.Foreground = OwnLand;
                                tb2.Foreground = Elseland;
                                tb3.Foreground = OwnLand;

                            }
                            else
                            {
                                tb1.Foreground = Elseland;
                                tb2.Foreground = OwnLand;
                                tb3.Foreground = Elseland;

                            }
                            tb4.Inlines.Add(tb1);
                            tb4.Inlines.Add(tb2);
                            tb4.Inlines.Add(tb3);
                            tb4.Inlines.Add(tb35);
                            tb4.TextWrapping = TextWrapping.Wrap;

                            curvedanimatev3(Attacker, city, attaknum, Method);

                            attaknum++;


                        });

                    }
                }
                if (Method == "attack2")
                {
                    int attaknum = 1;
                    foreach (string city in incomAttack)
                    {




                        this.Dispatcher.Invoke(() =>
                        {
                            TextBlock tb1 = new TextBlock();

                            TextBlock tb2 = new TextBlock();

                            TextBlock tb3 = new TextBlock();
                            TextBlock tb35 = new TextBlock();

                            TextBlock tb4 = new TextBlock();


                            tb1.Text = $"{Attacker} Has Hit ";
                            tb2.Text = $"{city} ";
                            tb3.Text = $"with ";
                            tb35.Text = $"{Attack2_name.Text}";

                            tb35.Foreground = Attack2Hitfill;

                            Label toinsert = new Label();
                            if (Attacker == Selfuser)
                            {
                                tb1.Foreground = OwnLand;
                                tb2.Foreground = Elseland;
                                tb3.Foreground = OwnLand;

                            }
                            else
                            {
                                tb1.Foreground = Elseland;
                                tb2.Foreground = OwnLand;
                                tb3.Foreground = Elseland;

                            }
                            tb4.Inlines.Add(tb1);
                            tb4.Inlines.Add(tb2);
                            tb4.Inlines.Add(tb3);
                            tb4.Inlines.Add(tb35);
                            tb4.TextWrapping = TextWrapping.Wrap;

                            curvedanimatev3(Attacker, city, attaknum, Method);
                            attaknum++;


                        });

                    }
                }
                if (Method == "attack3")
                {
                    int attaknum = 1;
                    foreach (string city in incomAttack)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            TextBlock tb1 = new TextBlock();

                            TextBlock tb2 = new TextBlock();

                            TextBlock tb3 = new TextBlock();
                            TextBlock tb35 = new TextBlock();

                            TextBlock tb4 = new TextBlock();


                            tb1.Text = $"{Attacker} Has Hit ";
                            tb2.Text = $"{city} ";
                            tb3.Text = $"with ";
                            tb35.Text = $"{Attack3_name.Text}";

                            tb35.Foreground = Attack3Hitfill;


                            Label toinsert = new Label();
                            if (Attacker == Selfuser)
                            {
                                tb1.Foreground = OwnLand;
                                tb2.Foreground = Elseland;
                                tb3.Foreground = OwnLand;

                            }
                            else
                            {
                                tb1.Foreground = Elseland;
                                tb2.Foreground = OwnLand;
                                tb3.Foreground = Elseland;

                            }
                            tb4.Inlines.Add(tb1);
                            tb4.Inlines.Add(tb2);
                            tb4.Inlines.Add(tb3);
                            tb4.Inlines.Add(tb35);
                            tb4.TextWrapping = TextWrapping.Wrap;

                            curvedanimatev3(Attacker, city, attaknum, Method);
                            attaknum++;
                        });

                    }
                }
                if (Method == "attack4")
                {
                    int attaknum = 1;
                    foreach (string city in incomAttack)
                    {




                        this.Dispatcher.Invoke(() =>
                        {
                            TextBlock tb1 = new TextBlock();

                            TextBlock tb2 = new TextBlock();

                            TextBlock tb3 = new TextBlock();
                            TextBlock tb35 = new TextBlock();

                            TextBlock tb4 = new TextBlock();


                            tb1.Text = $"{Attacker} Has Hit ";
                            tb2.Text = $"{city} ";
                            tb3.Text = $"with ";
                            tb35.Text = $"{Attack4_name.Text}";

                            tb35.Foreground = Attack4Hitfill;

                            Label toinsert = new Label();
                            if (Attacker == Selfuser)
                            {
                                tb1.Foreground = OwnLand;
                                tb2.Foreground = Elseland;
                                tb3.Foreground = OwnLand;

                            }
                            else
                            {
                                tb1.Foreground = Elseland;
                                tb2.Foreground = OwnLand;
                                tb3.Foreground = Elseland;

                            }
                            tb4.Inlines.Add(tb1);
                            tb4.Inlines.Add(tb2);
                            tb4.Inlines.Add(tb3);
                            tb4.Inlines.Add(tb35);
                            tb4.TextWrapping = TextWrapping.Wrap;

                            curvedanimatev3(Attacker, city, attaknum, Method);
                            attaknum++;
                        });

                    }
                }

            });


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


        // Clicking / targeting Methods

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
        private void map_click(object sender, RoutedEventArgs e)
        {
            // get the name of the ellipse clicked, add to targeting list, unless allready in list then remove

            var trigger = (Polygon)sender;

            var cityclicked = citylist.Find(i => i.name == trigger.Name);

            if (cityclicked.owner != Selfuser )
            {
                if (!Targeting_list_display.Items.Contains(trigger.Name) && Targeting_list_display.Items.Count < 10)
                {
                    Targeting_list_display.Items.Insert(0, trigger.Name);
                    Panel.SetZIndex(trigger, 21);
                    trigger.Stroke = TargetedBorder;

                }
                else
                {
                    Targeting_list_display.Items.Remove(trigger.Name);
                    Panel.SetZIndex(trigger, 7);
                    trigger.Stroke = Untargeted;
                }
            }
        }

        private void TeamSet_Button1_Click(object sender, RoutedEventArgs e)
        {
            // apply team 1 to user state when clicked and remove slection screen

            teamSet_background.Opacity = 0;
            TeamSet_Button1.Opacity = 0;
            TeamSet_Button2.Opacity = 0;
            TeamSet_Label.Opacity = 0;
            TeamSet_Button1.IsEnabled = false;
            TeamSet_Button2.IsEnabled = false;
            message_textbox.Text = "Team:Team1";

            Panel.SetZIndex(teamSet_background, 0);
            Panel.SetZIndex(TeamSet_Button1, 0);
            Panel.SetZIndex(TeamSet_Button2, 0);
            Panel.SetZIndex(TeamSet_Label, 0);


            RoutedEventArgs routedEventArgs = new(routedEvent: Clickteam);

            RaiseEvent(routedEventArgs);
            messagesend_button_Click(messagesend_button, routedEventArgs);
        }

        private void TeamSet_Button2_Click(object sender, RoutedEventArgs e)
        {
            // apply team 2 to user state when clicked and remove slection screen

            teamSet_background.Opacity = 0;
            TeamSet_Button1.Opacity = 0;
            TeamSet_Button2.Opacity = 0;
            TeamSet_Label.Opacity = 0;
            TeamSet_Button1.IsEnabled = false;
            TeamSet_Button2.IsEnabled = false;
            message_textbox.Text = "Team:Team2";

            Panel.SetZIndex(teamSet_background, 0);
            Panel.SetZIndex(TeamSet_Button1, 0);
            Panel.SetZIndex(TeamSet_Button2, 0);
            Panel.SetZIndex(TeamSet_Label, 0);

            RoutedEventArgs routedEventArgs = new(routedEvent: Clickteam);

            RaiseEvent(routedEventArgs);
            messagesend_button_Click(messagesend_button, routedEventArgs);
        }


        // animation methods

        private void curvedanimatev3(string attacker, string hitcityin, int attacknum, string Method)
        {
            // get the citys involved in the attack
            // get the polygon objects for the citys
            // get the x and y coordinates for the citys
            // Create and animate a line between the citys
            IEnumerable<Polygon> Hexs = map.Children.OfType<Polygon>();

            List<city> randomcity = new List<city>();
            randomcity.AddRange(citylist.Where(x => x.owner == attacker));

            Random random = new Random();
            city Hitcity = citylist.Find(x => x.name == hitcityin);

            var cityfound = randomcity[random.Next(randomcity.Count)];

            bool movingLtoRight = false;
            bool movingUtoDown = false;
            bool movingDtoUP = false;
            bool movingRtoLeft = false;

            double x1 = Canvas.GetLeft(Hexs.ElementAt(cityfound.ID));
            double y1 = Canvas.GetTop(Hexs.ElementAt(cityfound.ID));
            double x2 = Canvas.GetLeft(Hexs.ElementAt(Hitcity.ID));
            double y2 = Canvas.GetTop(Hexs.ElementAt(Hitcity.ID));

            double xmod = 0;
            double ymod = 0;
            double boxcorrection = 20;
            double xdif = Math.Abs(x1 - x2);
            double ydif = Math.Abs(y1 - y2);
            double midpointx = (x1 + x2) / 2;
            double midpointy = (y1 + y2) / 2;

            RectangleGeometry Clippingmask = new RectangleGeometry();
            Clippingmask.Rect = new Rect(0, 0, 150, 160);


            DropShadowEffect dropShadow = new DropShadowEffect();
            dropShadow.Color = Color.FromRgb(100, 100, 100);
            dropShadow.BlurRadius = 2;

            Rectangle rectangle = new Rectangle();
            rectangle.Width = 50;
            rectangle.Height = 50;
            rectangle.Stroke = Brushes.Blue;

            Path path = new Path();
            path.Stroke = Brushes.Red;
            path.StrokeDashArray = new DoubleCollection(new double[] { 2, 2 });
            path.StrokeThickness = 3;
            path.Effect = dropShadow;

            Path path2 = new Path();
            path2.Stroke = Brushes.Blue;
            path2.StrokeThickness = 4;
            path.StrokeDashCap = PenLineCap.Round;
            path.StrokeEndLineCap = PenLineCap.Round;

            if (attacker == Selfuser)
            {
                path.Stroke = OwnLand;
            }
            else
            {
                path.Stroke = Elseland;
            }

            PathGeometry pathGeometry = new PathGeometry();

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(x1 + 5, y1 + 5);

            PathGeometry Animationpath = new PathGeometry();

            PathFigure pathFigure2 = new PathFigure();


            if (xdif > ydif)
            {
                if (x1 > x2)
                {
                    movingRtoLeft = true;
                }
                else if (x2 > x1)
                {
                    movingLtoRight = true;
                }


                double ymidpointinsurt = 0;
                if (midpointy - 100 < 0)
                {
                    ymidpointinsurt = 20;

                }
                else
                {
                    ymidpointinsurt = midpointy - 100;
                }


                if (movingLtoRight)
                {
                    pathFigure.Segments.Add(new QuadraticBezierSegment(new Point(midpointx, ymidpointinsurt), new Point(x2, y2), true));
                    pathFigure2.StartPoint = new Point(x1, y1 - 60);
                    pathFigure2.Segments.Add(new LineSegment(new Point(x2, y2 - 60), true));
                }
                else if (movingRtoLeft)
                {
                    pathFigure.Segments.Add(new QuadraticBezierSegment(new Point(midpointx, ymidpointinsurt), new Point(x2, y2), true));
                    pathFigure2.StartPoint = new Point(x1, y1 + 10);
                    pathFigure2.Segments.Add(new LineSegment(new Point(x2, y2 + 10), true));
                }
                else
                {
                    pathFigure.Segments.Add(new QuadraticBezierSegment(new Point(midpointx, ymidpointinsurt), new Point(x2, y2), true));
                    pathFigure2.StartPoint = new Point(x1, y1 + 10);
                    pathFigure2.Segments.Add(new LineSegment(new Point(x2, y2 + 10), true));
                }
            }
            else if (ydif > xdif)
            {
                double xmidpointinsurt = 0;
                if (midpointx - 100 < 0)
                {
                    xmidpointinsurt = 20;
                }
                else
                {
                    xmidpointinsurt = midpointx - 100;
                }

                if (y1 > y2)
                {
                    movingDtoUP = true;
                }

                if (y2 > y1)
                {
                    movingUtoDown = true;
                }


                if (movingUtoDown)
                {
                    pathFigure.Segments.Add(new QuadraticBezierSegment(new Point(xmidpointinsurt, midpointy), new Point(x2, y2), true));
                    pathFigure2.StartPoint = new Point(x1, y1);
                    pathFigure2.Segments.Add(new LineSegment(new Point(x2, y2), true));
                }
                else if (movingDtoUP)
                {
                    pathFigure.Segments.Add(new QuadraticBezierSegment(new Point(xmidpointinsurt, midpointy), new Point(x2, y2), true));
                    pathFigure2.StartPoint = new Point(x1 - 100, y1);
                    pathFigure2.Segments.Add(new LineSegment(new Point(x2 - 100, y2), true));
                }
                else
                {
                    pathFigure.Segments.Add(new QuadraticBezierSegment(new Point(xmidpointinsurt, midpointy), new Point(x2, y2), true));
                    pathFigure2.StartPoint = new Point(x1, y1);
                    pathFigure2.Segments.Add(new LineSegment(new Point(x2, y2), true));
                }

            }

            pathGeometry.Figures.Add(pathFigure);
            path.Data = pathGeometry;
            Animationpath.Figures.Add(pathFigure2);
            path2.Data = Animationpath;

            RotateTransform animatedRotateTransform = new RotateTransform();
            TranslateTransform animatedTranslateTransform = new TranslateTransform();

            if(attacker != Selfuser)
            {
                attacknum = attacknum + 1000;
            }

            string rotateTransformName = "AnimatedRotateTransform" + attacknum;
            string translateTransformname = "AnimatedTranslateTransform" + attacknum;
            string clippingmaskname = "cliipingmask" + attacknum;

            this.RegisterName(rotateTransformName, animatedRotateTransform);
            this.RegisterName(translateTransformname, animatedTranslateTransform);
            this.RegisterName(clippingmaskname, Clippingmask);


            TransformGroup tGroup = new TransformGroup();
            tGroup.Children.Add(animatedRotateTransform);
            tGroup.Children.Add(animatedTranslateTransform);

            Clippingmask.Transform = tGroup;

            rectangle.RenderTransform = tGroup;

            PathGeometry animationpath = Animationpath;


            DoubleAnimationUsingPath angleAnimation = new DoubleAnimationUsingPath();
            angleAnimation.PathGeometry = animationpath;
            angleAnimation.Duration = TimeSpan.FromSeconds(3);
            angleAnimation.Source = PathAnimationSource.Angle;

            Storyboard.SetTargetName(angleAnimation, rotateTransformName);
            Storyboard.SetTargetProperty(angleAnimation, new PropertyPath(RotateTransform.AngleProperty));


            DoubleAnimationUsingPath translateXAnimation = new DoubleAnimationUsingPath();
            translateXAnimation.PathGeometry = animationpath;
            translateXAnimation.Duration = TimeSpan.FromSeconds(3);
            translateXAnimation.Source = PathAnimationSource.X;


            Storyboard.SetTargetName(translateXAnimation, translateTransformname);
            Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath(TranslateTransform.XProperty));

            DoubleAnimationUsingPath translateYAnimation = new DoubleAnimationUsingPath();
            translateYAnimation.PathGeometry = animationpath;
            translateYAnimation.Duration = TimeSpan.FromSeconds(3);
            translateYAnimation.Source = PathAnimationSource.Y;

            Storyboard.SetTargetName(translateYAnimation, translateTransformname);
            Storyboard.SetTargetProperty(translateYAnimation, new PropertyPath(TranslateTransform.YProperty));

            Storyboard pathAnimationStoryboard = new Storyboard();

            RectAnimation rectAnimation = new RectAnimation();
            rectAnimation.From = new Rect(0, 0, 0, 160);
            rectAnimation.To = new Rect(0, 0, 150, 160);
            rectAnimation.Duration = TimeSpan.FromSeconds(2);

            RectAnimation closing = new RectAnimation();
            closing.From = new Rect(0, 0, 150, 160);
            closing.To = new Rect(0, 0, 0, 160);
            closing.Duration = TimeSpan.FromSeconds(5);

            Storyboard.SetTargetName(rectAnimation, clippingmaskname);
            Storyboard.SetTargetProperty(rectAnimation, new PropertyPath(RectangleGeometry.RectProperty));

            Storyboard.SetTargetName(closing, clippingmaskname);
            Storyboard.SetTargetProperty(closing, new PropertyPath(RectangleGeometry.RectProperty));

            pathAnimationStoryboard.Children.Add(angleAnimation);
            pathAnimationStoryboard.Children.Add(translateXAnimation);
            pathAnimationStoryboard.Children.Add(translateYAnimation);
            pathAnimationStoryboard.Children.Add(rectAnimation);

            Storyboard closingtime = new Storyboard();
            closingtime.Children.Add(closing);

            Panel.SetZIndex(path, 30);

            path.Clip = Clippingmask;
            map.Children.Add(path);

            path.Loaded += delegate (object sender, RoutedEventArgs e)
            {
                pathAnimationStoryboard.Begin(this);
            };

            IEnumerable<Polygon> Hexsonmap = map.Children.OfType<Polygon>();

            Polygon hexfound = Hexsonmap.First(x => x.Name == hitcityin);

            pathAnimationStoryboard.Completed += (s, e) =>
            {
                map.Children.Remove(path);
                closingtime.Begin(this);
                UnregisterName(rotateTransformName);
                UnregisterName(clippingmaskname);
                UnregisterName(translateTransformname);

                TextBlock tb1 = new TextBlock();

                TextBlock tb2 = new TextBlock();

                TextBlock tb3 = new TextBlock();
                TextBlock tb35 = new TextBlock();

                TextBlock tb4 = new TextBlock();

                string methodtext;

                if(Method =="attack1")
                {
                    tb35.Text = Attack1_name.Text;
                }
                else if (Method == "attack2")
                {
                    tb35.Text = Attack2_name.Text;
                }
                else if (Method == "attack3")
                {
                    tb35.Text = Attack3_name.Text;
                }
                else if (Method == "attack4")
                {
                    tb35.Text = Attack4_name.Text;
                }

                tb1.Text = $"{attacker} Has Hit ";
                tb2.Text = $"{hitcityin} ";
                tb3.Text = $"with {Method}";

                Label toinsert = new Label();

                if (attacker == Selfuser)
                {
                    tb1.Foreground = OwnLand;
                    tb2.Foreground = Elseland;
                    tb3.Foreground = OwnLand;

                }
                else
                {
                    tb1.Foreground = Elseland;
                    tb2.Foreground = OwnLand;
                    tb3.Foreground = Elseland;

                }
                tb4.Inlines.Add(tb1);
                tb4.Inlines.Add(tb2);
                tb4.Inlines.Add(tb3);
                tb4.TextWrapping = TextWrapping.Wrap;
                tb4.FontSize = 20;

                stikefeeddisplay_box.Items.Insert(0,tb4);
                

                if (Method == "attack1")
                {
                    hexfound.Fill = Attack1Hitfill;
                }
                else if (Method == "attack2")
                {
                    hexfound.Fill = Attack2Hitfill;
                }
                else if (Method == "attack3")
                {
                    hexfound.Fill = Attack3Hitfill;
                }
                else if (Method == "attack4")
                {
                    hexfound.Fill = Attack4Hitfill;
                }

                if (attacker == Selfuser)
                {
                    if (Method == "attack1")
                    {
                        Random rnd = new Random();
                        Attak1_timerVar = Attak1_timerVar + new TimeSpan(0, 0, rnd.Next(10, 15));
                    }
                    if (Method == "attack2")
                    {
                        Random rnd = new Random();
                        Attak2_timerVar = Attak2_timerVar + new TimeSpan(0, 0, rnd.Next(10, 20));
                    }
                    if (Method == "attack3")
                    {
                        Random rnd = new Random();
                        Attak3_timerVar = Attak3_timerVar + new TimeSpan(0, 0, rnd.Next(10, 40));
                    }
                    if (Method == "attack4")
                    {
                        Random rnd = new Random();
                        Attak4_timerVar = Attak4_timerVar + new TimeSpan(0, rnd.Next(1, 3), 0);
                    }
                }
                else
                {
                    if (Method == "attack1" || Method == "attack2")
                    {
                        Random rnd = new Random();
                        Attak1_timerVar = Attak1_timerVar + new TimeSpan(0, 0, rnd.Next(10, 15));
                        Attak2_timerVar = Attak2_timerVar + new TimeSpan(0, 0, rnd.Next(10, 20));

                    }
                    if (Method == "attack3")
                    {
                        Random rnd = new Random();
                        Attak3_timerVar = Attak3_timerVar + new TimeSpan(0, 0, rnd.Next(10, 40));
                        Attak1_timerVar = Attak1_timerVar + new TimeSpan(0, 0, rnd.Next(10, 15));
                        Attak2_timerVar = Attak2_timerVar + new TimeSpan(0, 0, rnd.Next(10, 20));
                    }
                    if (Method == "attack4")
                    {
                        Random rnd = new Random();
                        Attak4_timerVar = Attak4_timerVar + new TimeSpan(0, rnd.Next(1, 3), 0);
                        Attak3_timerVar = Attak3_timerVar + new TimeSpan(0, 0, rnd.Next(10, 40));
                        Attak1_timerVar = Attak1_timerVar + new TimeSpan(0, 0, rnd.Next(10, 15));
                        Attak2_timerVar = Attak2_timerVar + new TimeSpan(0, 0, rnd.Next(10, 20));
                    }

                }
                Timercheck();

            };

        }


        // Hexagon Methods

        private void HexagonStart()
        {
            // create a hexagon grid
            int gridWidth = 32;  // Number of hexagons per row
            int gridHeight = 64; // Number of hexagons per column

            double hexagonSize = 10; // Size of each hexagon
            double yOffset = hexagonSize * Math.Sqrt(0.75);
            double xOffset = hexagonSize * 3 / 2 * 2;

            for (int row = 0; row < gridHeight; row++)
            {
                if (row % 2 == 0)
                {
                    offsetrow[row] = false;
                }
                else
                {
                    offsetrow[row] = true;
                }

                for (int col = 0; col < gridWidth; col++)
                {
                    Random random = new Random();

                    var hexagon = CreateHexagon(hexagonSize);
                    double x = col * xOffset + ((row % 2 == 1) ? xOffset / 2 : 0);
                    double y = row * yOffset;

                    Canvas.SetLeft(hexagon, x + 15);
                    Canvas.SetTop(hexagon, y + 60);
                    Panel.SetZIndex(hexagon, 4);

                    hexagon.Fill = Brushes.Black;
                    hexagon.Stroke = Untargeted;
                    hexagon.StrokeThickness = 2;
                    hexagon.Tag = total;


                    if (total == 0)
                    {
                        exclude.Add(total);
                    }

                    if (total < gridWidth * 2)
                    {
                        exclude.Add(total);
                    }
                    else if (total % gridWidth == 0)
                    {
                        exclude.Add(total);
                    }
                    else if ((total + 1) % gridWidth == 0)
                    {
                        exclude.Add(total);
                    }
                    else if (total > ((gridWidth * gridHeight) - gridWidth * 2))
                    {
                        exclude.Add(total);
                    }

                    total = total + 1;

                    map.Children.Add(hexagon);
                }
            }
        }

        private Polygon CreateHexagon(double size)
        {
            // create a polygon in the shape of a hexagon
            var hexagon = new Polygon();
            hexagon.Stroke = Brushes.Black;
            hexagon.StrokeThickness = 2;

            double angle = Math.PI / 3;
            for (int i = 0; i < 6; i++)
            {
                double x = size * Math.Cos(i * angle);
                double y = size * Math.Sin(i * angle);
                hexagon.Points.Add(new Point(x, y));
            }
            return hexagon;
        }



        public async void pickingTeam2points()
        {
            // pick a random hexagon and propergate outwards to create a teamlandmass for team 2
            this.Dispatcher.Invoke(() =>
            {
                IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();
            
                Random random = new Random();

                var range = Enumerable.Range((gridWidth * 2) + 1, hex.Count()).Where(i => !exclude.Contains(i));

                int selecting = random.Next((gridWidth * 2) + 1, hex.Count() - exclude.Count);
                int row = CalculateRow(gridWidth, gridHeight, selecting);

                List<int> outerids = new List<int>();
                List<int> newouterids = new List<int>();

                if (row % 2 == 0)
                {


                    hex.ElementAt(selecting - gridWidth).Fill = Brushes.Blue;
                    hex.ElementAt(selecting - (gridWidth + 1)).Fill = Brushes.Blue;
                    hex.ElementAt(selecting + (gridWidth - 1)).Fill = Brushes.Blue;
                    hex.ElementAt(selecting + gridWidth).Fill = Brushes.Blue;

                    outerids.Add(selecting - gridWidth);
                    outerids.Add(selecting - (gridWidth + 1));
                    outerids.Add(selecting + (gridWidth - 1));
                    outerids.Add(selecting + gridWidth);

                    Panel.SetZIndex(hex.ElementAt(selecting - gridWidth), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting - (gridWidth + 1)), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting + (gridWidth - 1)), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting + gridWidth), 7);

                    team2landmass.Add(selecting + gridWidth);
                    team2landmass.Add(selecting + (gridWidth - 1));
                    team2landmass.Add(selecting - gridWidth);
                    team2landmass.Add(selecting - (gridWidth + 1));

                }
                else
                {

                    hex.ElementAt(selecting - (gridWidth - 1)).Fill = Brushes.Blue;
                    hex.ElementAt(selecting - (gridWidth)).Fill = Brushes.Blue;
                    hex.ElementAt(selecting + (gridWidth)).Fill = Brushes.Blue;
                    hex.ElementAt(selecting + (gridWidth + 1)).Fill = Brushes.Blue;

                    outerids.Add(selecting - (gridWidth - 1));
                    outerids.Add(selecting - gridWidth);
                    outerids.Add(selecting + gridWidth);
                    outerids.Add(selecting + (gridWidth + 1));

                    Panel.SetZIndex(hex.ElementAt(selecting - gridWidth), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting + (gridWidth + 1)), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting - (gridWidth - 1)), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting + gridWidth), 7);

                    team2landmass.Add(selecting - gridWidth);
                    team2landmass.Add(selecting - (gridWidth - 1));
                    team2landmass.Add(selecting + gridWidth);
                    team2landmass.Add(selecting + (gridWidth + 1));

                }

                hex.ElementAt(selecting).Fill = Brushes.Blue;
                hex.ElementAt(selecting - (gridWidth * 2)).Fill = Brushes.Blue;
                hex.ElementAt(selecting + (gridWidth * 2)).Fill = Brushes.Blue;

                outerids.Add(selecting - (gridWidth * 2));
                outerids.Add(selecting + (gridWidth * 2));

                Panel.SetZIndex(hex.ElementAt(selecting - (gridWidth * 2)), 7);

                Panel.SetZIndex(hex.ElementAt(selecting + (gridWidth * 2)), 7);

                Panel.SetZIndex(hex.ElementAt(selecting), 7);

                team2landmass.Add(selecting);
                team2landmass.Add(selecting - (gridWidth * 2));
                team2landmass.Add(selecting + (gridWidth * 2));

                int newcetner;
                int newcenterhex;

                while (team2landmass.Count < landmasize)
                {
                    try
                    {
                        do
                        {
                        retry1:

                            newcetner = random.Next(1, outerids.Count);
                            try
                            {
                                newcenterhex = outerids.ElementAt(newcetner);
                            }
                            catch (Exception e)
                            {
                                outerids.Add(selecting - (gridWidth - 1));
                                outerids.Add(selecting - gridWidth);
                                outerids.Add(selecting + gridWidth);
                                outerids.Add(selecting + (gridWidth + 1));
                                outerids.Add(selecting - (gridWidth * 2));
                                outerids.Add(selecting + (gridWidth * 2));

                                goto retry1;
                            }

                        }
                        while (exclude.Contains(newcenterhex));
                    }
                    catch (Exception e)
                    {
                        outerids.Add(selecting - (gridWidth - 1));
                        outerids.Add(selecting - gridWidth);
                        outerids.Add(selecting + gridWidth);
                        outerids.Add(selecting + (gridWidth + 1));
                        outerids.Add(selecting - (gridWidth * 2));
                        outerids.Add(selecting + (gridWidth * 2));
                        
                        throw;
                    }


                    int newrow = CalculateRow(gridWidth, gridHeight, newcenterhex);

                    if (newrow % 2 == 0)
                    {
                        if (outerids.Contains(newcenterhex - gridWidth) == false && team2landmass.Contains(newcenterhex - gridWidth) == false && team1landmass.Contains(newcenterhex - gridWidth) == false)
                        {
                            newouterids.Add(newcenterhex - gridWidth);
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - gridWidth), 7);
                            hex.ElementAt(newcenterhex - gridWidth).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex - gridWidth);
                        }
                        if (outerids.Contains(newcenterhex - (gridWidth + 1)) == false && team2landmass.Contains(newcenterhex - (gridWidth + 1)) == false && team1landmass.Contains(newcenterhex - (gridWidth + 1)) == false)
                        {
                            newouterids.Add(newcenterhex - (gridWidth + 1));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - (gridWidth + 1)), 7);
                            hex.ElementAt(newcenterhex - (gridWidth + 1)).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex - (gridWidth + 1));
                        }
                        if (outerids.Contains(newcenterhex + (gridWidth - 1)) == false && team2landmass.Contains(newcenterhex + (gridWidth - 1)) == false && team1landmass.Contains(newcenterhex + (gridWidth - 1)) == false)
                        {
                            newouterids.Add(newcenterhex + (gridWidth - 1));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth - 1)), 7);
                            hex.ElementAt(newcenterhex + (gridWidth - 1)).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex + (gridWidth - 1));
                        }
                        if (outerids.Contains(newcenterhex + gridWidth) == false && team2landmass.Contains(newcenterhex + gridWidth) == false && team1landmass.Contains(newcenterhex + gridWidth) == false)
                        {
                            newouterids.Add(newcenterhex + gridWidth);
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth)), 7);
                            hex.ElementAt(newcenterhex + gridWidth).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex + gridWidth);
                        }
                        if (outerids.Contains(newcenterhex + (gridWidth * 2)) == false && team2landmass.Contains(newcenterhex + (gridWidth * 2)) == false && team1landmass.Contains(newcenterhex + (gridWidth * 2)) == false)
                        {
                            newouterids.Add(newcenterhex + (gridWidth * 2));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth * 2)), 7);
                            hex.ElementAt(newcenterhex + (gridWidth * 2)).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex + (gridWidth * 2));
                        }
                        if (outerids.Contains(newcenterhex - (gridWidth * 2)) == false && team2landmass.Contains(newcenterhex - (gridWidth * 2)) == false && team1landmass.Contains(newcenterhex - (gridWidth * 2)) == false)
                        {
                            newouterids.Add(newcenterhex - (gridWidth * 2));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - (gridWidth * 2)), 7);
                            hex.ElementAt(newcenterhex - (gridWidth * 2)).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex - (gridWidth * 2));
                        }

                        outerids.AddRange(newouterids);
                        outerids.Remove(newcenterhex);
                    }
                    else
                    {
                        if (outerids.Contains(newcenterhex - gridWidth) == false && team2landmass.Contains(newcenterhex - gridWidth) == false && team1landmass.Contains(newcenterhex - gridWidth) == false)
                        {
                            newouterids.Add(newcenterhex - gridWidth);
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - gridWidth), 14);
                            hex.ElementAt(newcenterhex - gridWidth).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex - gridWidth);
                        }
                        if (outerids.Contains(newcenterhex + (gridWidth + 1)) == false && team2landmass.Contains(newcenterhex - (gridWidth + 1)) == false && team1landmass.Contains(newcenterhex + (gridWidth + 1)) == false)
                        {
                            newouterids.Add(newcenterhex + (gridWidth + 1));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth + 1)), 14);
                            hex.ElementAt(newcenterhex + (gridWidth + 1)).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex + (gridWidth + 1));
                        }
                        if (outerids.Contains(newcenterhex - (gridWidth - 1)) == false && team2landmass.Contains(newcenterhex + (gridWidth - 1)) == false && team1landmass.Contains(newcenterhex - (gridWidth - 1)) == false)
                        {
                            newouterids.Add(newcenterhex - (gridWidth - 1));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - (gridWidth - 1)), 14);
                            hex.ElementAt(newcenterhex - (gridWidth - 1)).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex - (gridWidth - 1));
                        }
                        if (outerids.Contains(newcenterhex + gridWidth) == false && team2landmass.Contains(newcenterhex + gridWidth) == false && team1landmass.Contains(newcenterhex + gridWidth) == false)
                        {
                            newouterids.Add(newcenterhex + gridWidth);
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + gridWidth), 14);
                            hex.ElementAt(newcenterhex + gridWidth).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex + gridWidth);
                        }
                        if (outerids.Contains(newcenterhex + (gridWidth * 2)) == false && team2landmass.Contains(newcenterhex + (gridWidth * 2)) == false && team1landmass.Contains(newcenterhex + (gridWidth * 2)) == false)
                        {
                            newouterids.Add(newcenterhex + (gridWidth * 2));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth * 2)), 14);
                            hex.ElementAt(newcenterhex + (gridWidth * 2)).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex + (gridWidth * 2));
                        }
                        if (outerids.Contains(newcenterhex - (gridWidth * 2)) == false && team2landmass.Contains(newcenterhex - (gridWidth * 2)) == false && team1landmass.Contains(newcenterhex - (gridWidth * 2)) == false)
                        {
                            newouterids.Add(newcenterhex - (gridWidth * 2));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - (gridWidth * 2)), 14);
                            hex.ElementAt(newcenterhex - (gridWidth * 2)).Fill = Brushes.Blue;
                            team2landmass.Add(newcenterhex - (gridWidth * 2));
                        }

                        outerids.AddRange(newouterids);
                        outerids.Remove(newcenterhex);
                    }
                }

            });

        }

        public async void pickingTeam1points()
        {
            // pick a random hex and propergate outwards for team 1 landmass
            this.Dispatcher.Invoke(() =>
            {
                IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();

                Random random = new Random();

                var range = Enumerable.Range((gridWidth * 2) + 1, hex.Count()).Where(i => !exclude.Contains(i));

                int selecting = random.Next((gridWidth * 2) + 1, hex.Count() - exclude.Count);
                int row = CalculateRow(gridWidth, gridHeight, selecting);

                List<int> outerids = new List<int>();
                List<int> newouterids = new List<int>();

                if (row % 2 == 0)
                {

                    hex.ElementAt(selecting - gridWidth).Fill = Brushes.Red;
                    hex.ElementAt(selecting - (gridWidth + 1)).Fill = Brushes.Red;
                    hex.ElementAt(selecting + (gridWidth - 1)).Fill = Brushes.Red;
                    hex.ElementAt(selecting + gridWidth).Fill = Brushes.Red;

                    outerids.Add(selecting - gridWidth);
                    outerids.Add(selecting - (gridWidth + 1));
                    outerids.Add(selecting + (gridWidth - 1));
                    outerids.Add(selecting + gridWidth);

                    Panel.SetZIndex(hex.ElementAt(selecting - gridWidth), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting - (gridWidth + 1)), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting + (gridWidth - 1)), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting + gridWidth), 7);

                    team1landmass.Add(selecting + gridWidth);
                    team1landmass.Add(selecting + (gridWidth - 1));
                    team1landmass.Add(selecting - gridWidth);
                    team1landmass.Add(selecting - (gridWidth + 1));

                }
                else
                {
                    hex.ElementAt(selecting - (gridWidth - 1)).Fill = Brushes.Red;
                    hex.ElementAt(selecting - (gridWidth)).Fill = Brushes.Red;
                    hex.ElementAt(selecting + (gridWidth)).Fill = Brushes.Red;
                    hex.ElementAt(selecting + (gridWidth + 1)).Fill = Brushes.Red;

                    outerids.Add(selecting - (gridWidth - 1));
                    outerids.Add(selecting - gridWidth);
                    outerids.Add(selecting + gridWidth);
                    outerids.Add(selecting + (gridWidth + 1));

                    Panel.SetZIndex(hex.ElementAt(selecting - gridWidth), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting + (gridWidth + 1)), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting - (gridWidth - 1)), 7);
                    Panel.SetZIndex(hex.ElementAt(selecting + gridWidth), 7);

                    team1landmass.Add(selecting - gridWidth);
                    team1landmass.Add(selecting - (gridWidth - 1));
                    team1landmass.Add(selecting + gridWidth);
                    team1landmass.Add(selecting + (gridWidth + 1));

                }



                hex.ElementAt(selecting).Fill = Brushes.Red;
                hex.ElementAt(selecting - (gridWidth * 2)).Fill = Brushes.Red;
                hex.ElementAt(selecting + (gridWidth * 2)).Fill = Brushes.Red;

                outerids.Add(selecting - (gridWidth * 2));
                outerids.Add(selecting + (gridWidth * 2));

                Panel.SetZIndex(hex.ElementAt(selecting - (gridWidth * 2)), 7);

                Panel.SetZIndex(hex.ElementAt(selecting + (gridWidth * 2)), 7);

                Panel.SetZIndex(hex.ElementAt(selecting), 7);

                team1landmass.Add(selecting);
                team1landmass.Add(selecting - (gridWidth * 2));
                team1landmass.Add(selecting + (gridWidth * 2));

                int newcetner;
                int newcenterhex;

                while (team1landmass.Count < landmasize)
                {
                    do
                    {
                        newcetner = random.Next(0, outerids.Count);
                        newcenterhex = outerids.ElementAt(newcetner);
                    }
                    while (exclude.Contains(newcenterhex));

                    int newrow = CalculateRow(gridWidth, gridHeight, newcenterhex);

                    if (newrow % 2 == 0)
                    {
                        if (outerids.Contains(newcenterhex - gridWidth) == false && team1landmass.Contains(newcenterhex - gridWidth) == false)
                        {
                            newouterids.Add(newcenterhex - gridWidth);
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - gridWidth), 7);
                            hex.ElementAt(newcenterhex - gridWidth).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex - gridWidth);
                        }
                        if (outerids.Contains(newcenterhex - (gridWidth + 1)) == false && team1landmass.Contains(newcenterhex - (gridWidth + 1)) == false)
                        {
                            newouterids.Add(newcenterhex - (gridWidth + 1));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - (gridWidth + 1)), 7);
                            hex.ElementAt(newcenterhex - (gridWidth + 1)).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex - (gridWidth + 1));
                        }
                        if (outerids.Contains(newcenterhex + (gridWidth - 1)) == false && team1landmass.Contains(newcenterhex + (gridWidth - 1)) == false)
                        {
                            newouterids.Add(newcenterhex + (gridWidth - 1));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth - 1)), 7);
                            hex.ElementAt(newcenterhex + (gridWidth - 1)).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex + (gridWidth - 1));
                        }
                        if (outerids.Contains(newcenterhex + gridWidth) == false && team1landmass.Contains(newcenterhex + gridWidth) == false)
                        {
                            newouterids.Add(newcenterhex + gridWidth);
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth)), 7);
                            hex.ElementAt(newcenterhex + gridWidth).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex + gridWidth);
                        }
                        if (outerids.Contains(newcenterhex + (gridWidth * 2)) == false && team1landmass.Contains(newcenterhex + (gridWidth * 2)) == false)
                        {
                            newouterids.Add(newcenterhex + (gridWidth * 2));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth * 2)), 7);
                            hex.ElementAt(newcenterhex + (gridWidth * 2)).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex + (gridWidth * 2));
                        }
                        if (outerids.Contains(newcenterhex - (gridWidth * 2)) == false && team1landmass.Contains(newcenterhex - (gridWidth * 2)) == false)
                        {
                            newouterids.Add(newcenterhex - (gridWidth * 2));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - (gridWidth * 2)), 7);
                            hex.ElementAt(newcenterhex - (gridWidth * 2)).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex - (gridWidth * 2));
                        }

                        outerids.AddRange(newouterids);
                        outerids.Remove(newcenterhex);


                    }
                    else
                    {
                        if (outerids.Contains(newcenterhex - gridWidth) == false && team1landmass.Contains(newcenterhex - gridWidth) == false)
                        {
                            newouterids.Add(newcenterhex - gridWidth);
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - gridWidth), 14);
                            hex.ElementAt(newcenterhex - gridWidth).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex - gridWidth);
                        }
                        if (outerids.Contains(newcenterhex + (gridWidth + 1)) == false && team1landmass.Contains(newcenterhex - (gridWidth + 1)) == false)
                        {
                            newouterids.Add(newcenterhex + (gridWidth + 1));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth + 1)), 14);
                            hex.ElementAt(newcenterhex + (gridWidth + 1)).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex + (gridWidth + 1));
                        }
                        if (outerids.Contains(newcenterhex - (gridWidth - 1)) == false && team1landmass.Contains(newcenterhex + (gridWidth - 1)) == false)
                        {
                            newouterids.Add(newcenterhex - (gridWidth - 1));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - (gridWidth - 1)), 14);
                            hex.ElementAt(newcenterhex - (gridWidth - 1)).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex - (gridWidth - 1));
                        }
                        if (outerids.Contains(newcenterhex + gridWidth) == false && team1landmass.Contains(newcenterhex + gridWidth) == false)
                        {
                            newouterids.Add(newcenterhex + gridWidth);
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + gridWidth), 14);
                            hex.ElementAt(newcenterhex + gridWidth).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex + gridWidth);
                        }
                        if (outerids.Contains(newcenterhex + (gridWidth * 2)) == false && team1landmass.Contains(newcenterhex + (gridWidth * 2)) == false)
                        {
                            newouterids.Add(newcenterhex + (gridWidth * 2));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex + (gridWidth * 2)), 14);
                            hex.ElementAt(newcenterhex + (gridWidth * 2)).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex + (gridWidth * 2));
                        }
                        if (outerids.Contains(newcenterhex - (gridWidth * 2)) == false && team1landmass.Contains(newcenterhex - (gridWidth * 2)) == false)
                        {
                            newouterids.Add(newcenterhex - (gridWidth * 2));
                            Panel.SetZIndex(hex.ElementAt(newcenterhex - (gridWidth * 2)), 14);
                            hex.ElementAt(newcenterhex - (gridWidth * 2)).Fill = Brushes.Red;
                            team1landmass.Add(newcenterhex - (gridWidth * 2));
                        }

                        outerids.AddRange(newouterids);
                        outerids.Remove(newcenterhex);

                    }
                }
            });

        }

        int CalculateRow(int gridWidth, int gridHeight, int hexagonIndex)
        {
            // Calculate the row of the hexagon based on its id
            int totalColumns = gridWidth;
            int totalRows = (gridHeight + 1) * 2 - 1;
            int row = hexagonIndex / totalColumns;

            // Adjust the row for odd and even rows
            if (row % 2 == 1)
            {
                int column = hexagonIndex % totalColumns;
                if (column == totalColumns - 1)
                {
                    row -= 1;
                }
            }

            return row;
        }

        private void SetclickandName()
        {
            // Set the click event for each hexagon
            // generate name for each hexagon
            foreach (int city in team2landmass)
            {


                IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();

                var randomip = new PlaceNameGenerator();

            dovover:

                string namegen = randomip.GenerateRandomPlaceName();

                char[] namegen2 = namegen.ToCharArray();
                char[] alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                char[] finalname = new char[namegen.Length];

                int i = 0;
                foreach (var h in hex)
                {
                    if (namegen == h.Name)
                    {
                        goto dovover;
                    }
                }

                foreach (char letter in namegen2)
                {

                    if (letter == ' ')
                    {
                        namegen = namegen.Replace(letter, '_');
                    }
                    else if (bannedletters.Contains(letter))
                    {
                        namegen = namegen.Replace(letter, '_');
                    }

                    else if (alphabet.Contains(letter) == false)
                    {
                        int it = namegen.IndexOf(letter);
                        if (it == -1)
                        {
                            break;
                        }
                        namegen = namegen.Remove(it);
                    }
                }


                hex.ElementAt(city).Name = namegen;
                hex.ElementAt(city).Uid = "2";
                hex.ElementAt(city).MouseUp += map_click;

                Panel.SetZIndex(hex.ElementAt(city), 14);


            }
            foreach (int city in team1landmass)
            {
                IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();
            dovover2:

                var randomip = new PlaceNameGenerator();

                string namegen = randomip.GenerateRandomPlaceName();
                
                char[] namegen2 = namegen.ToCharArray();
                char[] alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

                int i = 0;

                foreach(var h in hex)
                {
                    if(namegen == h.Name)
                    {
                        goto dovover2;
                    }
                }

                foreach (char letter in namegen2)
                {

                    if (letter == ' ')
                    {
                        namegen = namegen.Replace(letter, '_');
                    }
                    else if (bannedletters.Contains(letter))
                    {
                        namegen = namegen.Replace(letter, '_');
                    }
                    else if (namegen.Length == 0)
                    {
                        goto dovover2;
                    }
                    else if (alphabet.Contains(letter) == false)
                    {
                        int it = namegen.IndexOf(letter);
                        if (it == -1)
                        {
                            break;
                        }
                        namegen = namegen.Remove(it);
                    }




                }
                hex.ElementAt(city).Name = namegen;
                hex.ElementAt(city).Uid = "1";
                hex.ElementAt(city).MouseUp += map_click;
                Panel.SetZIndex(hex.ElementAt(city), 14);

            }

        }

        // sending map method
        private async void packmap()
        {
            // pack map into a list of citytosend, send to server across 4 packets
            List<citytosend> mappacking = new List<citytosend>();
            List<citytosend> mappacking1q = new List<citytosend>();
            List<citytosend> mappacking2q = new List<citytosend>();
            List<citytosend> mappacking3q = new List<citytosend>();
            List<citytosend> mappacking4q = new List<citytosend>();

            foreach (var c in citylist)
            {

                mappacking.Add(new citytosend { name = c.name, ID = c.ID, owner = c.owner });
            }
            int i = 0;
            int Q1 = (mappacking.Count / 4);
            int Q2 = Q1 * 2;
            int Q3 = Q1 * 3;
            int Q4 = Q1 * 4;
            while (i <= Q1)
            {
                mappacking1q.Add(mappacking[i]);
                i = i + 1;
            }
            while (i <= Q2)
            {
                mappacking2q.Add(mappacking[i]);
                i = i + 1;
            }
            while (i <= Q3)
            {
                mappacking3q.Add(mappacking[i]);
                i = i + 1;
            }
            while (i <= (Q4 - 1))
            {
                mappacking4q.Add(mappacking[i]);
                i = i + 1;
            }

            string serializedDataq1 = JsonConvert.SerializeObject(mappacking1q);
            string serializedDataq2 = JsonConvert.SerializeObject(mappacking2q);
            string serializedDataq3 = JsonConvert.SerializeObject(mappacking3q);
            string serializedDataq4 = JsonConvert.SerializeObject(mappacking4q);
            Showsyncscreen();
            try
            {
                await connection.InvokeAsync(methodName: "Syncmap1", serializedDataq1, Selfuser);

            }
            catch (Exception ex)
            {
                messagedisplay_box.Items.Insert(0, ex.Message);
            }
            Thread.Sleep(5000);

            try
            {
                await connection.InvokeAsync(methodName: "Syncmap2", serializedDataq2, Selfuser);

            }
            catch (Exception ex)
            {
                messagedisplay_box.Items.Insert(0, ex.Message);
            }
            Thread.Sleep(5000);

            try
            {
                await connection.InvokeAsync(methodName: "Syncmap3", serializedDataq3, Selfuser);

            }

            catch (Exception ex)
            {
                messagedisplay_box.Items.Insert(0, ex.Message);
            }
            Thread.Sleep(5000);

            try
            {
                await connection.InvokeAsync(methodName: "Syncmap4", serializedDataq4, Selfuser);

            }
            catch (Exception ex)
            {
                messagedisplay_box.Items.Insert(0, ex.Message);
            }

            Removesyncscreen();

        }

        // syncing map and handling connection changes methods

        public void Syncmap1(string incommingmapsync)
        {
            // unpack map from list of citytosend, update map, show sync screen
            this.Dispatcher.Invoke(() =>
            {
                Showsyncscreen();
                List<citytosend> cities = JsonConvert.DeserializeObject<List<citytosend>>(incommingmapsync);

                citylist.Clear();

                IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();
                foreach (var c in cities)
                {
                    var hexselected = hex.ElementAt(c.ID);

                    hexselected.Name = c.name;
                    if (c.owner == "Team1")
                    {
                        hexselected.Uid = "1";
                    }
                    else if (c.owner == "Team2")
                    {
                        hexselected.Uid = "2";
                    }
                    else if (c.owner == "none")
                    {
                        hexselected.Opacity = 0.5;
                        Panel.SetZIndex(hexselected, 4);
                    }
                    citylist.Add(new city { ID = c.ID, name = c.name, owner = c.owner });

                    hexselected.Fill = Brushes.Black;
                    hexselected.Stroke = Untargeted;
                    hexselected.StrokeThickness = 2;

                }

            });

        }
        public void Syncmap2(string incommingmapsync)
        {
            // unpack map from list of citytosend, update map
            this.Dispatcher.Invoke(() =>
            {
                List<citytosend> cities = JsonConvert.DeserializeObject<List<citytosend>>(incommingmapsync);

                IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();

                foreach (var c in cities)
                {
                    var hexselected = hex.ElementAt(c.ID);

                    hexselected.Name = c.name;
                    if (c.owner == "Team1")
                    {
                        hexselected.Uid = "1";
                    }
                    else if (c.owner == "Team2")
                    {
                        hexselected.Uid = "2";
                    }
                    else if (c.owner == "none")
                    {
                        hexselected.Opacity = 0.5;
                        Panel.SetZIndex(hexselected, 4);
                    }
                    citylist.Add(new city { ID = c.ID, name = c.name, owner = c.owner, });

                    hexselected.Fill = Brushes.Black;
                    hexselected.Stroke = Untargeted;
                    hexselected.StrokeThickness = 2;

                }

            });

        }
        public void Syncmap3(string incommingmapsync)
        {
            // unpack map from list of citytosend, update map
            this.Dispatcher.Invoke(() =>
            {
                List<citytosend> cities = JsonConvert.DeserializeObject<List<citytosend>>(incommingmapsync);

                IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();

                foreach (var c in cities)
                {
                    var hexselected = hex.ElementAt(c.ID);

                    hexselected.Name = c.name;
                    if (c.owner == "Team1")
                    {
                        hexselected.Uid = "1";
                    }
                    else if (c.owner == "Team2")
                    {
                        hexselected.Uid = "2";
                    }
                    else if (c.owner == "none")
                    {
                        hexselected.Opacity = 0.5;
                        Panel.SetZIndex(hexselected, 4);
                    }
                    citylist.Add(new city { ID = c.ID, name = c.name, owner = c.owner });

                    hexselected.Fill = Brushes.Black;
                    hexselected.Stroke = Untargeted;
                    hexselected.StrokeThickness = 2;

                }

            });

        }
        public void Syncmap4(string incommingmapsync)
        {
            // unpack map from list of citytosend, update map, remove sync screen
            this.Dispatcher.Invoke(() =>
            {
                List<citytosend> cities = JsonConvert.DeserializeObject<List<citytosend>>(incommingmapsync);

                IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();

                foreach (var c in cities)
                {
                    var hexselected = hex.ElementAt(c.ID);

                    hexselected.Name = c.name;
                    if (c.owner == "Team1")
                    {
                        hexselected.Uid = "1";
                    }
                    else if (c.owner == "Team2")
                    {
                        hexselected.Uid = "2";
                    }
                    else if (c.owner == "none")
                    {
                        hexselected.Opacity = 0.5;
                        Panel.SetZIndex(hexselected, 4);
                    }
                    citylist.Add(new city { ID = c.ID, name = c.name, owner = c.owner });

                    hexselected.Fill = Brushes.Black;
                    hexselected.Stroke = Untargeted;
                    hexselected.StrokeThickness = 2;

                }

                if (Selfuser == "Team1")
                {
                    SetcolorTeam1();
                }
                if (Selfuser == "Team2")
                {
                    SetcolorTeam2();
                }

            });

            Setclickaftersync();
            Removesyncscreen();
        }

        public void Setclickaftersync()
        {
            // set click event for all hexes after successful sync
            this.Dispatcher.Invoke(() =>
            {
                IEnumerable<Polygon> hex = map.Children.OfType<Polygon>();
                team1landmass.Clear();
                team2landmass.Clear();
                foreach (var h in hex)
                {
                    h.ToolTip = null;
                    h.Name = null;
                    h.MouseUp -= map_click;
                }
                foreach (var c in citylist)
                {
                    if (c.owner == "Team1")
                    {
                        team1landmass.Add(c.ID);
                        hex.ElementAt(c.ID).Name = c.name;
                        hex.ElementAt(c.ID).Opacity = 1;
                        hex.ElementAt(c.ID).Uid = "1";
                        hex.ElementAt(c.ID).MouseUp += map_click;
                    }
                    if (c.owner == "Team2")
                    {
                        team2landmass.Add(c.ID);
                        hex.ElementAt(c.ID).Name = c.name;
                        hex.ElementAt(c.ID).Opacity = 1;
                        hex.ElementAt(c.ID).Uid = "2";
                        hex.ElementAt(c.ID).MouseUp += map_click;
                    }

                }
                Setuptooltips();

            });
        }

        public async void Showsyncscreen()
        {
            // show sync screen
            this.Dispatcher.Invoke(() =>
            {
                
                main_grid.Children.Remove(main_grid.FindName("Reconnectinglabel1") as Label);

                if(main_grid.Children.Contains((UIElement)main_grid.FindName("scrim1")))
                {
                    main_grid.Children.Remove(main_grid.FindName("scrim1") as Rectangle);
                    UnregisterName("scrim1");
                }
                if (main_grid.Children.Contains((UIElement)main_grid.FindName("Reconnectinglabel1")))
                {
                    main_grid.Children.Remove(main_grid.FindName("Reconnectinglabel1") as Label);
                    UnregisterName("Reconnectinglabel1");
                }
                if (main_grid.Children.Contains(main_grid.FindName("Ellipse11") as Ellipse))
                {
                    main_grid.Children.Remove(main_grid.FindName("Ellipse11") as Ellipse);
                    UnregisterName("Ellipse11");
                }
                if (main_grid.Children.Contains(main_grid.FindName("Ellipse21") as Ellipse))
                {
                    main_grid.Children.Remove(main_grid.FindName("Ellipse21") as Ellipse);
                    UnregisterName("Ellipse21");
                }
                if (main_grid.Children.Contains(main_grid.FindName("Ellipse31") as Ellipse))
                {
                    main_grid.Children.Remove(main_grid.FindName("Ellipse31") as Ellipse);
                    UnregisterName("Ellipse31");
                }

                Rectangle scrim = new Rectangle();

                scrim.Fill = Brushes.Black;
                scrim.Opacity = 0.8;
                scrim.Name = "scrim";
                Grid.SetColumnSpan(scrim, 3);
                Grid.SetRowSpan(scrim, 4);
                Panel.SetZIndex(scrim, 40);

                Label text = new Label();

                text.Content = "Syncing map with server";
                text.Foreground = Brushes.White;
                text.FontFamily = text.TryFindResource("AVB") as FontFamily;
                text.Name = "synclabel";
                text.FontSize = 100;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Bottom;

                Grid.SetColumnSpan(text, 3);
                Grid.SetRow(text, 0);
                Grid.SetZIndex(text, 41);

                Ellipse ellipse1 = new Ellipse();
                ellipse1.Fill = Brushes.White;
                ellipse1.Width = 40;
                ellipse1.Height = 40;
                ellipse1.Opacity = 1;
                Panel.SetZIndex(ellipse1, 45);
                Grid.SetRow(ellipse1, 1);
                Grid.SetColumn(ellipse1, 1);

                Ellipse ellipse2 = new Ellipse();
                ellipse2.Fill = Brushes.White;
                ellipse2.Width = 40;
                ellipse2.Height = 40;
                ellipse2.Opacity = 1;
                ellipse2.Margin = new Thickness(193, 70, 400, 70);
                Panel.SetZIndex(ellipse2, 45);
                Grid.SetRow(ellipse2, 1);
                Grid.SetColumn(ellipse2, 1);

                Ellipse ellipse3 = new Ellipse();
                ellipse3.Fill = Brushes.White;
                ellipse3.Width = 40;
                ellipse3.Height = 40;
                ellipse3.Opacity = 1;
                ellipse3.Margin = new Thickness(400, 70, 200, 70);
                Panel.SetZIndex(ellipse3, 45);
                Grid.SetRow(ellipse3, 1);
                Grid.SetColumn(ellipse3, 1);

                DoubleAnimation da1 = new DoubleAnimation();
                da1.From = -1;
                da1.To = 1;
                da1.BeginTime = TimeSpan.FromSeconds(1);
                da1.Duration = TimeSpan.FromSeconds(2);
                da1.RepeatBehavior = RepeatBehavior.Forever;
                da1.AutoReverse = true;

                DoubleAnimation da2 = new DoubleAnimation();
                da2.From = -1;
                da2.To = 1;
                da2.BeginTime = TimeSpan.FromSeconds(0);
                da2.Duration = TimeSpan.FromSeconds(2);
                da2.RepeatBehavior = RepeatBehavior.Forever;
                da2.AutoReverse = true;

                DoubleAnimation da3 = new DoubleAnimation();
                da3.From = -1;
                da3.To = 1;
                da3.BeginTime = TimeSpan.FromSeconds(2);
                da3.Duration = TimeSpan.FromSeconds(2);
                da3.RepeatBehavior = RepeatBehavior.Forever;
                da3.AutoReverse = true;

                Storyboard sb1 = new Storyboard();


                Storyboard.SetTarget(da1, ellipse1);
                Storyboard.SetTargetProperty(da1, new PropertyPath(Ellipse.OpacityProperty));


                Storyboard.SetTarget(da2, ellipse2);
                Storyboard.SetTargetProperty(da2, new PropertyPath(Ellipse.OpacityProperty));


                Storyboard.SetTarget(da3, ellipse3);
                Storyboard.SetTargetProperty(da3, new PropertyPath(Ellipse.OpacityProperty));

                sb1.Children.Add(da1);
                sb1.Children.Add(da2);
                sb1.Children.Add(da3);


                RegisterName("scrim", scrim);
                RegisterName("synclabel", text);
                RegisterName("Ellipse1", ellipse1);
                RegisterName("Ellipse2", ellipse2);
                RegisterName("Ellipse3", ellipse3);

                main_grid.Children.Add(scrim);
                main_grid.Children.Add(ellipse1);
                main_grid.Children.Add(ellipse2);
                main_grid.Children.Add(ellipse3);
                main_grid.Children.Add(text);

                sb1.Begin();


            });


        }

        public async void Removesyncscreen()
        {
            // remove sync screen
            this.Dispatcher.Invoke(() =>
            {
                
                main_grid.Children.Remove(main_grid.FindName("scrim") as Rectangle);
                main_grid.Children.Remove(main_grid.FindName("synclabel") as Label);
                main_grid.Children.Remove(main_grid.FindName("Ellipse1") as Ellipse);
                main_grid.Children.Remove(main_grid.FindName("Ellipse2") as Ellipse);
                main_grid.Children.Remove(main_grid.FindName("Ellipse3") as Ellipse);
                UnregisterName("scrim");
                UnregisterName("synclabel");
                UnregisterName("Ellipse1");
                UnregisterName("Ellipse2");
                UnregisterName("Ellipse3");
            });
        }

        private void Syncmapbutton_Click(object sender, RoutedEventArgs e)
        {
            // handle sync map button click
            packmap();
        }

        private void Exitbutton_Click(object sender, RoutedEventArgs e)
        {
            // exit button click
            var s = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(s);
            string strSettingsXmlFilePath = System.IO.Path.Combine(strWorkPath, "TS-PEACE-Client.exe");
            System.Diagnostics.Process.Start(strSettingsXmlFilePath);
            Environment.Exit(1);
            
        }

        public void reconnect()
        {
            // reconnect to server
            this.Dispatcher.Invoke(() =>
            {
                Rectangle scrim = new Rectangle();

                scrim.Fill = Brushes.Black;
                scrim.Opacity = 0.8;
                scrim.Name = "scrim";
                Grid.SetColumnSpan(scrim, 3);
                Grid.SetRowSpan(scrim, 4);
                Panel.SetZIndex(scrim, 40);

                Label text = new Label();
                text.Content = "Reconnecting to server";
                text.Foreground = Brushes.White;
                text.FontFamily = text.TryFindResource("AVB") as FontFamily;
                text.Name = "synclabel";
                text.FontSize = 100;
                Grid.SetColumnSpan(text, 3);
                Grid.SetRow(text, 0);
                Grid.SetZIndex(text, 41);
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Bottom;


                Ellipse ellipse1 = new Ellipse();
                ellipse1.Fill = Brushes.White;
                ellipse1.Width = 40;
                ellipse1.Height = 40;
                ellipse1.Opacity = 1;
                Panel.SetZIndex(ellipse1, 45);
                Grid.SetRow(ellipse1, 1);
                Grid.SetColumn(ellipse1, 1);

                Ellipse ellipse2 = new Ellipse();
                ellipse2.Fill = Brushes.White;
                ellipse2.Width = 40;
                ellipse2.Height = 40;
                ellipse2.Opacity = 1;
                ellipse2.Margin = new Thickness(193, 70, 400, 70);
                Panel.SetZIndex(ellipse2, 45);
                Grid.SetRow(ellipse2, 1);
                Grid.SetColumn(ellipse2, 1);

                Ellipse ellipse3 = new Ellipse();
                ellipse3.Fill = Brushes.White;
                ellipse3.Width = 40;
                ellipse3.Height = 40;
                ellipse3.Opacity = 1;
                ellipse3.Margin = new Thickness(400, 70, 200, 70);
                Panel.SetZIndex(ellipse3, 45);
                Grid.SetRow(ellipse3, 1);
                Grid.SetColumn(ellipse3, 1);

                DoubleAnimation da1 = new DoubleAnimation();
                da1.From = -1;
                da1.To = 1;
                da1.BeginTime = TimeSpan.FromSeconds(1);
                da1.Duration = TimeSpan.FromSeconds(2);
                da1.RepeatBehavior = RepeatBehavior.Forever;
                da1.AutoReverse = true;


                DoubleAnimation da2 = new DoubleAnimation();
                da2.From = -1;
                da2.To = 1;
                da2.BeginTime = TimeSpan.FromSeconds(0);
                da2.Duration = TimeSpan.FromSeconds(2);
                da2.RepeatBehavior = RepeatBehavior.Forever;
                da2.AutoReverse = true;

                DoubleAnimation da3 = new DoubleAnimation();
                da3.From = -1;
                da3.To = 1;
                da3.BeginTime = TimeSpan.FromSeconds(2);
                da3.Duration = TimeSpan.FromSeconds(2);
                da3.RepeatBehavior = RepeatBehavior.Forever;
                da3.AutoReverse = true;

                Storyboard sb1 = new Storyboard();


                Storyboard.SetTarget(da1, ellipse1);
                Storyboard.SetTargetProperty(da1, new PropertyPath(Ellipse.OpacityProperty));


                Storyboard.SetTarget(da2, ellipse2);
                Storyboard.SetTargetProperty(da2, new PropertyPath(Ellipse.OpacityProperty));


                Storyboard.SetTarget(da3, ellipse3);
                Storyboard.SetTargetProperty(da3, new PropertyPath(Ellipse.OpacityProperty));

                sb1.Children.Add(da1);
                sb1.Children.Add(da2);
                sb1.Children.Add(da3);


                RegisterName("scrim1", scrim);
                RegisterName("Reconnectinglabel1", text);
                RegisterName("Ellipse11", ellipse1);
                RegisterName("Ellipse21", ellipse2);
                RegisterName("Ellipse31", ellipse3);

                main_grid.Children.Add(scrim);
                main_grid.Children.Add(ellipse1);
                main_grid.Children.Add(ellipse2);
                main_grid.Children.Add(ellipse3);
                main_grid.Children.Add(text);

                sb1.Begin();

            });
        }

        public async void connectionsolidify()
        {
            // solidify connection, handle cannot connect and waiting for other players
            this.Dispatcher.Invoke(() =>
            {
                Rectangle scrim = new Rectangle();

                scrim.Fill = Brushes.Black;
                scrim.Opacity = 0.8;
                scrim.Name = "scrim";
                Grid.SetColumnSpan(scrim, 3);
                Grid.SetRowSpan(scrim, 4);
                Panel.SetZIndex(scrim, 40);

                Label text = new Label();
                text.Content = "Connecting to server";
                text.Foreground = Brushes.White;
                text.FontFamily = text.TryFindResource("AVB") as FontFamily;
                text.Name = "synclabel";
                text.FontSize = 100;
                Grid.SetColumnSpan(text, 3);
                Grid.SetRow(text, 0);
                Grid.SetZIndex(text, 41);
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Bottom;


                Ellipse ellipse1 = new Ellipse();
                ellipse1.Fill = Brushes.White;
                ellipse1.Width = 40;
                ellipse1.Height = 40;
                ellipse1.Opacity = 1;
                Panel.SetZIndex(ellipse1, 45);
                Grid.SetRow(ellipse1, 1);
                Grid.SetColumn(ellipse1, 1);

                Ellipse ellipse2 = new Ellipse();
                ellipse2.Fill = Brushes.White;
                ellipse2.Width = 40;
                ellipse2.Height = 40;
                ellipse2.Opacity = 1;
                ellipse2.Margin = new Thickness(193, 70, 400, 70);
                Panel.SetZIndex(ellipse2, 45);
                Grid.SetRow(ellipse2, 1);
                Grid.SetColumn(ellipse2, 1);

                Ellipse ellipse3 = new Ellipse();
                ellipse3.Fill = Brushes.White;
                ellipse3.Width = 40;
                ellipse3.Height = 40;
                ellipse3.Opacity = 1;
                ellipse3.Margin = new Thickness(400, 70, 200, 70);
                Panel.SetZIndex(ellipse3, 45);
                Grid.SetRow(ellipse3, 1);
                Grid.SetColumn(ellipse3, 1);

                DoubleAnimation da1 = new DoubleAnimation();
                da1.From = -1;
                da1.To = 1;
                da1.BeginTime = TimeSpan.FromSeconds(1);
                da1.Duration = TimeSpan.FromSeconds(2);
                da1.RepeatBehavior = RepeatBehavior.Forever;
                da1.AutoReverse = true;


                DoubleAnimation da2 = new DoubleAnimation();
                da2.From = -1;
                da2.To = 1;
                da2.BeginTime = TimeSpan.FromSeconds(0);
                da2.Duration = TimeSpan.FromSeconds(2);
                da2.RepeatBehavior = RepeatBehavior.Forever;
                da2.AutoReverse = true;

                DoubleAnimation da3 = new DoubleAnimation();
                da3.From = -1;
                da3.To = 1;
                da3.BeginTime = TimeSpan.FromSeconds(2);
                da3.Duration = TimeSpan.FromSeconds(2);
                da3.RepeatBehavior = RepeatBehavior.Forever;
                da3.AutoReverse = true;

                Storyboard sb1 = new Storyboard();


                Storyboard.SetTarget(da1, ellipse1);
                Storyboard.SetTargetProperty(da1, new PropertyPath(Ellipse.OpacityProperty));


                Storyboard.SetTarget(da2, ellipse2);
                Storyboard.SetTargetProperty(da2, new PropertyPath(Ellipse.OpacityProperty));


                Storyboard.SetTarget(da3, ellipse3);
                Storyboard.SetTargetProperty(da3, new PropertyPath(Ellipse.OpacityProperty));

                sb1.Children.Add(da1);
                sb1.Children.Add(da2);
                sb1.Children.Add(da3);


                RegisterName("scrim1", scrim);
                RegisterName("Reconnectinglabel1", text);
                RegisterName("Ellipse11", ellipse1);
                RegisterName("Ellipse21", ellipse2);
                RegisterName("Ellipse31", ellipse3);
                

                main_grid.Children.Add(scrim);
                main_grid.Children.Add(ellipse1);
                main_grid.Children.Add(ellipse2);
                main_grid.Children.Add(ellipse3);
                main_grid.Children.Add(text);

                sb1.Begin();

            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("connected");
                main_grid.Children.Remove(main_grid.FindName("Ellipse11") as Ellipse);
                main_grid.Children.Remove(main_grid.FindName("Ellipse21") as Ellipse);
                main_grid.Children.Remove(main_grid.FindName("Ellipse31") as Ellipse);
                main_grid.Children.Remove(main_grid.FindName("scrim1") as Rectangle);
                main_grid.Children.Remove(main_grid.FindName("Reconnectinglabel1") as Label);
                UnregisterName("scrim1");
                UnregisterName("Reconnectinglabel1");
                UnregisterName("Ellipse11");
                UnregisterName("Ellipse21");
                UnregisterName("Ellipse31");
                await connection.InvokeAsync(methodName: "UserCheck");


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
                main_grid.Children.Remove(main_grid.FindName("Ellipse11") as Ellipse);
                main_grid.Children.Remove(main_grid.FindName("Ellipse21") as Ellipse);
                main_grid.Children.Remove(main_grid.FindName("Ellipse31") as Ellipse);
                var exibutton = (main_grid.FindName("Exitbutton") as Button);
                Grid.SetZIndex(exibutton, 42);
                var title = main_grid.FindName("Reconnectinglabel1") as Label;

                title.Content = "Connection failed";

                TimeSpan recoontetingtime = TimeSpan.FromSeconds(30);

                TextBlock textBlock = new TextBlock();
                textBlock.Text = "retrying in " + recoontetingtime;

                textBlock.Foreground = Brushes.White;
                textBlock.FontFamily = textBlock.TryFindResource("AVB") as FontFamily;
                textBlock.FontSize = 50;
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Bottom;
                textBlock.Margin = new Thickness(0, 0, 0, 100);
                textBlock.Name = "reconnectingtextblock";
                Grid.SetColumnSpan(textBlock, 3);
                Grid.SetRow(textBlock, 1);
                Grid.SetZIndex(textBlock, 41);

                main_grid.Children.Add(textBlock);

                while(recoontetingtime > TimeSpan.Zero)
                {
                    await Task.Delay(1000);
                    recoontetingtime = recoontetingtime.Subtract(TimeSpan.FromSeconds(1));
                    textBlock.Text = "retrying in " + recoontetingtime;
                }
                main_grid.Children.Remove(textBlock);
                main_grid.Children.Remove(main_grid.FindName("scrim1") as Rectangle);
                main_grid.Children.Remove(main_grid.FindName("Reconnectinglabel1") as Label);

                
                UnregisterName("scrim1");
                UnregisterName("Reconnectinglabel1");
                UnregisterName("Ellipse11");
                UnregisterName("Ellipse21");
                UnregisterName("Ellipse31");
                
                connectionsolidify();

            }


        }

        public async Task Syncrequest()
        {
            // handle sync request from server
            Syncmapbutton_Click(null, null);

        }

        public async Task LoneClient()
        {
            // wait for second player to join
            this.Dispatcher.Invoke(() =>
            {
                Rectangle scrim = new Rectangle();

                scrim.Fill = Brushes.Black;
                scrim.Opacity = 0.8;
                scrim.Name = "scrim";
                Grid.SetColumnSpan(scrim, 3);
                Grid.SetRowSpan(scrim, 4);
                Panel.SetZIndex(scrim, 40);

                Label text = new Label();
                text.Content = "Awaiting second player";
                text.Foreground = Brushes.White;
                text.FontFamily = text.TryFindResource("AVB") as FontFamily;
                text.Name = "synclabel";
                text.FontSize = 100;
                Grid.SetColumnSpan(text, 3);
                Grid.SetRow(text, 0);
                Grid.SetZIndex(text, 41);
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Bottom;


                Ellipse ellipse1 = new Ellipse();
                ellipse1.Fill = Brushes.White;
                ellipse1.Width = 40;
                ellipse1.Height = 40;
                ellipse1.Opacity = 1;
                Panel.SetZIndex(ellipse1, 45);
                Grid.SetRow(ellipse1, 1);
                Grid.SetColumn(ellipse1, 1);

                Ellipse ellipse2 = new Ellipse();
                ellipse2.Fill = Brushes.White;
                ellipse2.Width = 40;
                ellipse2.Height = 40;
                ellipse2.Opacity = 1;
                ellipse2.Margin = new Thickness(193, 70, 400, 70);
                Panel.SetZIndex(ellipse2, 45);
                Grid.SetRow(ellipse2, 1);
                Grid.SetColumn(ellipse2, 1);

                Ellipse ellipse3 = new Ellipse();
                ellipse3.Fill = Brushes.White;
                ellipse3.Width = 40;
                ellipse3.Height = 40;
                ellipse3.Opacity = 1;
                ellipse3.Margin = new Thickness(400, 70, 200, 70);
                Panel.SetZIndex(ellipse3, 45);
                Grid.SetRow(ellipse3, 1);
                Grid.SetColumn(ellipse3, 1);

                DoubleAnimation da1 = new DoubleAnimation();
                da1.From = -1;
                da1.To = 1;
                da1.BeginTime = TimeSpan.FromSeconds(1);
                da1.Duration = TimeSpan.FromSeconds(2);
                da1.RepeatBehavior = RepeatBehavior.Forever;
                da1.AutoReverse = true;


                DoubleAnimation da2 = new DoubleAnimation();
                da2.From = -1;
                da2.To = 1;
                da2.BeginTime = TimeSpan.FromSeconds(0);
                da2.Duration = TimeSpan.FromSeconds(2);
                da2.RepeatBehavior = RepeatBehavior.Forever;
                da2.AutoReverse = true;

                DoubleAnimation da3 = new DoubleAnimation();
                da3.From = -1;
                da3.To = 1;
                da3.BeginTime = TimeSpan.FromSeconds(2);
                da3.Duration = TimeSpan.FromSeconds(2);
                da3.RepeatBehavior = RepeatBehavior.Forever;
                da3.AutoReverse = true;

                Storyboard sb1 = new Storyboard();


                Storyboard.SetTarget(da1, ellipse1);
                Storyboard.SetTargetProperty(da1, new PropertyPath(Ellipse.OpacityProperty));


                Storyboard.SetTarget(da2, ellipse2);
                Storyboard.SetTargetProperty(da2, new PropertyPath(Ellipse.OpacityProperty));


                Storyboard.SetTarget(da3, ellipse3);
                Storyboard.SetTargetProperty(da3, new PropertyPath(Ellipse.OpacityProperty));

                sb1.Children.Add(da1);
                sb1.Children.Add(da2);
                sb1.Children.Add(da3);


                RegisterName("scrim1", scrim);
                RegisterName("Reconnectinglabel1", text);
                RegisterName("Ellipse11", ellipse1);
                RegisterName("Ellipse21", ellipse2);
                RegisterName("Ellipse31", ellipse3);


                main_grid.Children.Add(scrim);
                main_grid.Children.Add(ellipse1);
                main_grid.Children.Add(ellipse2);
                main_grid.Children.Add(ellipse3);
                main_grid.Children.Add(text);

                sb1.Begin();

            });
        }



    }
}
