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
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Editors;
using NLog;
using DevExpress.XtraPrinting;

namespace NewHistoricalLog.Views
{
    /// <summary>
    /// Interaction logic for HiddenScreen.xaml
    /// </summary>
    public partial class HiddenScreen : ThemedWindow
    {
        /// <summary>
        /// Отправлять на печать
        /// </summary>
        public bool Print { get; set; } = false;
        public List<string> SavePath { get; set; } = new List<string>();

        Logger logger = LogManager.GetCurrentClassLogger();
        public HiddenScreen()
        {
            InitializeComponent();
            Loaded += HiddenScreen_Loaded;
        }

        private void messageView_CustomRowAppearance(object sender, CustomRowAppearanceEventArgs e)
        {
            e.Result = e.ConditionalValue;
            e.Handled = true;
        }

        private void HiddenScreen_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                WINAPI.SetWindowPos(new System.Windows.Interop.WindowInteropHelper(this).Handle, WINAPI.HWND_BOTTOM, 0, 0, 0, 0, WINAPI.SWP_NOMOVE | WINAPI.SWP_NOSIZE | WINAPI.SWP_NOACTIVATE);
                Work();
            }
            catch
            {
                Close();
            }
        }

        private void Work()
        {
            try
            {
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
                        Service.Printing = false;
                        Close();
                    }
                };
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка печати inside: {0}", ex.Message);
                Close();
            }
        }
    }
}
