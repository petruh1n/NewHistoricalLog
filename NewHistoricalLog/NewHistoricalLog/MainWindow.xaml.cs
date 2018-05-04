using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using DevExpress.Data.Filtering;

namespace NewHistoricalLog
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : ThemedWindow
	{
		public MainWindow()
		{
			InitializeComponent();
            this.Loaded += OnLoad;
		}

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            filterRow.Height = new GridLength(0);
            startDate.EditValue = DateTime.Now.AddHours(-1);
            endDate.EditValue = DateTime.Now;
        }

        private void OnFilterPopupClose(object sender, EventArgs e)
        {
            for(int i=0;i<Service.Priorities.Length;i++)
            {
                if(!Service.Priorities[i])
                {
                    var a = messageGrid.FilterString;
                    if (!String.IsNullOrEmpty(a))
                        messageGrid.FilterCriteria = CriteriaOperator.Parse(String.Format("{0} AND [Type] != {1}", a,i+1));
                    else
                        messageGrid.FilterCriteria = CriteriaOperator.Parse(String.Format("[Type] != {0}", i+1));
                    
                }
            }
        }

        private void PrintClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {

        }

        private void Priority_CheckedChanged(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            //Service.Priorities = new bool[] { red.IsChecked.Value, yellow.IsChecked.Value, green.IsChecked.Value, gray.IsChecked.Value };
        }

        private void ShowTextFilterClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            filterRow.Height = new GridLength(30);
        }
        private void ClearTextFilterClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            filterRow.Height = new GridLength(0);
        }
        private void RefreshClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var messData = MessageGridContent.LoadMessages(Service.StartDate, Service.EndDate);
            messageGrid.ItemsSource = null;
            messageGrid.ItemsSource = messData;
        }
        private void SaveClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {

        }
        private void ExitClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            Close();
        }

        private void StartDateChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            Service.StartDate = Convert.ToDateTime(startDate.EditValue);
        }
        private void EndDateChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            Service.EndDate = Convert.ToDateTime(endDate.EditValue);
        }

        private void LaunchFilterClick(object sender, RoutedEventArgs e)
        {
            messageGrid.FilterCriteria = CriteriaOperator.Parse(String.Format("Contains([{0}] ,'{1}')",Service.FilterField,Service.FilterPhrase));
            var a = messageGrid.FilterString;
        }

        private void FilterColumnChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            switch(filterBox.EditValue.ToString())
            {
                case "Сообщение":
                    Service.FilterField = "Text";
                    break;
                case "Источник":
                    Service.FilterField = "Source";
                    break;
                case "Пользователь":
                    Service.FilterField = "User";
                    break;
                case "Значение":
                    Service.FilterField = "Value";
                    break;
            }
        }

        private void FilterTextChanging(object sender, DevExpress.Xpf.Editors.EditValueChangingEventArgs e)
        {
            if(e.NewValue!=null)
                Service.FilterPhrase = e.NewValue.ToString();
            else
                Service.FilterPhrase = "";
        }
    }
}
