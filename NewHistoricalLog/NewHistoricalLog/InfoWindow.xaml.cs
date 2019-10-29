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
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : ThemedWindow
    {
        public InfoWindow()
        {
            InitializeComponent();
            Closing += InfoWindow_Closing;
            Version.Text = string.Format("Версия сборки: {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        private void InfoWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Deactivated -= ThemedWindow_Deactivated;
        }

        private void ThemedWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape | e.Key == Key.Enter)
            {
                Close();
            }
        }

        private void ThemedWindow_Deactivated(object sender, EventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }
    }
}
