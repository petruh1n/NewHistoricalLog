using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewHistoricalLog.Common;
using NewHistoricalLog.Models;
using DevExpress.Data.Filtering;
using System.Windows.Controls;
using DevExpress.Xpf.Editors;
using System.ComponentModel;

namespace NewHistoricalLog.ViewModels
{
    public class MainWindowViewModel:ViewModelBase
    {
        private string filterColumnName = "Сообщение";
        private bool filterVisible = false;
        private CriteriaOperator filter;
        private string textFilter;
        private bool adminMode = false;
        private bool red;
        private bool yellow;
        private bool green;
        private bool white;
        private bool indeterminant;
        private DateTime endTime;
        private DateTime startTime;
        private bool needProgressBar;
        private ObservableCollection<MessageClass> messages = new ObservableCollection<MessageClass>();
        private ObservableCollection<SubSystem> subsytems = new ObservableCollection<SubSystem>();
        private int pagesize;
        private System.Windows.TextWrapping wrapTextInGrid = System.Windows.TextWrapping.Wrap;
        private List<bool> columnVisibilities = new List<bool>() { true, true, true, true, true };
        private int progress;
        private int maxProgress;
        private string status;

        /// <summary>
        /// Коллекция сообщений
        /// </summary>
        public ObservableCollection<MessageClass> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                RaisePropertyChanged("Messages");
            }
        }
        /// <summary>
        /// Флаг необходимости прогресс-бара
        /// </summary>
        public bool NeedProgressBar
        {
            get { return needProgressBar; }
            set
            {
                needProgressBar = value;
                RaisePropertyChanged("NeedProgressBar");
            }
        }
        /// <summary>
        /// Стартовая дата выборки
        /// </summary>
        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                if (value!=startTime)
                {
                    startTime = value;
                    MainModel.StartTime = value;
                    RaisePropertyChanged("StartTime"); 
                }
            }
        }
        /// <summary>
        /// Конечная дата выборки
        /// </summary>
        public DateTime EndTime
        {
            get { return endTime; }
            set
            {
                if (value!=endTime)
                {
                    endTime = value;
                    MainModel.EndTime = value;
                    RaisePropertyChanged("EndTime"); 
                }
            }
        }
        /// <summary>
        /// Прогресс-бар без значений
        /// </summary>
        public bool Indeterminant
        {
            get { return indeterminant; }
            set
            {
                indeterminant = value;
                RaisePropertyChanged("Indeterminant");
            }
        }
        /// <summary>
        /// Показывать белые сообщения
        /// </summary>
        public bool White
        {
            get { return white; }
            set
            {
                if (value!=white)
                {
                    white = value;
                    MainModel.WhiteMessages = value;
                    MainModel.ManageFilter();
                    RaisePropertyChanged("White"); 
                }
            }
        }
        /// <summary>
        /// Показывать зеленые сообщения
        /// </summary>
        public bool Green
        {
            get { return green; }
            set
            {
                if (value!=green)
                {
                    green = value;
                    MainModel.GreenMessages = value;
                    MainModel.ManageFilter();
                    RaisePropertyChanged("Green"); 
                }
            }
        }
        /// <summary>
        /// Показывать желтые сообщения
        /// </summary>
        public bool Yellow
        {
            get { return yellow; }
            set
            {
                if (value!=yellow)
                {
                    yellow = value;
                    MainModel.YellowMessages = value;
                    MainModel.ManageFilter();
                    RaisePropertyChanged("Yellow"); 
                }
            }
        }
        /// <summary>
        /// Показывать красные сообщения
        /// </summary>
        public bool Red
        {
            get { return red; }
            set
            {
                if (value!=red)
                {
                    red = value;
                    MainModel.RedMessages = value;
                    MainModel.ManageFilter();
                    RaisePropertyChanged("Red"); 
                }
            }
        }
        /// <summary>
        /// Режим админа
        /// </summary>
        public bool AdminMode
        {
            get { return adminMode; }
            set
            {
                adminMode = value;
                RaisePropertyChanged("AdminMode");
            }
        }
        /// <summary>
        /// Текстовый фильтр
        /// </summary>
        public string TextFilter
        {
            get { return textFilter; }
            set
            {
                if (value!=textFilter)
                {
                    textFilter = value;
                    MainModel.TextFilterString = value;
                    RaisePropertyChanged("TextFilter"); 
                }
            }
        }
        /// <summary>
        /// Фильтр сообщений
        /// </summary>
        public CriteriaOperator Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                RaisePropertyChanged("Filter");
            }
        }
        /// <summary>
        /// Видимость фильтра
        /// </summary>
        public bool FilterVisible
        {
            get { return filterVisible; }
            set
            {
                filterVisible = value;
                RaisePropertyChanged("FilterVisible");
            }
        }
        /// <summary>
        /// Имя колонки фильтра
        /// </summary>
        public string FilterColumnName
        {
            get { return filterColumnName; }
            set
            {
                if(value!=filterColumnName)
                {
                    filterColumnName = value;
                    MainModel.FilterColumnName = value;
                    RaisePropertyChanged("FilterColumnName");
                }                
            }
        }
        /// <summary>
        /// Подсистемы для фильтрации
        /// </summary>
        public ObservableCollection<SubSystem> SubSystems
        {
            get { return subsytems; }
            set
            {
                subsytems = value;
                RaisePropertyChanged("SubSystems");
            }
        }       
        /// <summary>
        /// Число строк на странице
        /// </summary>
        public int PageSize
        {
            get { return pagesize; }
            set
            {
                pagesize = value;
                RaisePropertyChanged("PageSize");
            }
        }
        /// <summary>
        /// Переносить текст сообщения
        /// </summary>
        public System.Windows.TextWrapping WrapTextInGrid
        {
            get { return wrapTextInGrid; }
            set
            {
                wrapTextInGrid = value;
                RaisePropertyChanged("WrapTextInGrid");
            }
        }
        /// <summary>
        /// Видимость колонок
        /// </summary>
        public List<bool> ColumnVisibilities
        {
            get { return columnVisibilities; }
            set
            {
                columnVisibilities = value;
                RaisePropertyChanged("ColumnVisibilities");
            }
        }
        /// <summary>
        /// Максимальный значение прогресса
        /// </summary>
        public int MaxProgress
        {
            get { return maxProgress; }
            set
            {
                maxProgress = value;
                RaisePropertyChanged("MaxProgress");
            }
        }
        /// <summary>
        /// Текущее значение прогресса
        /// </summary>
        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                RaisePropertyChanged("Progress");
            }
        }
        /// <summary>
        /// Сообщение в прогресс-баре
        /// </summary>
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                RaisePropertyChanged("Status");
            }
        }

        public MainWindowViewModel()
        {
            MainModel.StaticPropertyChanged += GlobalPropertyChanged;
            Service.StaticPropertyChanged += GlobalPropertyChanged;
            MainModel.Messages.CollectionChanged += Messages_CollectionChanged;
            MainModel.SubSystems.CollectionChanged += SubSystems_CollectionChanged;
            MainModel.StartApp();
        }

        private void SubSystems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SubSystems = MainModel.SubSystems;
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Messages = MainModel.Messages;
        }

        private void GlobalPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SubSystems = MainModel.SubSystems;
            Messages = MainModel.Messages;
            EndTime = MainModel.EndTime;
            StartTime = MainModel.StartTime;
            NeedProgressBar = MainModel.NeedProgressBar;
            White = MainModel.WhiteMessages;
            Green = MainModel.GreenMessages;
            Yellow = MainModel.YellowMessages;
            Red = MainModel.RedMessages;
            Indeterminant = MainModel.Indeterminant;
            AdminMode = MainModel.AdminMode;
            Filter = CriteriaOperator.TryParse(MainModel.Filter);
            PageSize = Service.CountLines;
            WrapTextInGrid = Service.WrapText;
            ColumnVisibilities = Service.ColumnVisibilityList;
            MaxProgress = MainModel.MaxProgress;
            Progress = MainModel.Progress;
            Status = MainModel.Status;
        }


        public ICommand LoadMessagesCommand
        {
            get { return new RelayCommand(ExecuteLoadMessages); }
        }

        private async void ExecuteLoadMessages()
        {
            await MainModel.GetMessagesAsync();
        }

        public ICommand ClearFilterCommand
        {
            get { return new RelayCommand(ExecuteClearFilter); }
        }

        private void ExecuteClearFilter()
        {
            White = true;
            Red = true;
            Yellow = true;
            Green = true;
            TextFilter = "";
            foreach(var ss in MainModel.SubSystems)
            {
                foreach(var dev in ss.Devices)
                {
                    dev.Selected = false;
                }
            }
            MainModel.ManageFilter();
        }

        public ICommand LaunchTextFilterCommand
        {
            get { return new RelayCommand(ExecuteLaunchTextFilter); }
        }

        private void ExecuteLaunchTextFilter()
        {
            MainModel.ManageFilter();
        }

        public ICommand ShowSearchPanelCommand
        {
            get { return new RelayCommand<object>(ExecuteShowSearchPanel); }
        }

        private void ExecuteShowSearchPanel(object grid)
        {
            MainModel.ShowSearchPanel(grid);
        }

        public ICommand PrintCommand
        {
            get { return new RelayCommand(ExecutePrint); }
        }

        private void ExecutePrint()
        {
            Views.PrintWindow window = new Views.PrintWindow();
            window.Owner = SingleInstanceApplication.Current.MainWindow;
            if (window.ShowDialog() == true)
                MainModel.GlobalExport(true);
        }

        public ICommand SaveCommand
        {
            get { return new RelayCommand<object>(ExecuteSave); }
        }

        private void ExecuteSave(object destination)
        {
            Views.ExportScreen window = new Views.ExportScreen();
            window.Owner = SingleInstanceApplication.Current.MainWindow;
            if(window.ShowDialog()==true)
            {
                MainModel.GlobalExport(false);
            }
        }

        public ICommand ShowKeyboardCommand
        {
            get { return new RelayCommand<object>(ExecuteShowKeyboard); }
        }

        private void ExecuteShowKeyboard(object control)
        {
            MainModel.ShowKeyboard(((control as MouseButtonEventArgs).Source as TextEdit).EditCore as TextBox);
        }

        public ICommand ShowSettingsCommand
        {
            get { return new RelayCommand(ExecuteShowSettings); }
        }

        private void ExecuteShowSettings()
        {
            Views.SettingsScreen window = new Views.SettingsScreen();
            window.Owner = SingleInstanceApplication.Current.MainWindow;
            window.ShowDialog();
        }

        public ICommand ShowAboutCommand
        {
            get { return new RelayCommand(ExecuteShowAbout); }
        }

        private void ExecuteShowAbout()
        {
            InfoWindow window = new InfoWindow();
            window.Owner = SingleInstanceApplication.Current.MainWindow;
            window.ShowDialog();
        }

        public ICommand ClosingCommand
        {
            get { return new RelayCommand<object>(ExecuteClosing); }
        }

        private void ExecuteClosing(object args)
        {
            (args as CancelEventArgs).Cancel = true;
            MainModel.StartTime = DateTime.Now.AddHours(-1);
            MainModel.EndTime = DateTime.Now;
            MainModel.ni.Visible = true;
            SingleInstanceApplication.Current.MainWindow.Hide();
        }
    }
}
