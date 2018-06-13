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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : ThemedWindow
    {
        public Settings()
        {
            InitializeComponent();
            txbCountLines.Text = Service.CountLines.ToString();
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
            cbxNeedKeyboard.IsChecked = Service.KeyboardNeeded;
            cbxNeedWrapping.IsChecked = Service.GridTextWrapping;
        }

        private void ConfirmClick(object sender, RoutedEventArgs e)
        {
            Service.CountLines = Convert.ToInt32(txbCountLines.Text);
            Service.KeyboardNeeded = cbxNeedKeyboard.IsChecked.Value;
            Service.GridTextWrapping = cbxNeedWrapping.IsChecked.Value;
            Service.SaveSettings();
            this.Close();
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
