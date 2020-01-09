using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHistoricalLog.Models
{

    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool selected = false;
        public bool Selected
        {
            get { return selected; }
            set
            {
                if(selected!=value)
                {
                    selected = value;
                    MainModel.ManageFilter();                    
                    OnPropertyChanged("Selected");
                } 
            }
        }
        private string desc;
        public string Description
        {
            get { return desc; }
            set
            {
                desc = value;
                OnPropertyChanged("Description");
            }
        }
        private FilterClass filterDevice;
        public FilterClass DeviceFilter
        {
            get { return filterDevice; }
            set
            {
                filterDevice = value;
                OnPropertyChanged("DeviceFilter");
            }
        }

        public Device(string _desc)
        {
            Description = _desc;
            Selected = false;
            DeviceFilter = new FilterClass(_desc);
        }
    }
}
