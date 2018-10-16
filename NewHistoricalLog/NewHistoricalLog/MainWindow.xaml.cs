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

namespace NewHistoricalLog
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : ThemedWindow
	{
        #region Служебный переменные
        Logger logger = LogManager.GetCurrentClassLogger();
        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        Thread LoadMessagesThread;
        Thread SaveMessagesThread;
        Thread printThread;
        bool created = false;
        bool cleared = false;
        static bool[] fileds;
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
            Width = Service.Width;
            Height = Service.Height;

            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                this.Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top;
                this.Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left;
            }
            else
            {
                this.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top;
                this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left;
            }

            #endregion

            
            this.Loaded += OnLoad;
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

            if (Environment.GetCommandLineArgs().Length > 0)
            {
                for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++)
                {
                    try
                    {

                        switch (Environment.GetCommandLineArgs()[i].Remove(Environment.GetCommandLineArgs()[i].IndexOf("_")).ToUpper())
                        {
                            case "USERGROUP":
                                if (Environment.GetCommandLineArgs()[i].Remove(0, Environment.GetCommandLineArgs()[i].IndexOf("_") + 1).ToUpper() == "ADMIN")
                                {
                                    Service.IsAdminMode = true;
                                }
                                else
                                {
                                    Service.IsAdminMode = false;
                                }
                                break;
                            case "MONITOR":
                                try
                                {
                                    Service.Monitor = Convert.ToInt32(Environment.GetCommandLineArgs()[i].Remove(0, Environment.GetCommandLineArgs()[i].IndexOf("_") + 1));
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(String.Format("Ошибка чтения ключа {0}: {1}", Environment.GetCommandLineArgs()[i], ex.Message));
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(String.Format("Ошибка чтения ключей: {0}", ex.Message));
                    }

                }
            }

            messageGrid.Columns["Date"].SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
            LoadMessagesThread = new Thread(LoadMessagesMethod) { IsBackground = true };
            //DXSplashScreen.Show<AwaitScreen>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(this));
            //DXSplashScreen.SetState(string.Format("Получение сообщений с {0} по {1}", Service.StartDate, Service.EndDate));
            LoadMessagesThread.Start();
            messageGrid.RefreshData();
            WindowState = WindowState.Minimized;
            //messageGrid.ItemsSource = null;
            //messageGrid.ItemsSource = messData;
        }

        #region Обработчики событий контролов
        

        private void PrintClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            SelectColumnWindow wind = new SelectColumnWindow();
            if(wind.ShowDialog()!=false)
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
                    Thread hidePrintThread = new Thread(PrintMethod);
                    hidePrintThread.SetApartmentState(ApartmentState.STA);
                    hidePrintThread.IsBackground = true;
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
            try
            {
                CriteriaOperator co=null;
                List<MessageGridContent> mess = Service.Messages.ToList();
                Dispatcher.Invoke(() => co = messageGrid.FilterCriteria);
                HiddenPrintWindow window = new HiddenPrintWindow();
                window.MessagesToPrint = mess;
                window.CriteriaOperator = co;
                Service.Printing = true;
                window.ShowDialog();                
            }
            catch(Exception ex)
            {
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
                DXSplashScreen.Show<AwaitScreen>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(this));
                DXSplashScreen.SetState("Сохранение журнала");
                DXSplashScreen.Progress(0);
                SaveMessagesThread = new Thread(SaveMethod) { IsBackground = true };
                SaveMessagesThread.Start();
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
                if (!Directory.Exists(String.Format("{0}\\SEMHistory\\Export", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments))))
                    Directory.CreateDirectory(String.Format("{0}\\SEMHistory\\Export", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)));
                string fileName = String.Format("Сообщения с {0} по {1}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"));
                int counter = 1;
                while (File.Exists(String.Format("{0}\\SEMHistory\\Export\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), fileName)))
                {
                    fileName = String.Format("Сообщения с {0} по {1}_{2}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"), counter);
                    counter++;
                }
                //messageView.ExportToPdf(String.Format("{0}\\SEMHistory\\Export\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), fileName));
                PrintableControlLink link = null;
                Dispatcher.Invoke(() => link = new PrintableControlLink(messageGrid.View));
                link.PrintingSystem.ExportOptions.Xls.TextExportMode = TextExportMode.Text;
                //var xlsExportOptions = new XlsExportOptions();
                //xlsExportOptions.ExportMode = XlsExportMode.SingleFile;
                try
                {
                    link.PrintingSystem.ProgressReflector.PositionChanged += ProgressReflector_PositionChanged; ;
                }
                finally
                {
                    link.PrintingSystem.ResetProgressReflector();
                }
                if (DXSplashScreen.IsActive)
                {
                    DXSplashScreen.SetState("Создание файла");
                }
                Dispatcher.Invoke(() => link.CreateDocument(true));
                Dispatcher.Invoke(() => link.CreateDocumentFinished += (o, ee) => {
                    link.PrintingSystem.ProgressReflector.MaximizeRange();
                    link.ExportToPdf(String.Format("{0}\\SEMHistory\\Export\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), fileName));
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

        private void WindowMoved(object sender, EventArgs e)
        {
            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                this.Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top;
                this.Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left;
            }
            else
            {
                this.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top;
                this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left;
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
            TextBox a = null;
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
            if (wind.ShowDialog().Value)
            {
                try
                {
                    DXSplashScreen.Show<AwaitScreen>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(this));
                    DXSplashScreen.SetState("Сохранение журнала");
                    DXSplashScreen.Progress(0);
                    SaveMessagesThread = new Thread(new ParameterizedThreadStart(SaveToMethod)) { IsBackground = true };
                    SaveMessagesThread.Start(wind.Path);
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
                link.PrintingSystem.ExportOptions.Xls.TextExportMode = TextExportMode.Text;
                try
                {
                    link.PrintingSystem.ProgressReflector.PositionChanged += ProgressReflector_PositionChanged; ;
                }
                finally
                {
                    link.PrintingSystem.ResetProgressReflector();
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
                DXSplashScreen.Show<AwaitScreen>(WindowStartupLocation.CenterOwner, new SplashScreenOwner(this));
                DXSplashScreen.SetState("Сохранение журнала");
                DXSplashScreen.Progress(0);
                SaveMessagesThread = new Thread(SendToDMZMethod) { IsBackground = true };
                SaveMessagesThread.Start();
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
                link.PrintingSystem.ExportOptions.Xls.TextExportMode = TextExportMode.Text;
                //var xlsExportOptions = new XlsExportOptions();
                //xlsExportOptions.ExportMode = XlsExportMode.SingleFile;
                try
                {
                    link.PrintingSystem.ProgressReflector.PositionChanged += ProgressReflector_PositionChanged; ;
                }
                finally
                {
                    link.PrintingSystem.ResetProgressReflector();
                }
                if (DXSplashScreen.IsActive)
                {
                    DXSplashScreen.SetState("Создание файла");
                }
                Dispatcher.Invoke(() => link.CreateDocument(true));
                Dispatcher.Invoke(() => link.CreateDocumentFinished += (o, ee) => {
                    link.PrintingSystem.ProgressReflector.MaximizeRange();
                    link.ExportToPdf(String.Format("{0}\\{1}", Service.DmzPath, fileName));
                });
            }
            catch(Exception ex)
            {
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

        private void OnSettingsApply(object sender, EventArgs e)
        {
            if (Service.GridTextWrapping)
                messageGrid.Columns["Text"].EditSettings = new TextEditSettings() { TextWrapping = TextWrapping.Wrap };
            else
            {
                messageGrid.Columns["Text"].EditSettings = new TextEditSettings() { TextWrapping = TextWrapping.NoWrap };
            }
            messageView.PageSize = Service.CountLines;
        }

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
                TreeViewItem temp = new TreeViewItem() { Header = item.Name };
                if(item.Children.Count>0)
                {
                    foreach(var subItem in FillListView(item.Children))
                    {
                        temp.Items.Add(subItem);
                    }
                }
                temp.MouseDoubleClick += Temp_MouseDoubleClick;
                Result.Add(temp);
                
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
                    if(Service.SystemsFilterPhrase.Contains(string.Format("Contains([Text], '{0}') Or ", (sender as TreeViewItem).Header)))
                    {
                        Service.SystemsFilterPhrase = Service.SystemsFilterPhrase.Replace(string.Format("Contains([Text], '{0}') Or ", (sender as TreeViewItem).Header), "");
                    }
                    else if (Service.SystemsFilterPhrase.Contains(string.Format("Or Contains([Text], '{0}')", (sender as TreeViewItem).Header)))
                    {
                        Service.SystemsFilterPhrase = Service.SystemsFilterPhrase.Replace(string.Format("Or Contains([Text], '{0}')", (sender as TreeViewItem).Header), "");
                    }
                    else if (Service.SystemsFilterPhrase.Contains(string.Format("Contains([Text], '{0}')", (sender as TreeViewItem).Header)))
                    {
                        Service.SystemsFilterPhrase = Service.SystemsFilterPhrase.Replace(string.Format("Contains([Text], '{0}')", (sender as TreeViewItem).Header), "");
                    }
                }
                else
                {
                    (sender as TreeViewItem).FontWeight = FontWeights.Bold;
                    //если строка фильтрация по подсистемам пустая
                    if (string.IsNullOrEmpty(Service.SystemsFilterPhrase))
                    {
                        Service.SystemsFilterPhrase = string.Format("Contains([Text], '{0}')", (sender as TreeViewItem).Header);
                    }
                    else
                    {
                        Service.SystemsFilterPhrase = string.Format("{0} Or Contains([Text], '{1}')", Service.SystemsFilterPhrase, (sender as TreeViewItem).Header);
                    }
                }
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
            messageGrid.FilterCriteria = CriteriaOperator.Parse(criteria);
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
                    var connection = SQL.GetSqlConnection(Service.ConnectionString);
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
            e.Cancel = true;
            ClearMessages();
            if(SaveMessagesThread!=null)
            {
                if (SaveMessagesThread.IsAlive)
                    SaveMessagesThread.Abort();
            }
            if(printThread!=null)
            {
                if (printThread.IsAlive)
                    printThread.Abort();
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

        private void messageView_ShowingEditor(object sender, ShowingEditorEventArgs e)
        {

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
        //public string Description { get; set; } = "";
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

        //public SystemsItems()
        //{
        //    Children.
        //}
    }
}
