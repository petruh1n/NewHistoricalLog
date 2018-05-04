using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHistoricalLog
{
    public class MainWindowViewModel
	{
        public ObservableCollection<MessageGridContent> MessageCollection { get; set; }

        public MainWindowViewModel()
        {
            MessageCollection = new ObservableCollection<MessageGridContent>();
        }
	}
}
