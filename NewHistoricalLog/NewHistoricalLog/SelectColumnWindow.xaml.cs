using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;


namespace NewHistoricalLog
{
    /// <summary>
    /// Interaction logic for SelectColumnWindow.xaml
    /// </summary>
    public partial class SelectColumnWindow : ThemedWindow
    {
        public bool[] Fields { get; set; } = new bool[] { true, true, true, true, true, true, true };
        public SelectColumnWindow()
        {
            InitializeComponent();
            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top + Math.Abs(Height - Service.Height) / 2;
                Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left + Math.Abs(Width - Service.Width) / 2;
            }
            else
            {
                Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top + Math.Abs(Height - Service.Height) / 2;
                Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left + Math.Abs(Width - Service.Width) / 2;
            }
        }

        private void CheckedChange(object sender, RoutedEventArgs e)
        {
            Fields = new bool[] { date.IsChecked.Value, priority.IsChecked.Value, kvit.IsChecked.Value, text.IsChecked.Value, user.IsChecked.Value, source.IsChecked.Value, value.IsChecked.Value };
        }

        private void ThemedWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void ThemedWindow_LocationChanged(object sender, EventArgs e)
        {
            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top + Math.Abs(Height - Service.Height) / 2;
                Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left + Math.Abs(Width - Service.Width) / 2;
            }
            else
            {
                Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top + Math.Abs(Height - Service.Height) / 2;
                Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left + Math.Abs(Width - Service.Width) / 2;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Fields = new bool[] { true, true, true, true, true, true, true };
            Close();
        }
    }
}
