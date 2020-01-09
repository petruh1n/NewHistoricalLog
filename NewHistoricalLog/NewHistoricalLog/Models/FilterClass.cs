using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHistoricalLog
{

    public class FilterClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private string name="";
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        private string expression="";
        public string Expresion
        {
            get { return expression; }
            set
            {
                expression = value;
                OnPropertyChanged("Expresion");
            }
        }
        private bool selected=false;
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                OnPropertyChanged("Selected");
            }
        }

        public FilterClass(string name)
        {
            Name = name;
            Selected = false;
            expression = string.Format("(Contains([Text], '{0} ') Or Contains([Text], '{0}.'))", name);
        }
        /// <summary>
        /// Получить строку фильтрации по подсистемам
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string GetFilterString(IEnumerable<FilterClass> collection)
        {
            string result = "";
            foreach(var fc in collection)
            {
                if(fc.Selected)
                {
                    if(string.IsNullOrEmpty(result))
                    {
                        result = fc.Expresion;
                    }
                    else
                    {
                        result += string.Format(" Or {0}", fc.Expresion);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// получить фильтр по его имени
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FilterClass GetByName(IEnumerable<FilterClass> collection, string name)
        {
            foreach(var fc in collection)
            {
                if(fc.Name==name)
                {
                    return fc;
                }
            }
            return null;
        }
    }
}
