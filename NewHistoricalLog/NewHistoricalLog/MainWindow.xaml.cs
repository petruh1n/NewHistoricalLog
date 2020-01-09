using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using DevExpress.XtraPrinting;
using System.Threading;
using DevExpress.Xpf.Editors.Settings;
using System.IO;
using NLog;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Bars;
using DevExpress.Data.Filtering;
using DatabaseToolBox;
using System.Windows.Interop;
using System.Data.SqlClient;
using System.Windows.Data;
using System.Globalization;

namespace NewHistoricalLog
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : ThemedWindow
	{
        #region Служебный переменные
        Logger logger = LogManager.GetCurrentClassLogger();
        static Logger slogger = LogManager.GetCurrentClassLogger();
        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        Thread LoadMessagesThread;
        Thread SaveMessagesThread;
        bool created = false;
        bool cleared = false;
        Thread hidePrintThread;
        #endregion

        static MainWindow()
        {
            EditorLocalizer.Active = new EditorLocalizerEx();
        }

		public MainWindow()
		{

			InitializeComponent();
            ni.Icon = new System.Drawing.Icon("Msg.ico");
            ni.Visible = false;
            ni.Text = "Журнал исторических сообщений";
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    GetUser();
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            #region Чтение настроек приложения
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SEMHistory\\Config.xml"))
                Service.ReadSettings();
            else
                Service.GenerateXMLConfig();
            #endregion

            #region Чтение ключей
            if (Environment.GetCommandLineArgs() != null)
            {
                try
                {
                    for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++)
                    {

                        if (Environment.GetCommandLineArgs()[i].ToUpper().Contains("MONITOR"))
                        {
                            Service.Monitor = Convert.ToInt32(Environment.GetCommandLineArgs()[i].Remove(0, Environment.GetCommandLineArgs()[i].IndexOf("_") + 1));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(String.Format("Ошибка при чтении ключей: {0}", ex.Message));
                    Service.Monitor = 0;
                }
            }
            #endregion

            #region Позиционирование окна приложения
            PosWindow();

            #endregion

            
            this.Loaded += OnLoad;
		}
        public static void PosWindow()
        {
            (SingleInstanceApplication.Current.MainWindow as MainWindow).Width = Service.Width;
            (SingleInstanceApplication.Current.MainWindow as MainWindow).Height = Service.Height;

            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                (SingleInstanceApplication.Current.MainWindow as MainWindow).Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top;
                (SingleInstanceApplication.Current.MainWindow as MainWindow).Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left;
            }
            else
            {
                (SingleInstanceApplication.Current.MainWindow as MainWindow).Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top;
                (SingleInstanceApplication.Current.MainWindow as MainWindow).Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left;
            }
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WINAPI.WndProc);
        }
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            //Скрыли панельку с текстовыми фильтрами
            filterTextRow.Height = new GridLength(0);
            //Скрыли панель с подсистемами
            subSystemsColumn.Width = new GridLength(0);
            //В комбобоксе текстового фильтра поставили "Сообщение" 
			filterBox.SelectedItem = filterBox.Items[0];
            //Число строк в гриде равно тому, что в настройках
            messageView.PageSize = Service.CountLines;
            //устанавливаем промежуток времени - один час назад от текущего времени
            startDate.EditValue = DateTime.Now.AddHours(-1);
            endDate.EditValue = DateTime.Now;
            //в случае если заполнен адрес на сервере ДМЗ, то делаем доступным соответствующий пункт в меню
            if (String.IsNullOrEmpty(Service.DmzPath))
                sendToDmz.IsEnabled = false;
            else
                sendToDmz.IsEnabled = true;
            var systems = ScanForSystems();
            treeView.ItemsSource = FillListView(systems);
            GetUser();            
            messageGrid.Columns["Date"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
            LoadMessagesThread = new Thread(LoadMessagesMethod) { IsBackground = true };
            LoadMessagesThread.Start();
            messageGrid.RefreshData();
            WindowState = WindowState.Minimized;
        }

        public static void GetUser()
        {
            try
            {
                SqlConnection connection = new SqlConnection();
                //connection.ConnectionString = Service.SqlConnectionString;
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = string.Format(@"SELECT Accounts.GroupID FROM Accounts INNER JOIN CurrentUser ON CAST(Accounts.Login AS VARCHAR) = CAST(CurrentUser.Login AS VARCHAR)
                                                    WHERE CurrentUser.Place = 'АРМ: {0}'", Environment.MachineName); 
                SqlDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    Service.IsAdminMode = Convert.ToInt32(reader["GroupId"]) == 1;
                }
                reader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                slogger.Error("Ошибка получения группы пользователя: {0}", ex.Message);
                Service.IsAdminMode = false;
            }
        }

        #region Обработчики событий контролов       

        private void PrintClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            SelectColumnWindow wind = new SelectColumnWindow();
            if (wind.ShowDialog() != false)
            {
                if (Service.TestDefaultPrinterConnection())
                {
                    messageGrid.Columns["Date"].AllowPrinting = wind.Fields[0];
                    messageGrid.Columns["Prior"].AllowPrinting = wind.Fields[1];
                    messageGrid.Columns["Kvited"].AllowPrinting = wind.Fields[2];
                    messageGrid.Columns["Text"].AllowPrinting = wind.Fields[3];
                    messageGrid.Columns["User"].AllowPrinting = wind.Fields[4];
                    messageGrid.Columns["Source"].AllowPrinting = wind.Fields[5];
                    messageGrid.Columns["Value"].AllowPrinting = wind.Fields[6];
                    Service.Fields = wind.Fields;
                    hidePrintThread = new Thread(PrintMethod);
                    hidePrintThread.SetApartmentState(ApartmentState.STA);
                    hidePrintThread.Start();
                }
                else
                {
                    DXMessageBox.Show("Принтер, установленный в системе по умолчанию, занят или недоступен!", "Принтер недоступен", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }    

        void PrintMethod()
        {
            CriteriaOperator co = null;
            List<MessageGridContent> mess = Service.Messages.ToList();
            Dispatcher.Invoke(() => co = messageGrid.FilterCriteria);
            HiddenPrintWindow window = new HiddenPrintWindow();
            try
            {
                window.MessagesToPrint = mess;
                window.CriteriaOperator = co;
                Service.Printing = true;
                window.ShowDialog();                
            }
            catch(Exception ex)
            {
                window.Close();
                logger.Error("Ошибка печати: {0}", ex.Message);
            }
        }       

        void ClearFilters()
        {
            filterText.Text = "";
            messageGrid.FilterCriteria = null;
            greenPriority.IsChecked = true;
            grayPriority.IsChecked = true;
            yellowPriority.IsChecked = true;
            redPriority.IsChecked = true;
            Service.PriorityFilterPhrase = "";
            Service.SystemsFilterPhrase = "";
            Service.TextFilterPhrase = "";
            foreach (TreeViewItem items in treeView.Items)
            {
                items.FontWeight = FontWeights.Normal;
                if (items.HasItems)
                {
                    foreach (TreeViewItem subitem in items.Items)
                    {
                        subitem.FontWeight = FontWeights.Normal;
                    }
                }
            }
            foreach(var fc in Service.SubSystemsFiltersList)
            {
                fc.Selected = false;
            }
        }

        private void ClearTextFilterClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            ClearFilters();
        }

        private void RefreshClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            if(Service.EndDate>Service.StartDate)
            {
                LoadMessagesThread = new Thread(LoadMessagesMethod) { IsBackground = true };
                DXSplashScreen.Show<AwaitScreen>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(this));
                DXSplashScreen.SetState(string.Format("Получение сообщений с {0} по {1}", Service.StartDate, Service.EndDate));
                LoadMessagesThread.Start();

            }
            else
            {
                MessageBox.Show("Начало выборки должно быть раньше ее завершения!", "Некорректный промежуток времени!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            try
            {               
                SelectColumnWindow wind = new SelectColumnWindow();
                wind.Owner = this;
                if (wind.ShowDialog() != false)
                {
                    messageGrid.Columns["Date"].AllowPrinting = wind.Fields[0];
                    messageGrid.Columns["Prior"].AllowPrinting = wind.Fields[1];
                    messageGrid.Columns["Kvited"].AllowPrinting = wind.Fields[2];
                    messageGrid.Columns["Text"].AllowPrinting = wind.Fields[3];
                    messageGrid.Columns["User"].AllowPrinting = wind.Fields[4];
                    messageGrid.Columns["Source"].AllowPrinting = wind.Fields[5];
                    messageGrid.Columns["Value"].AllowPrinting = wind.Fields[6];
                    DXSplashScreen.Show<AwaitScreen>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(this));
                    DXSplashScreen.SetState("Сохранение журнала");
                    DXSplashScreen.Progress(0);
                    SaveMessagesThread = new Thread(SaveMethod) { IsBackground = true };
                    SaveMessagesThread.ApartmentState = ApartmentState.STA;
                    SaveMessagesThread.Start();
                }                    
            }
            catch (Exception ex)
            {
                if(DXSplashScreen.IsActive)
                    DXSplashScreen.Close();
                Dispatcher.Invoke(() => DXMessageBox.Show("Файл сохранить не удалось. Для подробной информации см. лог приложения.", "Ошибка при сохранении файла!", MessageBoxButton.OK, MessageBoxImage.Error));
                logger.Error(String.Format("Ошибка при сохранении файла: {0}", ex.Message));
            }
        }

        void SaveMethod()
        {
            try
            {
                Service.IsOperating = false;
                if (!Directory.Exists(Service.SavePath))
                    Directory.CreateDirectory(Service.SavePath);
                string fileName = String.Format("Сообщения с {0} по {1}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"));
                int counter = 1;
                while (File.Exists(String.Format("{0}\\{1}", Service.SavePath, fileName)))
                {
                    fileName = String.Format("Сообщения с {0} по {1}_{2}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"), counter);
                    counter++;
                }
                PrintableControlLink link = null;
                Dispatcher.Invoke(() => link = new PrintableControlLink(messageGrid.View));
                Dispatcher.Invoke(() => link.PrintingSystem.ExportOptions.Xls.TextExportMode = TextExportMode.Text);
                //((IPrintingSystem)link.PrintingSystem).AutoFitToPagesWidth = 1;
                //link.PrintingSystem.ContinuousPageNumbering = true;

                Dispatcher.Invoke(() =>
                {
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
                });
                

                try
                {
                    Dispatcher.Invoke(() => link.PrintingSystem.ProgressReflector.PositionChanged += ProgressReflector_PositionChanged);
                }
                finally
                {
                    Dispatcher.Invoke(() => link.PrintingSystem.ResetProgressReflector());
                }
                if (DXSplashScreen.IsActive)
                {
                    DXSplashScreen.SetState("Создание файла");
                }
                SingleInstanceApplication.Current.Dispatcher.Invoke(() => link.CreateDocument(true));
                SingleInstanceApplication.Current.Dispatcher.Invoke(() => link.CreateDocumentFinished += (o, ee) =>
                {
                    link.PrintingSystem.ProgressReflector.MaximizeRange();
                    link.ExportToPdf(String.Format("{0}\\{1}", Service.SavePath, fileName));
                });
            }
            catch(Exception ex)
            {
                Dispatcher.Invoke(() => DXMessageBox.Show("Файл сохранить не удалось. Для подробной информации см. лог приложения.", "Ошибка при сохранении файла!", MessageBoxButton.OK, MessageBoxImage.Error));
                logger.Error(String.Format("Ошибка при сохранении файла: {0}", ex.Message));
            }
            
        }

        private void ProgressReflector_PositionChanged(object sender, EventArgs e)
        {
            if(DXSplashScreen.IsActive)
            {
                DXSplashScreen.Progress((sender as ProgressReflector).Position, (sender as ProgressReflector).Maximum);
            }
            
            if((sender as ProgressReflector).Position ==  (sender as ProgressReflector).Maximum)
            {
                if(created)
                {
                    Service.IsOperating = true;
                    if (DXSplashScreen.IsActive)
                        DXSplashScreen.Close();
                    created = false;
                    Dispatcher.Invoke(() => DXMessageBox.Show("Файл сохранен!", "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information));
                }
                else
                {
                    created = true;
                    if(DXSplashScreen.IsActive)
                    {
                        DXSplashScreen.SetState("Сохранение файла");
                    }
                }  
                
            }
        }

        private void ExitClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            Close();
        }

        private void StartDateChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            Service.StartDate = Convert.ToDateTime(startDate.EditValue);
        }

        private void EndDateChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            Service.EndDate = Convert.ToDateTime(endDate.EditValue);
        }

        private void LaunchFilterClick(object sender, RoutedEventArgs e)
        {
            Service.TextFilterPhrase = string.IsNullOrEmpty(filterText.Text)?"":string.Format("Contains([{0}], '{1}')", Service.FilterField, filterText.Text);
            ChangeFilterCriteria();
		}

        private void FilterColumnChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            switch(filterBox.EditValue.ToString())
            {
                case "Сообщение":
                    Service.FilterField = "Text";
                    break;
                case "Источник":
                    Service.FilterField = "Source";
                    break;
                case "Пользователь":
                    Service.FilterField = "User";
                    break;
                case "Значение":
                    Service.FilterField = "Value";
                    break;
            }
        }
		private void RefreshButtonClick(object sender, RoutedEventArgs e)
		{
            if (Service.EndDate > Service.StartDate)
            {
                LoadMessagesThread = new Thread(LoadMessagesMethod) { IsBackground = true };
                DXSplashScreen.Show<AwaitScreen>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(this));
                DXSplashScreen.SetState(string.Format("Получение сообщений с {0} по {1}",Service.StartDate, Service.EndDate));
                LoadMessagesThread.Start();
                
            }
            else
            {
                MessageBox.Show("Начало выборки должно быть раньше ее завершения!", "Некорректный промежуток времени!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) child = GetVisualChild<T>(v);
                if (child != null) break;
            }
            return child;
        }

        private void StartSearchClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            messageView.ShowSearchPanel(true);
            if(messageView.SearchControl==null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    GetVisualChild<TextBox>(messageView.SearchControl).PreviewMouseLeftButtonDown += MainWindow_PreviewMouseLeftButtonDown; ;
                }), DispatcherPriority.Loaded);
            }
        }

        private void MainWindow_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Service.KeyboardNeeded)
            {
                OnScreenKeyboard keyboard = new OnScreenKeyboard();
                keyboard.FormClosed += delegate (object send, System.Windows.Forms.FormClosedEventArgs args)
                {
                    (sender as TextBox).Text = keyboard.GetText() == "" ? (sender as TextBox).Text : keyboard.GetText();
                };
                keyboard.TopMost = true;
                keyboard.ShowDialog();
            }
        }

        private void SaveToClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            ExpWindow wind = new ExpWindow();
            wind.Owner = this;
            if (wind.ShowDialog().Value)
            {
                if (!string.IsNullOrEmpty(wind.Path))
                {
                    try
                    {
                        SelectColumnWindow wind1 = new SelectColumnWindow();
                        wind1.Owner = this;
                        wind1.Path = wind.Path;
                        if (wind1.ShowDialog() != false)
                        {
                            messageGrid.Columns["Date"].AllowPrinting = wind1.Fields[0];
                            messageGrid.Columns["Prior"].AllowPrinting = wind1.Fields[1];
                            messageGrid.Columns["Kvited"].AllowPrinting = wind1.Fields[2];
                            messageGrid.Columns["Text"].AllowPrinting = wind1.Fields[3];
                            messageGrid.Columns["User"].AllowPrinting = wind1.Fields[4];
                            messageGrid.Columns["Source"].AllowPrinting = wind1.Fields[5];
                            messageGrid.Columns["Value"].AllowPrinting = wind1.Fields[6];
                            DXSplashScreen.Show<AwaitScreen>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(this));
                            DXSplashScreen.SetState("Сохранение журнала");
                            DXSplashScreen.Progress(0);
                            SaveMessagesThread = new Thread(new ParameterizedThreadStart(SaveToMethod)) { IsBackground = true };
                            SaveMessagesThread.Start(wind.Path);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (DXSplashScreen.IsActive)
                            DXSplashScreen.Close();
                        Dispatcher.Invoke(() => DXMessageBox.Show("Файл сохранить не удалось. Для подробной информации см. лог приложения.", "Ошибка при сохранении файла!", MessageBoxButton.OK, MessageBoxImage.Error));
                        logger.Error(String.Format("Ошибка при сохранении файла: {0}", ex.Message));
                    } 
                }
            }
        }

        void SaveToMethod(object path)
        {
            try
            {
                Service.IsOperating = false;
                string fileName = String.Format("Сообщения с {0} по {1}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"));
                int counter = 1;
                while (File.Exists(String.Format("{0}\\{1}", (string)path, fileName)))
                {
                    fileName = String.Format("Сообщения с {0} по {1}_{2}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"), counter);
                    counter++;
                }
                PrintableControlLink link = null;
                Dispatcher.Invoke(() => link = new PrintableControlLink(messageGrid.View));
                Dispatcher.Invoke(() => link.PrintingSystem.ExportOptions.Xls.TextExportMode = TextExportMode.Text);
                //((IPrintingSystem)link.PrintingSystem).AutoFitToPagesWidth = 1;
                //link.PrintingSystem.ContinuousPageNumbering = true;

                Dispatcher.Invoke(() =>
                {
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
                });


                try
                {
                    Dispatcher.Invoke(() => link.PrintingSystem.ProgressReflector.PositionChanged += ProgressReflector_PositionChanged);
                }
                finally
                {
                    Dispatcher.Invoke(() => link.PrintingSystem.ResetProgressReflector());
                }
                if (DXSplashScreen.IsActive)
                {
                    DXSplashScreen.SetState("Создание файла");
                }
                Dispatcher.Invoke(() => link.CreateDocument(true));
                Dispatcher.Invoke(() => link.CreateDocumentFinished += (o, ee) => {
                    link.PrintingSystem.ProgressReflector.MaximizeRange();
                    link.ExportToPdf(String.Format("{0}\\{1}", (string)path, fileName));
                });
            }
            catch(Exception ex)
            {
                if (DXSplashScreen.IsActive)
                    DXSplashScreen.Close();
                Dispatcher.Invoke(()=>DXMessageBox.Show("Файл сохранить не удалось. Для подробной информации см. лог приложения.", "Ошибка при сохранении файла!", MessageBoxButton.OK, MessageBoxImage.Error));
                logger.Error(String.Format("Ошибка при сохранении файла: {0}", ex.Message));
            }
            
        }

        private void AboutClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            About wind = new About();
            wind.Owner = this;
            wind.Show();
        }

        private void OnSpecialKeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key==System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }

        private void SendToDmz(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            try
            {
                SelectColumnWindow wind1 = new SelectColumnWindow();
                wind1.Owner = this;
                wind1.Path = Service.DmzPath;
                if (wind1.ShowDialog() != false)
                {
                    messageGrid.Columns["Date"].AllowPrinting = wind1.Fields[0];
                    messageGrid.Columns["Prior"].AllowPrinting = wind1.Fields[1];
                    messageGrid.Columns["Kvited"].AllowPrinting = wind1.Fields[2];
                    messageGrid.Columns["Text"].AllowPrinting = wind1.Fields[3];
                    messageGrid.Columns["User"].AllowPrinting = wind1.Fields[4];
                    messageGrid.Columns["Source"].AllowPrinting = wind1.Fields[5];
                    messageGrid.Columns["Value"].AllowPrinting = wind1.Fields[6];
                    DXSplashScreen.Show<AwaitScreen>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(this));
                    DXSplashScreen.SetState("Сохранение журнала");
                    DXSplashScreen.Progress(0);
                    SaveMessagesThread = new Thread(SendToDMZMethod) { IsBackground = true };
                    SaveMessagesThread.Start();
                }                
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => DXMessageBox.Show("Файл сохранить не удалось. Для подробной информации см. лог приложения.", "Ошибка при сохранении файла!", MessageBoxButton.OK, MessageBoxImage.Error));
                logger.Error(String.Format("Ошибка при сохранении файла: {0}", ex.Message));
            }
        }
        #endregion

        void SendToDMZMethod()
        {
            try
            {
                Service.IsOperating = false;
                string fileName = String.Format("Сообщения с {0} по {1}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"));
                int counter = 1;
                while (File.Exists(String.Format("{0}\\{1}", Service.DmzPath, fileName)))
                {
                    fileName = String.Format("Сообщения с {0} по {1}_{2}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"), counter);
                    counter++;
                }
                PrintableControlLink link = null;
                Dispatcher.Invoke(() => link = new PrintableControlLink(messageGrid.View));
                Dispatcher.Invoke(() => link.PrintingSystem.ExportOptions.Xls.TextExportMode = TextExportMode.Text);
                //((IPrintingSystem)link.PrintingSystem).AutoFitToPagesWidth = 1;
                //link.PrintingSystem.ContinuousPageNumbering = true;

                Dispatcher.Invoke(() =>
                {
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
                });


                try
                {
                    Dispatcher.Invoke(() => link.PrintingSystem.ProgressReflector.PositionChanged += ProgressReflector_PositionChanged);
                }
                finally
                {
                    Dispatcher.Invoke(() => link.PrintingSystem.ResetProgressReflector());
                }
                if (DXSplashScreen.IsActive)
                {
                    DXSplashScreen.SetState("Создание файла");
                }
                Dispatcher.Invoke(() => link.CreateDocument(true));
                Dispatcher.Invoke(() => link.CreateDocumentFinished += (o, ee) => {
                    try
                    {
                        link.PrintingSystem.ProgressReflector.MaximizeRange();
                        link.ExportToPdf(String.Format("{0}\\{1}", Service.DmzPath, fileName));
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Ошибка экспорта на ДМЗ: {0}", ex.Message);
                        if (DXSplashScreen.IsActive)
                        {
                            DXSplashScreen.Close();
                        }
                        Dispatcher.Invoke(() => DXMessageBox.Show("Файл сохранить не удалось. Для подробной информации см. лог приложения.", "Ошибка при сохранении файла!", MessageBoxButton.OK, MessageBoxImage.Error));
                        Service.IsOperating = true;
                    }
                });
                Service.IsOperating = true;
            }
            catch(Exception ex)
            {
                Service.IsOperating = true;
                Dispatcher.Invoke(() => DXMessageBox.Show("Файл сохранить не удалось. Для подробной информации см. лог приложения.", "Ошибка при сохранении файла!", MessageBoxButton.OK, MessageBoxImage.Error));
                logger.Error(String.Format("Ошибка при сохранении файла: {0}", ex.Message));
            }
            
        }

        private void filterText_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(Service.KeyboardNeeded)
            {
                OnScreenKeyboard keyboard = new OnScreenKeyboard();
                keyboard.FormClosed += delegate (object send, System.Windows.Forms.FormClosedEventArgs args)
                {
                    (sender as TextEdit).Text = keyboard.GetText() == "" ? (sender as TextEdit).Text : keyboard.GetText();
                };
                keyboard.TopMost = true;
                keyboard.ShowDialog();
            }
        }

        //private void OnSettingsApply(object sender, EventArgs e)
        //{
        //    if (Service.GridTextWrapping)
        //        messageGrid.Columns["Text"].EditSettings = new TextEditSettings() { TextWrapping = TextWrapping.Wrap };
        //    else
        //    {
        //        messageGrid.Columns["Text"].EditSettings = new TextEditSettings() { TextWrapping = TextWrapping.NoWrap };
        //    }
        //    messageView.PageSize = Service.CountLines;
        //}

        private void FilterPriorityChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                Service.Priorities = new bool[] { grayPriority.IsChecked.Value, greenPriority.IsChecked.Value, yellowPriority.IsChecked.Value, redPriority.IsChecked.Value };
            }
            catch
            {
                Service.Priorities = new bool[] { true, true, true, true };
            }

            for (int i = 0; i < Service.Priorities.Length; i++)
            {
                ///если надо отключить сообщения определенного типа
                if (!Service.Priorities[i])
                {
                    //если строка не пустая
                    if (!String.IsNullOrEmpty(Service.PriorityFilterPhrase))
                    {
                        //если строка не содержит фильтра "Type <> приоритет" 
                        if (!Service.PriorityFilterPhrase.Contains(String.Format("[Type] <> {0}", i + 1)))
                            //добавляем в конец строки фильтр "Type <> приоритет" 
                            Service.PriorityFilterPhrase= String.Format("{0} AND [Type] <> {1}", Service.PriorityFilterPhrase, i + 1);
                    }
                    //если строка пустая
                    else
                        //добавляем в конец строки фильтр "Type <> приоритет" 
                        Service.PriorityFilterPhrase = String.Format("[Type] <> {0}", i + 1);

                }
                //если надо включить сообщения определнного проиоритета 
                else
                {
                    //если строка не пустая
                    if (!String.IsNullOrEmpty(Service.PriorityFilterPhrase))
                    {
                        //если строка содержит фильтр "Type <> приоритет" 
                        if (Service.PriorityFilterPhrase.Contains(String.Format("[Type] <> {0}", i + 1)))
                        {
                            //если условие первое и единственное
                            if (Service.PriorityFilterPhrase.IndexOf(String.Format("[Type] <> {0}", i + 1)) == 0 && Service.PriorityFilterPhrase.Length == String.Format("[Type] <> {0}", i + 1).Length)
                            {
                                //очищаем строку с условием
                                Service.PriorityFilterPhrase = "";
                            }
                            //если условие первое, но не единственное
                            else if (Service.PriorityFilterPhrase.IndexOf(String.Format("[Type] <> {0}", i + 1)) == 0)
                            {
                                Service.PriorityFilterPhrase = Service.PriorityFilterPhrase.Remove(0, String.Format("[Type] <> {0} AND ", i + 1).Length);
                            }
                            //если условие не первое
                            else
                            {
                                Service.PriorityFilterPhrase = Service.PriorityFilterPhrase.Remove(Service.PriorityFilterPhrase.IndexOf(String.Format("[Type] <> {0}", i + 1)) - 5, String.Format("AND [Type] <> {0} ", i + 1).Length);
                            }
                        }
                    }
                }
                ChangeFilterCriteria();
            }

        }

        public List<TreeViewItem> FillListView(List<SystemsItems> list)
        {
            List<TreeViewItem> Result = new List<TreeViewItem>();
            foreach(var item in list)
            {
                if(item.Name!="")
                {
                    TreeViewItem temp = new TreeViewItem() { Header = item.Name };
                    Service.SubSystemsFiltersList.Add(new FilterClass(item.Name));
                    if (item.Children.Count > 0)
                    {
                        foreach (var subItem in FillListView(item.Children))
                        {
                            temp.Items.Add(subItem);
                        }
                    }
                    temp.MouseDoubleClick += Temp_MouseDoubleClick;
                    Result.Add(temp);
                }
            }
            return Result;
        }

        private void Temp_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           if((sender as TreeViewItem).Parent!=null)
            {
                if((sender as TreeViewItem).FontWeight==FontWeights.Bold)
                {
                    (sender as TreeViewItem).FontWeight = FontWeights.Normal;
                    FilterClass.GetByName(Service.SubSystemsFiltersList, (sender as TreeViewItem).Header.ToString()).Selected=false;
                    Service.SystemsFilterPhrase = FilterClass.GetFilterString(Service.SubSystemsFiltersList);                    
                }
                else
                {
                    (sender as TreeViewItem).FontWeight = FontWeights.Bold;
                    FilterClass.GetByName(Service.SubSystemsFiltersList, (sender as TreeViewItem).Header.ToString()).Selected = true;
                    Service.SystemsFilterPhrase = FilterClass.GetFilterString(Service.SubSystemsFiltersList);                   
                }
                //Service.SystemsFilterPhrase.Replace("  ", " ");
                //Service.SystemsFilterPhrase.Trim();
                ChangeFilterCriteria();
            }
        }

        public void ChangeFilterCriteria()
        {
            string criteria = "";
            if(!string.IsNullOrEmpty(Service.SystemsFilterPhrase))
            {
                criteria = string.Format("({0})", Service.SystemsFilterPhrase);
            }
            if(!string.IsNullOrEmpty(Service.TextFilterPhrase))
            {
                if(string.IsNullOrEmpty(criteria))
                {
                    criteria = string.Format("({0})", Service.TextFilterPhrase);
                }
                else
                {
                    criteria = string.Format("({0}) AND ({1})", criteria, Service.TextFilterPhrase);
                }
            }
            if(!string.IsNullOrEmpty(Service.PriorityFilterPhrase))
            {
                if (string.IsNullOrEmpty(criteria))
                {
                    criteria = string.Format("({0})", Service.PriorityFilterPhrase);
                }
                else
                {
                    criteria = string.Format("({0}) AND ({1})", criteria, Service.PriorityFilterPhrase);
                }
            }
            try
            {
                messageGrid.FilterCriteria = CriteriaOperator.Parse(criteria);
            }
            catch 
            {
                DXMessageBox.Show("Некорректная фраза текстового фильтра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                filterText.Clear();
            }
            
        }

        public List<SystemsItems> ScanForSystems()
        {
            try
            {
                List<SystemsItems> result = new List<SystemsItems>();
                var splitedSystems = Service.TabsForScan.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var note in splitedSystems)
                {
                    SystemsItems item = new SystemsItems();
                    var splitNote = note.Split(new char[] { '-' });
                    item.Name = splitNote[1].Trim();
                    var connection = SQL.GetSqlConnection(/*Service.SqlConnectionString*/"");
                    connection.Open();
                    var data = SQL.GetDataList(connection,
                        string.Format("SELECT ParamName FROM dbo.{0} ", splitNote[0]));
                    for(int i=0;i<data.Count;i++)
                    {
                        SystemsItems subItem = new SystemsItems() { Name = data[i].Split(new char[] { ';' })[0].Trim() };
                        item.Children.Add(subItem);
                    }
                    result.Add(item);
                }
                return result;
            }
            catch(Exception ex)
            {
                logger.Error("Ошибка при формировании массива подсистем: {0}", ex.Message);
                return new List<SystemsItems>();
            }
        }       

        private void BarCheckItem_CheckedChanged(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            if((sender as BarCheckItem).IsChecked.Value)
            {
                filterTextRow.Height = new GridLength(40);
                subSystemsColumn.Width = new GridLength(220);
            }
            else
            {
                filterTextRow.Height = new GridLength(0);
                subSystemsColumn.Width = new GridLength(0);
            }
        }

        private void ThemedWindow_StateChanged(object sender, EventArgs e)
        {
            if(WindowState==WindowState.Minimized)
            {
                ni.Visible = true;
                if (DXSplashScreen.IsActive)
                    DXSplashScreen.Close();
                Hide();
            }
            if(WindowState==WindowState.Normal)
            {
                //устанавливаем промежуток времени - один час назад от текущего времени
                if(cleared)
                {
                    startDate.EditValue = DateTime.Now.AddHours(-1);
                    endDate.EditValue = DateTime.Now;
                    Service.Messages = MessageGridContent.LoadMessages(Service.StartDate, Service.EndDate);
                    messageGrid.RefreshData();
                    cleared = false;
                }
                messageGrid.ClearSorting();
                messageGrid.SortBy(messageGrid.Columns["Date"], DevExpress.Data.ColumnSortOrder.Descending);
                GC.Collect();
            }
        }

        private void ClearMessages()
        {
            ClearFilters();
            messageGrid.ItemsSource = null;
            cleared = true;
        }

        private void ThemedWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Service.Printing)
            {
                if(DXMessageBox.Show("Идет печать. При закрытии процесс печати прервется. Действительно закрыть?","Прервать печать?",MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            e.Cancel = true;
            ClearMessages();
            if(SaveMessagesThread!=null)
            {
                if (SaveMessagesThread.IsAlive)
                    SaveMessagesThread.Abort();
            }
            if(hidePrintThread!=null)
            {
                if (hidePrintThread.IsAlive)
                {
                    hidePrintThread.Abort();
                    Service.Printing = false;
                }
                    
            }
            WindowState = WindowState.Minimized;
        }

        private void messageView_CustomRowAppearance(object sender, CustomRowAppearanceEventArgs e)
        {
            e.Result = e.ConditionalValue;
            e.Handled = true;
        }

        public void LoadMessagesMethod()
        {
            Service.IsOperating = false;
            Service.Messages = MessageGridContent.LoadMessages(Service.StartDate, Service.EndDate);
            if (DXSplashScreen.IsActive)
                DXSplashScreen.Close();
            Dispatcher.Invoke(() => messageGrid.ItemsSource = null);
            Dispatcher.Invoke(() => messageGrid.ItemsSource = Service.Messages);
            Service.IsOperating = true;
        }

        //private async void messageGrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    var info = messageView.CalcHitInfo(e.GetPosition(messageView));
        //    if (info.HitTest == TableViewHitTest.ColumnHeader && e.ClickCount > 1)
        //    {
        //        messageView.BestFitColumn(info.Column);
        //        locker = true;
        //    }
        //    else if (info.HitTest == TableViewHitTest.ColumnHeader && e.ClickCount == 1)
        //    {
        //        await Task.Delay(300);
        //        if (!locker)
        //        {
        //            foreach (GridColumn col in messageGrid.Columns)
        //            {
        //                if (info.Column != col)
        //                    col.SortOrder = DevExpress.Data.ColumnSortOrder.None;
        //            }
        //            messageGrid.SortBy(info.Column, info.Column.SortOrder == DevExpress.Data.ColumnSortOrder.Ascending ? DevExpress.Data.ColumnSortOrder.Descending : DevExpress.Data.ColumnSortOrder.Ascending);
        //        }
        //        locker = false;
        //    }
        //}
        private void messageView_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var info = messageView.CalcHitInfo(e.GetPosition(messageView));
            if (info.HitTest == TableViewHitTest.RowCell)
            {
                messageView.BestFitColumn(info.Column);
            }
        }

        private void SettingsClick(object sender, ItemClickEventArgs e)
        {
            Settings wind = new Settings();
            wind.Owner = this;
            if(wind.ShowDialog()==true)
            {
                if (Service.GridTextWrapping)
                    messageGrid.Columns["Text"].EditSettings = new TextEditSettings() { TextWrapping = TextWrapping.Wrap };
                else
                {
                    messageGrid.Columns["Text"].EditSettings = new TextEditSettings() { TextWrapping = TextWrapping.NoWrap };
                }
                messageView.PageSize = Service.CountLines;
            }
        }

        private void InfoClick(object sender, ItemClickEventArgs e)
        {
            InfoWindow window = new InfoWindow();
            window.Owner = this;
            window.ShowDialog();
        }
    }
    public class EditorLocalizerEx : EditorLocalizer
    {
        protected override void PopulateStringTable()
        {
            base.PopulateStringTable();
            this.AddString(EditorStringId.LastPage, "Последняя страница");
            this.AddString(EditorStringId.NextPage, "Следующая страница");
            this.AddString(EditorStringId.FirstPage, "Первая страница");
            this.AddString(EditorStringId.PrevPage, "Предыдущая страница");
            this.AddString(EditorStringId.LookUpSearch, "Поиск");
            this.AddString(EditorStringId.LookUpClose, "Закрыть");
            this.AddString(EditorStringId.Page, "Страница");
            this.AddString(EditorStringId.Of, "из {0}");
            this.AddString(EditorStringId.DatePickerMinutes, "мин");
            this.AddString(EditorStringId.DatePickerHours, "час");
            this.AddString(EditorStringId.DatePickerSeconds, "сек");
        }
    }
    public class SystemsItems
    {
        bool hasChildren = false;
        List<SystemsItems> children = new List<SystemsItems>();

        public string Name { get; set; } = "";
        public bool Selected { get; set; } = true;
        public List<SystemsItems> Children
        {
            get { return children; }
            set
            {
                children = value;
                if (children.Count > 0)
                {
                    hasChildren = true;
                }
                else
                {
                    hasChildren = false;
                }
            }
        }
        public bool HasChildren
        {
            get { return hasChildren; }
        }

    }
    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) / 3;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
