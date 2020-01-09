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
        public bool? DialogResult
        {
            get { return dialogResult; }
            set
            {
                dialogResult = value;
                RaisePropertyChanged("DialogResult");
            }
        }

        private bool prior;
        public bool PriorityVisible
        {
            get { return prior; }
            set
            {
                prior = value;
                RaisePropertyChanged("PriorityVisible");
            }
        }
        private bool kvit;
        public bool KvitVisible
        {
            get { return kvit; }
            set
            {
                kvit = value;
                RaisePropertyChanged("KvitVisible");
            }
        }
        private bool val;
        public bool ValueVisible
        {
            get { return val; }
            set
            {
                val = value;
                RaisePropertyChanged("ValueVisible");
            }
        }
        private bool source;
        public bool SourceVisible
        {
            get { return source; }
            set
            {
                source = value;
                RaisePropertyChanged("SourceVisible");
            }
        }
        private bool user;
        public bool UserVisible
        {
            get { return user; }
            set
            {
                user = value;
                RaisePropertyChanged("UserVisible");
            }
        }
        private string path="";
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                RaisePropertyChanged("Path");
            }
        }
        private List<string> devices = new List<string>();
        public List<string> Devices
        {
            get { return devices; }
            set
            {
                devices = value;
                RaisePropertyChanged("Devices");
            }
        }
        private bool sendToDmz=false;
        public bool SendToDmz
        {
            get { return sendToDmz; }
            set
            {
                sendToDmz = value;
                RaisePropertyChanged("SendToDmz");
            }
        }
        private bool sendToDefaultDir;
        public bool SendToDefaultDir
        {
            get { return sendToDefaultDir; }
            set
            {
                sendToDefaultDir = value;
                RaisePropertyChanged("SendToDefaultDir");
            }
        }
        private string defaultDir="";
        public string DefaultDir
        {
            get { return defaultDir; }
            set
            {
                defaultDir = value;
                RaisePropertyChanged("DefaultDir");
            }
        }
        private bool isAdmin;
        public bool IsAdmin
        {
            get { return isAdmin; }
            set
            {
                isAdmin = value;
                RaisePropertyChanged("IsAdmin");
            }
        }
        private bool sendToStorage;
        public bool SendToStorage
        {
            get { return sendToStorage; }
            set
            {
                sendToStorage = value;
                RaisePropertyChanged("SendToStorage");
            }
        }

        public ExportViewModel()
        {
            IsAdmin = Models.MainModel.AdminMode;
            DefaultDir = Service.SavePath;
            Devices = GetDevices();
            PriorityVisible = Service.ColumnVisibilityList[0];
            KvitVisible = Service.ColumnVisibilityList[1];
            ValueVisible = Service.ColumnVisibilityList[2];
            SourceVisible = Service.ColumnVisibilityList[3];
            UserVisible = Service.ColumnVisibilityList[4];
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
