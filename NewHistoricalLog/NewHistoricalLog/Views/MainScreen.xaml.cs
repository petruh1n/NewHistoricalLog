using System;
using System.Windows;
using DevExpress.Xpf.Core;
using System.Windows.Interop;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Bars;

namespace NewHistoricalLog.Views
{
    /// <summary>
    /// Interaction logic for MainScreen.xaml
    /// </summary>
    public partial class MainScreen : ThemedWindow
    {
        static MainScreen()
        {
            EditorLocalizer.Active = new EditorLocalizerEx();
        }
        public MainScreen()
        {
            InitializeComponent();
            //Loaded += MainScreen_Loaded;
        }

        //private void MainScreen_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var hwnd = new WindowInteropHelper(this).Handle;
            
        //    WINAPI.SetWindowLong(hwnd, WINAPI.GWL_STYLE, WINAPI.GetWindowLong(hwnd, WINAPI.GWL_STYLE) & ~WINAPI.WS_SYSMENU);
        //}

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WINAPI.WndProc);
        }

        private void TableView_CustomRowAppearance(object sender, DevExpress.Xpf.Grid.CustomRowAppearanceEventArgs e)
        {
            e.Result = e.ConditionalValue;
            e.Handled = true;
        }

        private void BarButtonItem_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            switch((sender as BarButtonItem).Content.ToString())
            {
                case "Сортировка по возрастанию":
                    ((sender as BarButtonItem).DataContext as DevExpress.Xpf.Grid.GridColumnMenuInfo).Column.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
                    break;
                case "Сортировка по убыванию":
                    ((sender as BarButtonItem).DataContext as DevExpress.Xpf.Grid.GridColumnMenuInfo).Column.SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
                    break;
                case "Снять сортировку":
                    ((sender as BarButtonItem).DataContext as DevExpress.Xpf.Grid.GridColumnMenuInfo).Column.SortOrder = DevExpress.Data.ColumnSortOrder.None;
                    break;
                default:
                    foreach(var column in gridControl.Columns)
                    {
                        column.SortOrder = DevExpress.Data.ColumnSortOrder.None;
                    }                   
                    break;
            }
            
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
            this.AddString(EditorStringId.LookUpPrevious, "Пред.");
            this.AddString(EditorStringId.LookUpNext, "След.");
        }
    }
}
