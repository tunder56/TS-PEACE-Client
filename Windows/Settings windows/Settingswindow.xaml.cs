using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
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
using Color = System.Drawing.Color;

namespace TS_PEACE_Client.Windows.Settings_windows
{
    /// <summary>
    /// Interaction logic for Settingswindow.xaml
    /// </summary>
    public partial class Settingswindow : Window
    {
        // main line code
        public Settingswindow()
        {
            InitializeComponent();

            // get current setting colors and set them as preselcted
            getcurrentcolors();

            Attack1rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(GlobalSettings.Default.Attack1color.A, GlobalSettings.Default.Attack1color.R, GlobalSettings.Default.Attack1color.G, GlobalSettings.Default.Attack1color.B));
            Attack2rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(GlobalSettings.Default.Attack2color.A, GlobalSettings.Default.Attack2color.R, GlobalSettings.Default.Attack2color.G, GlobalSettings.Default.Attack2color.B));
            Attack3rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(GlobalSettings.Default.Attack3color.A, GlobalSettings.Default.Attack3color.R, GlobalSettings.Default.Attack3color.G, GlobalSettings.Default.Attack3color.B));
            Attack4rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(GlobalSettings.Default.Attack4color.A, GlobalSettings.Default.Attack4color.R, GlobalSettings.Default.Attack4color.G, GlobalSettings.Default.Attack4color.B));

            Setcombolists();

            Attack1name.Text = GlobalSettings.Default.Attack1name;
            Attack2name.Text = GlobalSettings.Default.Attack2name;
            Attack3name.Text = GlobalSettings.Default.Attack3name;
            Attack4name.Text = GlobalSettings.Default.Attack4name;



        }


        




        private void Setcombolists()
        {
            // add colors to comboboxes
            Attack1box.Items.Add("Default");
            Attack2box.Items.Add("Default");
            Attack3box.Items.Add("Default");
            Attack4box.Items.Add("Default");

            var colorNames = typeof(Brushes)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Select(x => x.Name);

            foreach (var color in colorNames)
            {
                Attack1box.Items.Add(color);
                Attack2box.Items.Add(color);
                Attack3box.Items.Add(color);
                Attack4box.Items.Add(color);
            }


        }

        private void getcurrentcolors()
        {
            // get current colors and set them as selected
            if(GlobalSettings.Default.Attack1color == Color.FromArgb(255, 240, 201, 135))
            {
                Attack1box.SelectedItem = "Default";
            }
            else
            {
                Attack1box.SelectedItem = GlobalSettings.Default.Attack1color.Name;
            }
            if (GlobalSettings.Default.Attack2color == Color.FromArgb(255, 97, 137, 133))
            {
                Attack2box.SelectedItem = "Default";
            }
            else
            {
                Attack2box.SelectedItem = GlobalSettings.Default.Attack2color.Name;
            }
            if (GlobalSettings.Default.Attack3color == Color.FromArgb(255, 149, 125, 149))
            {
                Attack3box.SelectedItem = "Default";
            }
            else
            {
                Attack3box.SelectedItem = GlobalSettings.Default.Attack3color.Name;
            }
            if (GlobalSettings.Default.Attack4color == Color.FromArgb(255, 239, 111, 108))
            {
                Attack4box.SelectedItem = "Default";
            }
            else
            {
                Attack4box.SelectedItem = GlobalSettings.Default.Attack4color.Name;
            }

        }

        private void Savecurrentcolors()
        {
            // save current colors
            if(Attack1box.SelectedItem == "Default") 
            { 
                
                GlobalSettings.Default.Attack1color = Color.FromArgb(255, 240, 201, 135);
            }
            else if (Attack1box.SelectedItem != "Custom")
            {

                GlobalSettings.Default.Attack1color = Color.FromName(Attack1box.SelectedItem.ToString());
            }
            if (Attack2box.SelectedItem == "Default")
            {

                GlobalSettings.Default.Attack2color = Color.FromArgb(255, 97, 137, 133);
            }
            else if (Attack2box.SelectedItem != "Custom")
            {
                GlobalSettings.Default.Attack2color = Color.FromName(Attack2box.SelectedItem.ToString());
            }
            if (Attack3box.SelectedItem == "Default")
            {

                GlobalSettings.Default.Attack3color = Color.FromArgb(255, 149, 125, 149);
            }
            else if (Attack3box.SelectedItem != "Custom")
            {
                GlobalSettings.Default.Attack3color = Color.FromName(Attack3box.SelectedItem.ToString());
            }
            if (Attack4box.SelectedItem == "Default")
            {

                GlobalSettings.Default.Attack4color = Color.FromArgb(255, 239, 111, 108);
            }
            else if (Attack4box.SelectedItem != "Custom")
            {
                GlobalSettings.Default.Attack4color = Color.FromName(Attack4box.SelectedItem.ToString());
            }

            Label label = new Label();

            label.Content = "Saved";
            label.Foreground = new SolidColorBrush(Colors.LightGreen);
            label.FontSize = 20;
            label.FontFamily = new FontFamily("Avara");
            Grid.SetColumn(label, 7);
            Grid.SetRow(label, 0);
            label.VerticalAlignment = VerticalAlignment.Bottom;
            label.HorizontalAlignment = HorizontalAlignment.Right;

            main_grid.Children.Add(label);
            GlobalSettings.Default.Save();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            // exit
            MainWindow win = new MainWindow();
            win.Show();
            this.Close();
        }

        private void combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // change color of rectangle when combobox is changed
            ComboBox cb = sender as ComboBox;
            if(cb.Name == "Attack1box")
            {
                var c = Attack1box.SelectedItem.ToString();

                if (c == "Default")
                {
                    Attack1rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 240 , 201, 135));
                }
                else if (c != "Custom")
                {
                    Attack1rec.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(c);
                }
            }
            if (cb.Name == "Attack2box")
            {
                var c = Attack2box.SelectedItem.ToString();

                if (c == "Default")
                {
                    Attack2rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 97, 137, 133));
                }
                else if (c != "Custom")
                {
                    Attack2rec.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(c);
                }
            }



            if (cb.Name == "Attack3box")
            {
                var c = Attack3box.SelectedItem.ToString();

                if (c == "Default")
                {
                    Attack3rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 149, 125, 149));
                }
                else if (c != "Custom")
                {
                    Attack3rec.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(c);
                }
            }

            if (cb.Name == "Attack4box")
            {
                var c = Attack4box.SelectedItem.ToString();

                if (c == "Default")
                {
                    Attack4rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 239, 94, 67));
                }
                else if (c != "Custom")
                {
                    Attack4rec.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(c);
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // save button
            Savecurrentcolors();
        }

        // methods to handle when the text is changed in the textboxes

        private void Attack4name_TextChanged(object sender, TextChangedEventArgs e)
        {
           
            GlobalSettings.Default.Attack4name = Attack4name.Text;
        }

        private void Attack3name_TextChanged(object sender, TextChangedEventArgs e)
        {
            GlobalSettings.Default.Attack3name = Attack3name.Text;
        }

        private void Attack2name_TextChanged(object sender, TextChangedEventArgs e)
        {
            GlobalSettings.Default.Attack2name = Attack2name.Text;
        }

        private void Attack1name_TextChanged(object sender, TextChangedEventArgs e)
        {
            GlobalSettings.Default.Attack1name = Attack1name.Text;
        }
    }
}
