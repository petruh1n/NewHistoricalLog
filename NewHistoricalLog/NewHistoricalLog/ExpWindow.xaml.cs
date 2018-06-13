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
using System.IO;
using DevExpress.Xpf.Core;


namespace NewHistoricalLog
{
    /// <summary>
    /// Interaction logic for ExpWindow.xaml
    /// </summary>
    public partial class ExpWindow : ThemedWindow
    {
        public string Path { get; set; } = "";
        public ExpWindow()
        {
            InitializeComponent();
            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top + Math.Abs(Height- Service.Height) / 2;
                Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left + Math.Abs( Width- Service.Width) / 2;
            }
            else
            {
                Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top + Math.Abs(Height - Service.Height) / 2;
                Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left + Math.Abs(Width - Service.Width) / 2;
            }
            this.Loaded += ExpWindow_Loaded;
        }

        private void ExpWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DriveInfo[] Devices = DriveInfo.GetDrives();
            var Pathes = (from dev in Devices
                          where dev.DriveType == DriveType.Removable
                          select dev.RootDirectory);
            devisesBox.ItemsSource = Pathes;
        }

        private void ThemedWindow_LocationChanged(object sender, EventArgs e)
        {
            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top + (Service.Height - Height) / 2;
                Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left + (Service.Width - Width) / 2;
            }
            else
            {
                Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top + (Service.Height - Height) / 2;
                Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left + (Service.Width - Width) / 2;
            }
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            Path = devisesBox.EditValue.ToString() + "\\Экспортированные сообщения";
            if(!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            DriveInfo[] Devices = DriveInfo.GetDrives();
            var Pathes = (from dev in Devices
                          where dev.DriveType == DriveType.Removable
                          select dev.RootDirectory);
            devisesBox.ItemsSource = null;
            devisesBox.ItemsSource = Pathes;
        }

        private void ThemedWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }
    }
}
