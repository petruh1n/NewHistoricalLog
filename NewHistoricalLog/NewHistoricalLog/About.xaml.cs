﻿using System;
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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : ThemedWindow
    {
        public About()
        {
            InitializeComponent();

            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top + (Service.Height - Height) / 2;
                Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left + (Service.Width - Width) / 2;
            }
            else
            {
                Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top + (Service.Height-Height)/2;
                Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left + (Service.Width-Width)/2;
            }
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

        private void ThemedWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }
    }
}
