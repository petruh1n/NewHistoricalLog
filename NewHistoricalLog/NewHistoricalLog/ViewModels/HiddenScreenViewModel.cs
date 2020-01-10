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
        private CriteriaOperator filter;
        private bool red;
        private bool yellow;
        private bool green;
        private bool white;        
        private ObservableCollection<MessageClass> messages = new ObservableCollection<MessageClass>();
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
            Messages = MainModel.Messages;
            White = MainModel.WhiteMessages;
            Green = MainModel.GreenMessages;
            Yellow = MainModel.YellowMessages;
            Red = MainModel.RedMessages;
            Filter = CriteriaOperator.TryParse(MainModel.Filter);
            PageSize = Service.CountLines;
            WrapTextInGrid = Service.WrapText;
            ColumnVisibilities = MainModel.ExportColumns;
        }
    }
}
