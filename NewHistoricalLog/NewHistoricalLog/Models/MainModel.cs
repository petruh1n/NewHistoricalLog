using NewHistoricalLog.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NewHistoricalLog.Models
{

    public class MainModel
    {
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        static void OnStaticPropertyChanged(string name)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }

        static Logger logger = LogManager.GetCurrentClassLogger();
        public static System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        static DateTime startDate = DateTime.Now.AddHours(-1);
        static DateTime endTime = DateTime.Now;
        static string filter="";
        static ObservableCollection<MessageClass> messages = new ObservableCollection<MessageClass>();
        static ObservableCollection<SubSystem> subsystems = new ObservableCollection<SubSystem>();
        static bool needProgressBar = false;
        static string status="";
        static bool whiteMessages = true;
        static bool greenMessages = true;
        static bool yellowMessages = true;
        static bool redMessages = true;
        static bool indeterminant = false;
        static int progress=0;
        static int maxProgress=100;
        static bool adminMode = false;
        static string textFilterString;
        static string filterColumnName = "Сообщение";
        static List<bool> exportColumns = Service.ColumnVisibilityList;
        static List<string> expAddresses = new List<string>();

        /// <summary>
        /// Стартовая дата выборки
        /// </summary>
        public static DateTime StartTime
        {
            get { return startDate; }
            set
            {
                startDate = value;
                OnStaticPropertyChanged("StartTime");
            }
        }
        /// <summary>
        /// Конечная дата выборки
        /// </summary>
        public static DateTime EndTime
        {
            get { return endTime; }
            set
            {
                endTime = value;
                OnStaticPropertyChanged("EndTime");
            }
        }
        /// <summary>
        /// Строковый фильтр
        /// </summary>
        public static string Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                OnStaticPropertyChanged("Filter");
            }
        }
        /// <summary>
        /// Коллекция сообщений
        /// </summary>
        public static ObservableCollection<MessageClass> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                OnStaticPropertyChanged("Messages");
            }
        }  
        /// <summary>
        /// коллекция подсистем для фильтрации
        /// </summary>
        public static ObservableCollection<SubSystem> SubSystems
        {
            get { return subsystems; }
            set
            {
                subsystems = value;
                OnStaticPropertyChanged("SubSystems");
            }
        }
        /// <summary>
        /// Флаг отображения прогресса
        /// </summary>
        public static bool NeedProgressBar
        {
            get { return needProgressBar; }
            set
            {
                needProgressBar = value;
                OnStaticPropertyChanged("NeedProgressBar");
            }
        }
        /// <summary>
        /// Статус
        /// </summary>
        public static string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnStaticPropertyChanged("Status");
            }
        }
        /// <summary>
        /// Флаг отображения сообщений с нормальнм приоритетом
        /// </summary>
        public static bool WhiteMessages
        {
            get { return whiteMessages; }
            set
            {
                whiteMessages = value;
                OnStaticPropertyChanged("WhiteMessages");
            }
        }
        /// <summary>
        /// Флаг отображения сообщений с низким приоритетом
        /// </summary>
        public static bool GreenMessages
        {
            get { return greenMessages; }
            set
            {
                greenMessages = value;
                OnStaticPropertyChanged("GreenMessages");
            }
        }
        /// <summary>
        /// Флаг отображения сообщений со средним приоритетом
        /// </summary>
        public static bool YellowMessages
        {
            get { return yellowMessages; }
            set
            {
                yellowMessages = value;
                OnStaticPropertyChanged("YellowMessages");
            }
        }
        /// <summary>
        /// Флаг отображения сообщений с высоким приоритетом
        /// </summary>
        public static bool RedMessages
        {
            get { return redMessages; }
            set
            {
                redMessages = value;
                OnStaticPropertyChanged("RedMessages");
            }
        }
        /// <summary>
        /// Флаг отображения прогресс-бара без конкретных значений
        /// </summary>
        public static bool Indeterminant
        {
            get { return indeterminant; }
            set
            {
                indeterminant = value;
                OnStaticPropertyChanged("Indeterminant");
            }
        }
        /// <summary>
        /// Прогресс длительной операции
        /// </summary>
        public static int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnStaticPropertyChanged("Progress");
            }
        }
        /// <summary>
        /// Максимальное значние прогресса
        /// </summary>
        public static int MaxProgress
        {
            get { return maxProgress; }
            set
            {
                maxProgress = value;
                OnStaticPropertyChanged("MaxProgress");
            }
        }
        /// <summary>
        /// Приложение в режиме администратора
        /// </summary>
        public static bool AdminMode
        {
            get { return adminMode; }
            set
            {
                adminMode = value;
                OnStaticPropertyChanged("AdminMode");
            }
        }
        /// <summary>
        /// Строка текстового фильтра
        /// </summary>
        public static string TextFilterString
        {
            get { return textFilterString; }
            set
            {
                textFilterString = value;
                OnStaticPropertyChanged("TextFilterString");
            }
        }
        /// <summary>
        /// Заголовок колонки для фильтра
        /// </summary>
        public static string FilterColumnName
        {
            get { return filterColumnName; }
            set
            {
                filterColumnName = value;
                OnStaticPropertyChanged("FilterColumnName");
            }
        }       
        /// <summary>
        /// Видимость колонок для экспортируемого документа
        /// </summary>
        public static List<bool> ExportColumns
        {
            get { return exportColumns; }
            set
            {
                exportColumns = value;
                OnStaticPropertyChanged("ExportColumns");
            }
        }
        /// <summary>
        /// Список адресов для экспорта
        /// </summary>
        public static List<string> ExpAddresses
        {
            get { return expAddresses; }
            set
            {
                expAddresses = value;
                OnStaticPropertyChanged("ExpAddresses");
            }
        }

        static MainModel()
        {
            //ni.Icon = new System.Drawing.Icon(Path.Combine(Directory.GetCurrentDirectory(), "Images\\Msg.ico"));
            ni.Icon = Properties.Resources.Msg;
            ni.Visible = false;
            ni.Text = "Журнал исторических сообщений";
            ni.DoubleClick +=
                async delegate (object sender, EventArgs args)
                {
                    await DbHelper.GetUserData();
                    if(string.IsNullOrEmpty(Filter))
                        SubSystems = await DbHelper.GetSubSystemInfo();
                    await GetMessagesAsync();
                    SingleInstanceApplication.Current.MainWindow.ShowActivated = true;
                    SingleInstanceApplication.Current.MainWindow.Show();
                    SingleInstanceApplication.Current.MainWindow.Topmost =true;
                };
            System.Windows.Forms.Timer fixControlTimer = new System.Windows.Forms.Timer();
            fixControlTimer.Interval = 1000;
            fixControlTimer.Tick += FixControlTimer_Tick;
            fixControlTimer.Start();
        }
        private static void FixControlTimer_Tick(object sender, EventArgs e)
        {
            (sender as System.Windows.Forms.Timer).Stop();
            try
            {
                if (Service.FollowFix && Proficy.iFixToolkit.Adapter2.Helper.FixIsFixRunning() == 0)
                {
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка определения состояния iFix: {0}", ex.Message);
            }
            (sender as System.Windows.Forms.Timer).Start();
        }

        /// <summary>
        /// Стартовый метод приложения
        /// </summary>
        public static async void StartApp()
        {
            Service.ReadSettings();
            ReadArgs(Environment.GetCommandLineArgs());
            ChangeWindowPosition();
            await DbHelper.GetUserData();
            SubSystems = await DbHelper.GetSubSystemInfo();
            await GetMessagesAsync();
            SingleInstanceApplication.Current.MainWindow.Close();
        }
        /// <summary>
        /// Получить сообщения
        /// </summary>
        /// <returns></returns>
        public async static Task GetMessagesAsync()
        {
            Status = string.Format("Получение сообщений за период с {0} по {1}", StartTime, EndTime);
            NeedProgressBar = true;
            Indeterminant = true;
            Messages = await DbHelper.GetMessages(StartTime, EndTime);
            Indeterminant = false;
            NeedProgressBar = false;
        } 
        /// <summary>
        /// Изменить расположение окна
        /// </summary>
        public static void ChangeWindowPosition()
        {
            if (SingleInstanceApplication.Current.Dispatcher.Invoke(() => SingleInstanceApplication.Current.MainWindow != null))
            {
                SingleInstanceApplication.Current.Dispatcher.Invoke(() => SingleInstanceApplication.Current.MainWindow.Width = Service.Width);
                SingleInstanceApplication.Current.Dispatcher.Invoke(() => SingleInstanceApplication.Current.MainWindow.Height = Service.Height);

                if (Service.Monitor < System.Windows.Forms.Screen.AllScreens.Length)
                {
                    SingleInstanceApplication.Current.Dispatcher.Invoke(() =>
                        SingleInstanceApplication.Current.MainWindow.Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top);
                    SingleInstanceApplication.Current.Dispatcher.Invoke(() =>
                        SingleInstanceApplication.Current.MainWindow.Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left);
                }
                else
                {
                    SingleInstanceApplication.Current.Dispatcher.Invoke(() =>
                        SingleInstanceApplication.Current.MainWindow.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top);
                    SingleInstanceApplication.Current.Dispatcher.Invoke(() =>
                        SingleInstanceApplication.Current.MainWindow.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left);
                }
            }
        }
        /// <summary>
        /// Прочитать аргументы
        /// </summary>
        /// <param name="args"></param>
        public static void ReadArgs(string[] args)
        {
            if (args != null)
            {
                try
                {
                    for (int i = 0; i < args.Length; i++)
                    {

                        if (args[i].ToUpper().Contains("MONITOR"))
                        {
                            Service.Monitor = Convert.ToInt32(args[i].Remove(0, args[i].IndexOf("_") + 1));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(String.Format("Ошибка при чтении ключей: {0}", ex.Message));
                    Service.Monitor = 0;
                }
            }
        }
        /// <summary>
        /// Определение строки фильтра
        /// </summary>
        internal static void ManageFilter()
        {
            try
            {
                string fString = "";
                List<bool> priorityList = new List<bool>() { WhiteMessages, GreenMessages, YellowMessages, RedMessages };
                for(int i=0;i<4;i++)
                {
                    if(!priorityList[i] && string.IsNullOrEmpty(fString))
                    {
                        fString = string.Format("[Priority] <> {0}", i+1);
                    }
                    else if(!priorityList[i])
                    {
                        fString += string.Format(" AND [Priority] <> {0}", i+1);
                    }
                }
                var ssFstr = SubSystem.GetFilterString(SubSystems);
                if (string.IsNullOrEmpty(fString))
                {
                    fString = ssFstr;
                }
                else if(!string.IsNullOrEmpty(ssFstr))
                {
                    fString += string.Format(" AND {0}", ssFstr);
                }                
                if(!string.IsNullOrEmpty(TextFilterString) && string.IsNullOrEmpty(fString))
                {
                    fString = string.Format("Contains ([{0}], '{1}')", GetFilterColumnName(FilterColumnName), TextFilterString);
                }
                else if(!string.IsNullOrEmpty(TextFilterString))
                {
                    fString += string.Format(" AND Contains ([{0}], '{1}')", GetFilterColumnName(FilterColumnName), TextFilterString);
                }
                Filter = fString;
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка формирования строки фильтра: ", ex.Message);
            }
        }
       
        /// <summary>
        /// Получить имя столбца по его заголовку
        /// </summary>
        /// <param name="name">Заголовок столбца</param>
        /// <returns></returns>
        public static string GetFilterColumnName(string name)
        {
            switch(name)
            {
                case "Источник":
                    return "Source";
                case "Пользователь":
                    return "User";
                case "Значение":
                    return "MessageValue";
                default:
                    return "Text";
            }
        }
        /// <summary>
        /// Получение наследников по разметке
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
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
        /// <summary>
        /// Открытие панели поиска с возможностью отображения экранной клавиатуры
        /// </summary>
        /// <param name="grid"></param>
        internal static void ShowSearchPanel(object grid)
        {
            try
            {
                (grid as DevExpress.Xpf.Grid.GridControl).View.ShowSearchPanel(true);
                if ((grid as DevExpress.Xpf.Grid.GridControl).View.SearchControl==null)
                {
                    SingleInstanceApplication.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                GetVisualChild<TextBox>((grid as DevExpress.Xpf.Grid.GridControl).View.SearchControl).PreviewMouseLeftButtonDown += delegate
                                 {
                                        ShowKeyboard(GetVisualChild<TextBox>((grid as DevExpress.Xpf.Grid.GridControl).View.SearchControl));
                                 };
                            }), DispatcherPriority.Loaded); 
                }

            }
            catch (Exception ex)
            {
                logger.Error("Ошибка отображения панели поиска: {0}", ex.Message);
            }
        }
        /// <summary>
        /// Показать экранную клавиатуру
        /// </summary>
        /// <param name="tbox"></param>
        internal static void ShowKeyboard(TextBox tbox)
        {
            if(tbox!=null && Service.KeyboardNeeded)
            {
                Views.OnScreenKeyboard window = new Views.OnScreenKeyboard() { Value = tbox.Text };
                window.Closing += delegate
                {
                    tbox.Text = window.Value;
                };
                window.ShowDialog();
            }
        }        
        /// <summary>
        /// Экспорт и печать сообщений
        /// </summary>
        /// <param name="print"></param>
        public static void GlobalExport(bool print)
        {
            System.Threading.Thread printThread = new System.Threading.Thread(delegate() 
            {
                Views.HiddenScreen window = new Views.HiddenScreen() { Print = print, SavePath = ExpAddresses };
                window.ShowDialog();
                Progress = 0;
                NeedProgressBar = false;
            });
            printThread.SetApartmentState(System.Threading.ApartmentState.STA);
            printThread.Start();
        }
    }
}
