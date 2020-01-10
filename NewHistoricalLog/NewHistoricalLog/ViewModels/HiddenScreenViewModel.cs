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

namespace NewHistoricalLog.ViewModels
{
    public class HiddenScreenViewModel:ViewModelBase
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
        private List<bool> columnVisibilities = new List<bool>() { true, true, true, true, true };
        private int pagesize;
        private System.Windows.TextWrapping wrapTextInGrid = System.Windows.TextWrapping.Wrap;

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
                if (value != startTime)
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
                if (value != endTime)
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
                if (value != white)
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
                if (value != green)
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
                if (value != yellow)
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
                if (value != red)
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
                if (value != textFilter)
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
                if (value != filterColumnName)
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
        /// Видимость колонок таблицы
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
        /// Перенос текста по словам
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

        public HiddenScreenViewModel()
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
            ColumnVisibilities = MainModel.ExportColumns;
        }
    }
}
