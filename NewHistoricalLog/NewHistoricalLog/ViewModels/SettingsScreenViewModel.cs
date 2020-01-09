using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using NewHistoricalLog.Models;
using DevExpress.Xpf.Editors;
using System.Windows.Controls;

namespace NewHistoricalLog.ViewModels
{
    public class SettingsScreenViewModel:ViewModelBase
    {

        private int pageSize;
        public int PageSize
        {
            get { return pageSize; }
            set
            {
                pageSize = value;
                RaisePropertyChanged("PageSize");
            }
        }
        private string exportpath;
        public string ExportPath
        {
            get { return exportpath; }
            set
            {
                exportpath = value;
                RaisePropertyChanged("ExportPath");
            }
        }
        private bool wrap;
        public bool Wrap
        {
            get { return wrap; }
            set
            {
                wrap = value;
                RaisePropertyChanged("Wrap");
            }
        }
        private bool needKeyboard;
        public bool NeedKeyboard
        {
            get { return needKeyboard; }
            set
            {
                needKeyboard = value;
                RaisePropertyChanged("NeedKeyboard");
            }
        }
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

        public SettingsScreenViewModel()
        {
            NeedKeyboard = Service.KeyboardNeeded;
            Wrap = Service.WrapText==System.Windows.TextWrapping.Wrap;
            PageSize = Service.CountLines;
            ExportPath = Service.SavePath;
            PriorityVisible = Service.ColumnVisibilityList[0];
            KvitVisible = Service.ColumnVisibilityList[1];
            ValueVisible = Service.ColumnVisibilityList[2];
            SourceVisible = Service.ColumnVisibilityList[3];
            UserVisible = Service.ColumnVisibilityList[4];
        }

        public ICommand ConfirmCommand
        {
            get { return new RelayCommand(ExecuteConfirm); }
        }

        private void ExecuteConfirm()
        {
            Service.KeyboardNeeded = NeedKeyboard;
            Service.WrapText = Wrap?System.Windows.TextWrapping.Wrap:System.Windows.TextWrapping.NoWrap;
            Service.CountLines = PageSize;
            Service.SavePath = ExportPath;
            Service.ColumnVisibilityList = new List<bool> { PriorityVisible, KvitVisible, ValueVisible, SourceVisible, UserVisible };
            Service.SaveSettings();
            DialogResult = true;
        }

        public ICommand DiscardCommand
        {
            get { return new RelayCommand(ExecuteDiscard); }
        }

        private void ExecuteDiscard()
        {
            DialogResult = false;
        }

        public ICommand ShowKeyboardCommand
        {
            get { return new RelayCommand<object>(ExecuteShowKeyboard); }
        }

        private void ExecuteShowKeyboard(object control)
        {
            MainModel.ShowKeyboard(((control as MouseButtonEventArgs).Source as TextEdit).EditCore as TextBox);
        }
    }
}
