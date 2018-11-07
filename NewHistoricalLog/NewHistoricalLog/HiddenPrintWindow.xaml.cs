using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Runtime.InteropServices;
using System.Windows.Threading;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core;
using NLog;
using DevExpress.Xpf.Grid;

namespace NewHistoricalLog
{
    /// <summary>
    /// Interaction logic for HiddenPrintWindow.xaml
    /// </summary>
    public partial class HiddenPrintWindow : Window
    {
        /// <summary>
        /// Сообщения, отправляемые на печать
        /// </summary>
        public List<MessageGridContent> MessagesToPrint { get; set; } = new List<MessageGridContent>();
        /// <summary>
        /// Фильтр для сообщений
        /// </summary>
        public CriteriaOperator CriteriaOperator { get; set; }

        Logger logger = LogManager.GetCurrentClassLogger();

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        public HiddenPrintWindow()
        {
            InitializeComponent();    
            Loaded += HiddenPrintWindow_Loaded;
        }

        private void HiddenPrintWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SetWindowPos(new System.Windows.Interop.WindowInteropHelper(this).Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                Work();
            }
            catch
            {
                Close();
            }
        }

        void Work()
        {
            try
            {
                messageGrid.ItemsSource = MessagesToPrint;
                messageGrid.FilterCriteria = CriteriaOperator;
                messageView.PrintDirect();
                Service.Printing = false;
                Close();
            }
            catch (Exception ex)
            {
                //DXMessageBox.Show("Ошибка печати: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Error("Ошибка печати: {0}", ex.Message);
            }
        }

        private void messageView_CustomRowAppearance(object sender, CustomRowAppearanceEventArgs e)
        {
            e.Result = e.ConditionalValue;
            e.Handled = true;
        }
    }
}
