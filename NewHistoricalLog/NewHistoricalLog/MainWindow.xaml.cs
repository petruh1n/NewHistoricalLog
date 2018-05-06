using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using System.IO;
using NLog;
using DevExpress.Xpf.Core;
using DevExpress.Data.Filtering;

namespace NewHistoricalLog
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : ThemedWindow
	{
        #region Служебный переменные
        Logger logger = LogManager.GetCurrentClassLogger();
		string currentFilterPhrase = "";
		string currentFilterColumn = "";

		#endregion

        static MainWindow()
        {
            EditorLocalizer.Active = new EditorLocalizerEx();
        }

		public MainWindow()
		{
			InitializeComponent();
            this.Loaded += OnLoad;
		}

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            #region Чтение настроек приложения
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SEMHistory\\Config.xml"))
                Service.ReadSettings();
            else
                Service.GenerateXMLConfig();
            #endregion

            #region Чтение ключей
            if (Environment.GetCommandLineArgs() != null)
            {
                try
                {
                    for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++)
                    {
                       
                        if (Environment.GetCommandLineArgs()[i].ToUpper().Contains("MONITOR"))
                        {
                            Service.Monitor = Convert.ToInt32(Environment.GetCommandLineArgs()[i].Remove(0, Environment.GetCommandLineArgs()[i].IndexOf("_") + 1));
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(String.Format("Ошибка при чтении ключей: {0}", ex.Message));
                    Service.Monitor = 0;
                }
            }
            #endregion

            #region Позиционирование окна приложения
            Width = Service.Width;
            Height = Service.Height;

            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                this.Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top;
                this.Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left;
            }
            else
            {
                this.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top;
                this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left;
            }

            #endregion

            //Скрыли панельку с текстовыми фильтрами
            filterRow.Height = new GridLength(0);
            //В комбобоксе текстового фильтра поставили "Сообщение" 
			filterBox.SelectedItem = filterBox.Items[0];
            //Число строк в гриде равно тому, что в настройках
            messageView.PageSize = Service.CountLines;
            //устанавливаем промежуток времени - один час назад от текущего времени
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
					{
						if (!a.Contains(String.Format("[Type] <> {0}", i + 1)))
							messageGrid.FilterCriteria = CriteriaOperator.Parse(String.Format("{0} AND [Type] <> {1}", a, i + 1));
					}
                    else
                        messageGrid.FilterCriteria = CriteriaOperator.Parse(String.Format("[Type] <> {0}", i+1));
                    
                }
				else
				{
					var a = messageGrid.FilterString;
					if (!String.IsNullOrEmpty(a))
					{
						if(a.Contains(String.Format("[Type] <> {0}", i + 1)))
						{
							//если условие первое и единственное
							if(a.IndexOf(String.Format("[Type] <> {0}", i + 1))==0 && a.Length== String.Format("[Type] <> {0}", i + 1).Length)
							{
								messageGrid.FilterCriteria = null;
							}
							//если условие первое, но не единственное
							else if(a.IndexOf(String.Format("[Type] <> {0}", i + 1)) == 0)
							{
								a = a.Remove(0, String.Format("[Type] <> {0} AND ", i + 1).Length);
								messageGrid.FilterCriteria = CriteriaOperator.Parse(a);
							}
							//если условие не первое
							else
							{
								a = a.Remove(a.IndexOf(String.Format("[Type] <> {0}", i + 1))-5, String.Format("AND [Type] <> {0} ", i + 1).Length);
								messageGrid.FilterCriteria = CriteriaOperator.Parse(a);
							}
						}
					}
				}
            }
        }

        private void PrintClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            messageView.Print();
            //messageView.ShowPrintPreviewDialog(this);
        }

        private void Priority_CheckedChanged(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
			try
			{
				Service.Priorities = new bool[] { gray.IsChecked.Value, green.IsChecked.Value, yellow.IsChecked.Value, red.IsChecked.Value };
			}
			catch
			{
				Service.Priorities = new bool[] { true,true,true,true};
			}
        }

        private void ShowTextFilterClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            filterRow.Height = new GridLength(30);
        }
        private void ClearTextFilterClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            filterRow.Height = new GridLength(0);
			filterText.Text = "";
			messageGrid.FilterCriteria = null;
        }
        private void RefreshClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            if(Service.EndDate>Service.StartDate)
            {
                var messData = MessageGridContent.LoadMessages(Service.StartDate, Service.EndDate);
                messageGrid.ItemsSource = null;
                messageGrid.ItemsSource = messData;
                if(messData.Count>0)
                {
                    tools.IsEnabled = true;
                }
                else
                {
                    tools.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show("Начало выборки должно быть раньше ее завершения!", "Некорректный промежуток времени!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            try
            {
                string fileName = String.Format("Сообщения с {0} по {1}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"));
                int counter = 1;
                while (File.Exists(String.Format("{0}\\SEMHistory\\Export\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName)))
                {
                    fileName = String.Format("Сообщения с {0} по {1}_{2}.pdf", Service.StartDate.ToString().Replace(":", "_"), Service.EndDate.ToString().Replace(":", "_"), counter);
                    counter++;
                }
                //DevExpress.XtraPrinting.RtfExportOptions options = new DevExpress.XtraPrinting.RtfExportOptions();
                //options.ExportMode = DevExpress.XtraPrinting.RtfExportMode.SingleFile;
                //options.ExportPageBreaks = true;
                DevExpress.XtraPrinting.PdfExportOptions pdfOptions = new DevExpress.XtraPrinting.PdfExportOptions();
                messageView.ExportToPdf(String.Format("{0}\\SEMHistory\\Export\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName));
                //messageView.ExportToRtf(String.Format( "{0}\\SEMHistory\\Export\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName), options);
                //messageView.ExportToCsv(String.Format("{0}\\SEMHistory\\Export\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "test.csv"));
                MessageBox.Show("Файл сохранен!", "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Файл сохранить не удалось. Для подробной информации см. лог приложения.", "Ошибка при сохранении файла!", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Error(String.Format("Ошибка при сохранении файла: {0}", ex.Message));
            }
            
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
			var a = messageGrid.FilterString;
			if(string.IsNullOrEmpty(a))
				messageGrid.FilterCriteria = CriteriaOperator.Parse(String.Format("Contains([{0}] ,'{1}')",Service.FilterField,Service.FilterPhrase));
			else if(a.Contains(currentFilterPhrase))
			{
				a = a.Remove(0, String.Format("Contains([{0}] ,'{1}')", currentFilterColumn, currentFilterPhrase).Length);
				messageGrid.FilterCriteria = CriteriaOperator.Parse(String.Format("Contains([{0}] ,'{1}') {2}", Service.FilterField, Service.FilterPhrase,a));
			}
			else
				messageGrid.FilterCriteria = CriteriaOperator.Parse(String.Format("Contains([{0}] ,'{1}') AND {2}", Service.FilterField, Service.FilterPhrase, a));
			currentFilterColumn = Service.FilterField;
			currentFilterPhrase = Service.FilterPhrase;

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

		private void RefreshButtonClick(object sender, RoutedEventArgs e)
		{
            if (Service.EndDate > Service.StartDate)
            {
                var messData = MessageGridContent.LoadMessages(Service.StartDate, Service.EndDate);
                messageGrid.ItemsSource = null;
                messageGrid.ItemsSource = messData;
            }
            else
            {
                MessageBox.Show("Начало выборки должно быть раньше ее завершения!", "Некорректный промежуток времени!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WindowMoved(object sender, EventArgs e)
        {
            if (Service.Monitor <= System.Windows.Forms.Screen.AllScreens.Length)
            {
                this.Top = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.Y + Service.Top;
                this.Left = System.Windows.Forms.Screen.AllScreens[Service.Monitor].Bounds.X + Service.Left;
            }
            else
            {
                this.Top = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y + Service.Top;
                this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.X + Service.Left;
            }
        }

        private void StartSearchClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            messageView.ShowSearchPanel(true);
        }

        private void SaveToClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {

        }
        private void AboutClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {

        }
    }
    public class EditorLocalizerEx : EditorLocalizer
    {
        protected override void PopulateStringTable()
        {
            base.PopulateStringTable();
            this.AddString(EditorStringId.LastPage, "Последняя страница");
            this.AddString(EditorStringId.NextPage, "Следующая страница");
            this.AddString(EditorStringId.FirstPage, "Первая страница");
            this.AddString(EditorStringId.PrevPage, "Предыдущая страница");
            this.AddString(EditorStringId.LookUpSearch, "Поиск");
            this.AddString(EditorStringId.LookUpClose, "Закрыть");
            this.AddString(EditorStringId.Page, "Страница");
            this.AddString(EditorStringId.Of, "из {0}");
            this.AddString(EditorStringId.DatePickerMinutes, "мин");
            this.AddString(EditorStringId.DatePickerHours, "час");
            this.AddString(EditorStringId.DatePickerSeconds, "сек");
        }
    }
}
