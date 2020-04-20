﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHistoricalLog.Models
{

    public class SubSystem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string tablename;
        private string description;
        private ObservableCollection<Device> devices = new ObservableCollection<Device>();

        /// <summary>
        /// Имя таблицы
        /// </summary>
        public string TableName
        {
            get { return tablename; }
            set
            {
                tablename = value;
                OnPropertyChanged("TableName");
            }
        }
        /// <summary>
        /// Описание подсистемы
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }
        /// <summary>
        /// Список устройств в подсистеме
        /// </summary>
        public ObservableCollection<Device> Devices
        {
            get { return devices; }
            set
            {
                devices = value;
                OnPropertyChanged("Devices");
            }
        }

        public static string GetFilterString(IEnumerable<SubSystem> collection)
        {
            string result = "";
            foreach(var element in collection)
            {
                foreach(var device in element.Devices)
                {
                    if(device.Selected && string.IsNullOrEmpty(result))
                    {
                        result = device.DeviceFilter.Expresion;
                    }
                    else if(device.Selected)
                    {
                        result += string.Format(" OR {0}", device.DeviceFilter.Expresion);
                    }
                }
            }
            return string.IsNullOrEmpty(result)?"":string.Format("({0})",result);
        }
    }
}
