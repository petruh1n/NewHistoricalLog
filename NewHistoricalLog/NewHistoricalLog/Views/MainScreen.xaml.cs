using System;
using System.Windows;
using DevExpress.Xpf.Core;
using System.Windows.Interop;

namespace NewHistoricalLog.Views
{
    /// <summary>
    /// Interaction logic for MainScreen.xaml
    /// </summary>
    public partial class MainScreen : ThemedWindow
    {
        public MainScreen()
        {
            InitializeComponent();
            //Loaded += MainScreen_Loaded;
        }

        //private void MainScreen_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var hwnd = new WindowInteropHelper(this).Handle;
            
        //    WINAPI.SetWindowLong(hwnd, WINAPI.GWL_STYLE, WINAPI.GetWindowLong(hwnd, WINAPI.GWL_STYLE) & ~WINAPI.WS_SYSMENU);
        //}

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WINAPI.WndProc);
        }

        private void TableView_CustomRowAppearance(object sender, DevExpress.Xpf.Grid.CustomRowAppearanceEventArgs e)
        {
            e.Result = e.ConditionalValue;
            e.Handled = true;
        }
    }
}
