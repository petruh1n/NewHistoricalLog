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
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Editors;
using DevExpress.XtraPrinting;

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

        public bool[] Fields;

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
                messageGrid.Columns["Date"].AllowPrinting = Service.Fields[0];
                messageGrid.Columns["Prior"].AllowPrinting = Service.Fields[1];
                messageGrid.Columns["Kvited"].AllowPrinting = Service.Fields[2];
                messageGrid.Columns["Text"].AllowPrinting = Service.Fields[3];
                messageGrid.Columns["User"].AllowPrinting = Service.Fields[4];
                messageGrid.Columns["Source"].AllowPrinting = Service.Fields[5];
                messageGrid.Columns["Value"].AllowPrinting = Service.Fields[6];
                messageGrid.ItemsSource = MessagesToPrint;
                messageGrid.FilterCriteria = CriteriaOperator;
                PrintableControlLink link = null;
                link = new PrintableControlLink(messageGrid.View);
                link.PrintingSystem.ExportOptions.Xls.TextExportMode = TextExportMode.Text;
                //((IPrintingSystem)link.PrintingSystem).AutoFitToPagesWidth = 1;
                //link.PrintingSystem.ContinuousPageNumbering = true;

                //Формирование заголовка
                var templateHeader = new DataTemplate();
                var controlHeader = new FrameworkElementFactory(typeof(TextEdit));
                controlHeader.SetValue(TextEdit.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                controlHeader.SetValue(TextEdit.FontSizeProperty, 25d);
                controlHeader.SetValue(TextEdit.MarginProperty, new Thickness(0));
                controlHeader.SetBinding(TextEdit.WidthProperty, new Binding("UsablePageWidth"));
                controlHeader.SetValue(TextEdit.EditValueProperty, Service.PrintTitle);

                //Формирование номеров страниц
                var templateFooter = new DataTemplate();
                var controlFooter = new FrameworkElementFactory(typeof(StackPanel));
                controlFooter.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

                var edit1 = new FrameworkElementFactory(typeof(TextEdit));
                edit1.SetBinding(TextEdit.WidthProperty, new Binding("UsablePageWidth") { Converter = new MyConverter() });
                controlFooter.AppendChild(edit1);

                var edit2 = new FrameworkElementFactory(typeof(TextEdit));
                edit2.SetBinding(TextEdit.WidthProperty, new Binding("UsablePageWidth") { Converter = new MyConverter() });
                edit2.SetValue(TextEdit.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
                edit2.SetValue(ExportSettings.TargetTypeProperty, TargetType.PageNumber);
                edit2.SetValue(PageNumberExportSettings.FormatProperty, "Страница {0} из {1}");
                edit2.SetValue(PageNumberExportSettings.KindProperty, PageNumberKind.NumberOfTotal);
                controlFooter.AppendChild(edit2);

                var edit3 = new FrameworkElementFactory(typeof(TextEdit));
                edit3.SetBinding(TextEdit.WidthProperty, new Binding("UsablePageWidth") { Converter = new MyConverter() });
                edit3.SetValue(TextEdit.HorizontalContentAlignmentProperty, HorizontalAlignment.Right);
                edit3.SetValue(TextEdit.EditValueProperty, DateTime.Now);
                controlFooter.AppendChild(edit3);

                templateHeader.VisualTree = controlHeader;
                templateFooter.VisualTree = controlFooter;

                link.PageHeaderTemplate = templateHeader;
                link.PageFooterTemplate = templateFooter;

                link.CreateDocument(true);
                link.CreateDocumentFinished += (o, ee) => {
                    try
                    {
                        //link.PrintingSystem.ProgressReflector.MaximizeRange();
                        link.PrintDirect();
                        Service.Printing = false;
                        Close();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Ошибка печати ссылки: {0}", ex.Message);
                        if (DXSplashScreen.IsActive)
                        {
                            DXSplashScreen.Close();
                        }
                        Service.Printing = false;
                        Close();
                        //Dispatcher.Invoke(() => DXMessageBox.Show("Файл сохранить не удалось. Для подробной информации см. лог приложения.", "Ошибка при сохранении файла!", MessageBoxButton.OK, MessageBoxImage.Error));
                        //Service.IsOperating = true;
                    }
                };

                
            }
            catch (Exception ex)
            {
                //DXMessageBox.Show("Ошибка печати: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Error("Ошибка печати inside: {0}", ex.Message);
                Close();
            }
        }

        private void messageView_CustomRowAppearance(object sender, CustomRowAppearanceEventArgs e)
        {
            e.Result = e.ConditionalValue;
            e.Handled = true;
        }
    }
}
