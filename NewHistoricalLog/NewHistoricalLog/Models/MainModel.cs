using NewHistoricalLog.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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

       

        public static DateTime StartTime
        {
            get { return startDate; }
            set
            {
                startDate = value;
                OnStaticPropertyChanged("StartTime");
            }
        }
        static DateTime endTime = DateTime.Now;
        public static DateTime EndTime
        {
            get { return endTime; }
            set
            {
                endTime = value;
                OnStaticPropertyChanged("EndTime");
            }
        }
        static string filter;
        public static string Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                OnStaticPropertyChanged("Filter");
            }
        }
        static ObservableCollection<MessageClass> messages = new ObservableCollection<MessageClass>();
        public static ObservableCollection<MessageClass> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                OnStaticPropertyChanged("Messages");
            }
        }  
        static ObservableCollection<SubSystem> subsystems = new ObservableCollection<SubSystem>();
        public static ObservableCollection<SubSystem> SubSystems
        {
            get { return subsystems; }
            set
            {
                subsystems = value;
                OnStaticPropertyChanged("SubSystems");
            }
        }
        static bool needProgressBar=false;
        public static bool NeedProgressBar
        {
            get { return needProgressBar; }
            set
            {
                needProgressBar = value;
                OnStaticPropertyChanged("NeedProgressBar");
            }
        }
        static string status;
        public static string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnStaticPropertyChanged("Status");
            }
        }
        static bool whiteMessages=true;
        public static bool WhiteMessages
        {
            get { return whiteMessages; }
            set
            {
                whiteMessages = value;
                OnStaticPropertyChanged("WhiteMessages");
            }
        }
        static bool greenMessages=true;
        public static bool GreenMessages
        {
            get { return greenMessages; }
            set
            {
                greenMessages = value;
                OnStaticPropertyChanged("GreenMessages");
            }
        }
        static bool yellowMessages = true;
        public static bool YellowMessages
        {
            get { return yellowMessages; }
            set
            {
                yellowMessages = value;
                OnStaticPropertyChanged("YellowMessages");
            }
        }
        static bool redMessages = true;
        public static bool RedMessages
        {
            get { return redMessages; }
            set
            {
                redMessages = value;
                OnStaticPropertyChanged("RedMessages");
            }
        }
        static bool indeterminant=false;
        public static bool Indeterminant
        {
            get { return indeterminant; }
            set
            {
                indeterminant = value;
                OnStaticPropertyChanged("Indeterminant");
            }
        }
        static int progress;
        public static int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnStaticPropertyChanged("Progress");
            }
        }
        static int maxProgress;
        public static int MaxProgress
        {
            get { return maxProgress; }
            set
            {
                maxProgress = value;
                OnStaticPropertyChanged("MaxProgress");
            }
        }
        static bool adminMode=false;
        public static bool AdminMode
        {
            get { return adminMode; }
            set
            {
                adminMode = value;
                OnStaticPropertyChanged("AdminMode");
            }
        }
        static string textFilterString;
        public static string TextFilterString
        {
            get { return textFilterString; }
            set
            {
                textFilterString = value;
                OnStaticPropertyChanged("TextFilterString");
            }
        }
        static string filterColumnName = "Сообщение";
        public static string FilterColumnName
        {
            get { return filterColumnName; }
            set
            {
                filterColumnName = value;
                OnStaticPropertyChanged("FilterColumnName");
            }
        }       
        static List<bool> exportColumns = Service.ColumnVisibilityList;
        public static List<bool> ExportColumns
        {
            get { return exportColumns; }
            set
            {
                exportColumns = value;
                OnStaticPropertyChanged("ExportColumns");
            }
        }
        static List<string> expAddresses = new List<string>();
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
            ni.Icon = new System.Drawing.Icon(Path.Combine(Directory.GetCurrentDirectory(), "Images\\Msg.ico"));
            ni.Visible = false;
            ni.Text = "Журнал исторических сообщений";
            ni.DoubleClick +=
                async delegate (object sender, EventArgs args)
                {
                    await DbHelper.GetUserData();
                    SubSystems = await DbHelper.GetSubSystemInfo();
                    await GetMessagesAsync();
                    SingleInstanceApplication.Current.MainWindow.Show();
                    SingleInstanceApplication.Current.MainWindow.WindowState = WindowState.Normal;
                };
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
                foreach(var subSys in SubSystems)
                {
                    foreach(var dev in subSys.Devices)
                    {
                        if(dev.Selected && string.IsNullOrEmpty(fString))
                        {
                            fString = dev.DeviceFilter.Expresion;
                        }
                        else if(dev.Selected)
                        {
                            fString += string.Format(" OR {0}", dev.DeviceFilter.Expresion);
                        }
                    }
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
                Views.HiddenScreen window = new Views.HiddenScreen();
                //window.Owner = SingleInstanceApplication.Current.MainWindow;
                window.Print = print;
                window.SavePath = ExpAddresses;
                window.ShowDialog();
                Progress = 0;
                NeedProgressBar = false;
            });
            printThread.SetApartmentState(System.Threading.ApartmentState.STA);
            printThread.Start();
        }
    }
}
