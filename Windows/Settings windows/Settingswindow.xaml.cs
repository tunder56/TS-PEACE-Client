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
        public Settingswindow()
        {
            InitializeComponent();

            getcurrentcolors();

            Attack1rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(GSettings.Default.Attack1color.A, GSettings.Default.Attack1color.R, GSettings.Default.Attack1color.G, GSettings.Default.Attack1color.B));
            Attack2rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(GSettings.Default.Attack2color.A, GSettings.Default.Attack2color.R, GSettings.Default.Attack2color.G, GSettings.Default.Attack2color.B));
            Attack3rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(GSettings.Default.Attack3color.A, GSettings.Default.Attack3color.R, GSettings.Default.Attack3color.G, GSettings.Default.Attack3color.B));
            Attack4rec.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(GSettings.Default.Attack4color.A, GSettings.Default.Attack4color.R, GSettings.Default.Attack4color.G, GSettings.Default.Attack4color.B));

            Setcombolists();
        }


        




        private void Setcombolists()
        {
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
            if(GSettings.Default.Attack1color == Color.FromArgb(255, 240, 201, 135))
            {
                Attack1box.SelectedItem = "Default";
            }
            else
            {
                Attack1box.SelectedItem = GSettings.Default.Attack1color.Name;
            }
            if (GSettings.Default.Attack2color == Color.FromArgb(255, 97, 137, 133))
            {
                Attack2box.SelectedItem = "Default";
            }
            else
            {
                Attack2box.SelectedItem = GSettings.Default.Attack2color.Name;
            }
            if (GSettings.Default.Attack3color == Color.FromArgb(255, 149, 125, 149))
            {
                Attack3box.SelectedItem = "Default";
            }
            else
            {
                Attack3box.SelectedItem = GSettings.Default.Attack3color.Name;
            }
            if (GSettings.Default.Attack4color == Color.FromArgb(255, 239, 111, 108))
            {
                Attack4box.SelectedItem = "Default";
            }
            else
            {
                Attack4box.SelectedItem = GSettings.Default.Attack4color.Name;
            }

        }

        private void Savecurrentcolors()
        {
            if(Attack1box.SelectedItem == "Default") 
            { 
                
                GSettings.Default.Attack1color = Color.FromArgb(255, 240, 201, 135);
            }
            else if (Attack1box.SelectedItem != "Custom")
            {

                GSettings.Default.Attack1color = Color.FromName(Attack1box.SelectedItem.ToString());
            }
            if (Attack2box.SelectedItem == "Default")
            {

                GSettings.Default.Attack2color = Color.FromArgb(255, 97, 137, 133);
            }
            else if (Attack2box.SelectedItem != "Custom")
            {
                GSettings.Default.Attack2color = Color.FromName(Attack2box.SelectedItem.ToString());
            }
            if (Attack3box.SelectedItem == "Default")
            {

                GSettings.Default.Attack3color = Color.FromArgb(255, 149, 125, 149);
            }
            else if (Attack3box.SelectedItem != "Custom")
            {
                GSettings.Default.Attack3color = Color.FromName(Attack3box.SelectedItem.ToString());
            }
            if (Attack4box.SelectedItem == "Default")
            {

                GSettings.Default.Attack4color = Color.FromArgb(255, 239, 111, 108);
            }
            else if (Attack4box.SelectedItem != "Custom")
            {
                GSettings.Default.Attack4color = Color.FromName(Attack4box.SelectedItem.ToString());
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
            GSettings.Default.Save();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            MainWindow win = new MainWindow();
            win.Show();
            this.Close();
        }

        private void combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            Savecurrentcolors();
        }
    }
}
