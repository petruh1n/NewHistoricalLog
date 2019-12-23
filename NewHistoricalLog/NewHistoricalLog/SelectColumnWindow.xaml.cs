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
using System.Windows.Interop;

namespace NewHistoricalLog
{
    /// <summary>
    /// Interaction logic for SelectColumnWindow.xaml
    /// </summary>
    public partial class SelectColumnWindow : ThemedWindow
    {
        public bool[] Fields { get; set; } = new bool[] { true, true, true, true, true, true, true };
        public string Path { get; set; } = "";
        public SelectColumnWindow()
        {
            InitializeComponent();
            this.Loaded += SelectColumnWindow_Loaded;            
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WINAPI.WndProc);
        }
        private void SelectColumnWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Path))
                place.Text = string.Format("Место сохранения: {0}",Path);
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
