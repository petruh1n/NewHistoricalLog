using System;
using System.Windows;
using DevExpress.Xpf.Core;
using System.Windows.Interop;

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
            cbxNeedKeyboard.IsChecked = Service.KeyboardNeeded;
            cbxNeedWrapping.IsChecked = Service.GridTextWrapping;
            txbCountLines.EditValue = Service.CountLines;
            dmz.EditValue = Service.DmzPath;
            place.EditValue = Service.SavePath;
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WINAPI.WndProc);
        }
        private void ConfirmClick(object sender, RoutedEventArgs e)
        {
            if(txbCountLines.EditValue!=null)
                if(txbCountLines.EditValue.ToString()!="")
                    Service.CountLines = Convert.ToInt32(txbCountLines.EditValue);
            Service.KeyboardNeeded = cbxNeedKeyboard.IsChecked.Value;
            Service.GridTextWrapping = cbxNeedWrapping.IsChecked.Value;
            if (place.EditValue != null)
                if (place.EditValue.ToString() != "")
                    Service.SavePath = place.EditValue.ToString();
            if (dmz.EditValue != null)
                if (dmz.EditValue.ToString() != "")
                    Service.DmzPath = dmz.EditValue.ToString();
            Service.SaveSettings();
            this.DialogResult = true;
            this.Close();
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
