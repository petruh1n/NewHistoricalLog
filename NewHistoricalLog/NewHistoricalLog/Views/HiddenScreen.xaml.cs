using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Printing;
using DevExpress.Xpf.Editors;
using NLog;
using DevExpress.XtraPrinting;
using System.IO;

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

        private void TableView_CustomRowAppearance(object sender, DevExpress.Xpf.Grid.CustomRowAppearanceEventArgs e)
        {
            e.Result = e.ConditionalValue;
            e.Handled = true;
        }

    private void HiddenScreen_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                WINAPI.SetWindowPos(new System.Windows.Interop.WindowInteropHelper(this).Handle, WINAPI.HWND_BOTTOM, 0, 0, 0, 0, WINAPI.SWP_NOMOVE | WINAPI.SWP_NOSIZE | WINAPI.SWP_NOACTIVATE);
                Models.MainModel.NeedProgressBar = true;
                Models.MainModel.Status = "Подготовка документа с сообщениями.";
                Models.MainModel.Progress = 0;
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
                PrintableControlLink link = new PrintableControlLink(gridControl.View);
                link.PrintingSystem.ExportOptions.Xls.TextExportMode = TextExportMode.Text;
                link.Landscape = true;
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

                link.PrintingSystem.ProgressReflector.PositionChanged += ProgressReflector_PositionChanged;
                link.CreateDocument(true);
                link.CreateDocumentFinished += (o, ee) => 
                {
                    try
                    {
                        //link.PrintingSystem.ProgressReflector.MaximizeRange();
                        if (Print)
                        {
                            try
                            {
                                Models.MainModel.Status = "Печать документа с сообщениями";
                                Models.MainModel.Indeterminant = true;
                                link.PrintDirect();
                                Models.MainModel.Indeterminant = false;
                            }
                            catch (Exception ex)
                            {
                                Models.MainModel.Indeterminant = false;
                                logger.Error("Ошибка печати: {0}", ex.Message);
                            }
                        }
                        else
                        {
                            Models.MainModel.Indeterminant = true;
                            foreach (var path in SavePath)
                            {
                                try
                                {
                                    if (!Directory.Exists(path))
                                        Directory.CreateDirectory(path);
                                    string fileName = String.Format("Сообщения с {0} по {1}.pdf", Models.MainModel.StartTime.ToString().Replace(":", "_"), Models.MainModel.EndTime.ToString().Replace(":", "_"));
                                    int counter = 1;
                                    while (File.Exists(String.Format("{0}\\{1}", path, fileName)))
                                    {
                                        fileName = String.Format("Сообщения с {0} по {1}_{2}.pdf", Models.MainModel.StartTime.ToString().Replace(":", "_"), Models.MainModel.EndTime.ToString().Replace(":", "_"), counter);
                                        counter++;
                                    }
                                    Models.MainModel.Status = string.Format("Экспорт в {0}", String.Format("{0}\\{1}", path, fileName));
                                    link.ExportToPdf(String.Format("{0}\\{1}", path, fileName));
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Ошибка экспорта в {0}: {1}", path, ex.Message);
                                }
                            }
                            Models.MainModel.Indeterminant = false;
                        }
                        Close();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Ошибка создания документа для экспорта: {0}", ex.Message);  
                        Close();
                    }
                };
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка глобального экспорта: {0}", ex.Message);
                Close();
            }
        }
        /// <summary>
        /// Обработка статуса создания документа для печати/экспорта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressReflector_PositionChanged(object sender, EventArgs e)
        {
            Models.MainModel.MaxProgress = (sender as ProgressReflector).Maximum;
            Models.MainModel.Progress = (sender as ProgressReflector).Position;
        }
    }
}
