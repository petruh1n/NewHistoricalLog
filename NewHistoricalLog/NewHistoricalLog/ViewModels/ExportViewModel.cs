using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.IO;

namespace NewHistoricalLog.ViewModels
{
    public class ExportViewModel:ViewModelBase
    {
        private bool? dialogResult;
        private bool prior;
        private bool kvit;
        private bool val;
        private bool source;
        private bool user;
        private string path = "";
        private List<string> devices = new List<string>();
        private bool sendToDmz = false;
        private bool sendToDefaultDir;
        private string defaultDir = "";
        private bool isAdmin;
        private bool sendToStorage;
        private bool isIng;
        private bool allowRemote;


        public bool? DialogResult
        {
            get { return dialogResult; }
            set
            {
                dialogResult = value;
                RaisePropertyChanged("DialogResult");
            }
        }
        /// <summary>
        /// Флаг видимости колонки Приоритет
        /// </summary>
        public bool PriorityVisible
        {
            get { return prior; }
            set
            {
                prior = value;
                RaisePropertyChanged("PriorityVisible");
            }
        }
        /// <summary>
        /// Флаг видимости колонки Квитирование
        /// </summary>
        public bool KvitVisible
        {
            get { return kvit; }
            set
            {
                kvit = value;
                RaisePropertyChanged("KvitVisible");
            }
        }
        /// <summary>
        /// Флаг видимости колонки Значение
        /// </summary>
        public bool ValueVisible
        {
            get { return val; }
            set
            {
                val = value;
                RaisePropertyChanged("ValueVisible");
            }
        }
        /// <summary>
        /// Флаг видимости колонки Источник
        /// </summary>
        public bool SourceVisible
        {
            get { return source; }
            set
            {
                source = value;
                RaisePropertyChanged("SourceVisible");
            }
        }
        /// <summary>
        /// Флаг видимости колонки Пользователь
        /// </summary>
        public bool UserVisible
        {
            get { return user; }
            set
            {
                user = value;
                RaisePropertyChanged("UserVisible");
            }
        }
        /// <summary>
        /// Путь по умолчанию для экспорта
        /// </summary>
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                RaisePropertyChanged("Path");
            }
        }
        /// <summary>
        /// Список съемных устройств
        /// </summary>
        public List<string> Devices
        {
            get { return devices; }
            set
            {
                devices = value;
                RaisePropertyChanged("Devices");
            }
        }
        /// <summary>
        /// Флаг отправки на ДМЗ
        /// </summary>
        public bool SendToDmz
        {
            get { return sendToDmz; }
            set
            {
                sendToDmz = value;
                RaisePropertyChanged("SendToDmz");
            }
        }
        /// <summary>
        /// Флаг отправки в директорию по умолчанию
        /// </summary>
        public bool SendToDefaultDir
        {
            get { return sendToDefaultDir; }
            set
            {
                sendToDefaultDir = value;
                RaisePropertyChanged("SendToDefaultDir");
            }
        }
        /// <summary>
        /// Директория по умолчанию
        /// </summary>
        public string DefaultDir
        {
            get { return defaultDir; }
            set
            {
                defaultDir = value;
                RaisePropertyChanged("DefaultDir");
            }
        }
        /// <summary>
        /// Флаг администратора
        /// </summary>
        public bool IsAdmin
        {
            get { return isAdmin; }
            set
            {
                isAdmin = value;
                RaisePropertyChanged("IsAdmin");
            }
        }
        /// <summary>
        /// Флаг инженера
        /// </summary>
        public bool IsIng
        {
            get { return isIng; }
            set
            {
                isIng = value;
                RaisePropertyChanged("IsIng");
            }
        }
        /// <summary>
        /// Флаг отправки на съемный носитель
        /// </summary>
        public bool SendToStorage
        {
            get { return sendToStorage; }
            set
            {
                sendToStorage = value;
                RaisePropertyChanged("SendToStorage");
            }
        }
       
        public bool AllowRemote
        {
            get { return allowRemote; }
            set
            {
                allowRemote = value;
                RaisePropertyChanged("AllowRemote");
            }
        }

        public ExportViewModel()
        {
            IsAdmin = Models.MainModel.AdminMode;
            IsIng = Models.MainModel.IngMode;
            DefaultDir = Service.SavePath;
            Devices = GetDevices();
            PriorityVisible = Service.ColumnVisibilityList[0];
            KvitVisible = Service.ColumnVisibilityList[1];
            ValueVisible = Service.ColumnVisibilityList[2];
            SourceVisible = Service.ColumnVisibilityList[3];
            UserVisible = Service.ColumnVisibilityList[4];
            AllowRemote = Service.AllowRemoteDevices;
        }

        public List<string> GetDevices()
        {
            List<string> result = new List<string>();
            DriveInfo[] Devs = DriveInfo.GetDrives();
            var Pathes = (from dev in Devs
                          where dev.DriveType == DriveType.Removable
                          select dev.RootDirectory);
            foreach (var path in Pathes)
            {
                result.Add(path.FullName);
            }
            return result;
        }

        public ICommand ConfirmCommand
        {
            get { return new RelayCommand(ExecuteConfirm); }
        }

        private void ExecuteConfirm()
        {
            Models.MainModel.ExportColumns = new List<bool> { PriorityVisible, KvitVisible, ValueVisible, SourceVisible, UserVisible };
            Models.MainModel.ExpAddresses = new List<string>();
            Service.SavePath = DefaultDir;
            if (SendToDefaultDir)
                Models.MainModel.ExpAddresses.Add(Service.SavePath);
            if (SendToDmz)
                Models.MainModel.ExpAddresses.Add(Service.DmzPath);
            if (SendToStorage)
                Models.MainModel.ExpAddresses.Add(Path);
            Service.SaveSettings();
            DialogResult = true;
        }

        public ICommand DiscardCommand
        {
            get { return new RelayCommand(ExecuteDiscard); }
        }

        private void ExecuteDiscard()
        {
            Models.MainModel.ExpAddresses = new List<string>();
            DialogResult = false;
        }

        public ICommand RefreshCommand
        {
            get { return new RelayCommand(ExecuteRefresh); }
        }

        private void ExecuteRefresh()
        {
            Path = "";
            Devices = GetDevices();
        }
    }
}
