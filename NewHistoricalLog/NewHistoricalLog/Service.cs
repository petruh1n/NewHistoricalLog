using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using NLog;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using Microsoft.Win32;

namespace NewHistoricalLog
{
	public class Service
	{
        static Logger logger = LogManager.GetCurrentClassLogger();
        static bool isAdminMode = false;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void NotifyStaticPropertyChanged(string propertyName)
        {
            if (StaticPropertyChanged != null)
                StaticPropertyChanged(null, new PropertyChangedEventArgs(propertyName));
        }
        static bool isOperating = true;

        #region Global properties
        public static bool IsOperating
        {
            get { return isOperating; }
            set { isOperating = value; NotifyStaticPropertyChanged("IsOperating"); }
        }
        /// <summary>
        /// Глобальная коллекция сообщений
        /// </summary>
        public static ObservableCollection<MessageGridContent> Messages { get; set; } = new ObservableCollection<MessageGridContent>();
        /// <summary>
        /// Стартовая дата выборки
        /// </summary>
        public static DateTime StartDate { get; set; } = new DateTime(2017,3,7,10,0,0,0,DateTimeKind.Local);
        /// <summary>
        /// Конечная дата выборки
        /// </summary>
        public static DateTime EndDate { get; set; } = new DateTime(2017, 3, 7, 20, 0, 0, 0, DateTimeKind.Local);
        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        public static string ConnectionString { get; set; } = "Data Source=ORPO-165\\MYSERVER;Integrated Security=False; User = ORPO; Password = Bzpa/123456789; Initial Catalog = SARD_BEREZOVOE; Connection Timeout = 3;";
        /// <summary>
        /// Столбец, по которому осуществляется текстовая фильтрация
        /// </summary>
        public static string FilterField { get; set; } = "Text";
        /// <summary>
        /// Фраза для текстового фильтра
        /// </summary>
        public static string TextFilterPhrase { get; set; } = "";
        /// <summary>
        /// Фраза для фильтра по подсистемам
        /// </summary>
        public static string SystemsFilterPhrase { get; set; } = "";
        /// <summary>
        /// Фраза для фильтра по приоритетам
        /// </summary>
        public static string PriorityFilterPhrase { get; set; } = "";
        /// <summary>
        /// Массив приоритетов сообщений
        /// </summary>
        public static bool[] Priorities { get; set; } = new bool[] { true, true, true, true };
        /// <summary>
        /// Верхний левый угол экрана. Отступ сверху. 
        /// </summary>
        public static double Top { get; set; }
        /// <summary>
        /// Верхний левый угол экрана. Отступ слева. 
        /// </summary>
        public static double Left { get; set; }
        /// <summary>
        /// Ширина окна
        /// </summary>
        public static double Width { get; set; }
        /// <summary>
        /// Высота окна
        /// </summary>
        public static double Height { get; set; }
        /// <summary>
        /// Число строк в гриде
        /// </summary>
        public static int CountLines { get; set; } = 50;
        /// <summary>
        /// Монитор, на котором будет открыто окно приложения
        /// </summary>
        public static int Monitor { get; set; } = 0;
        /// <summary>
        /// Путь на ДМЗ
        /// </summary>
        public static string DmzPath { get; set; } = "";
        /// <summary>
        /// Нужно ли использовать экранную клавиатуру
        /// </summary>
        public static bool KeyboardNeeded { get; set; } = false;
        /// <summary>
        /// Нужно ли переносить текст в гриде на новую строку
        /// </summary>
        public static bool GridTextWrapping { get; set; } = false;
        /// <summary>
        /// Список таблиц, в которых находятся подситемы
        /// </summary>
        public static string TabsForScan { get; set; } = "ZDV-Задвижки;VS-Вспомсистемы;";

        public static string HighPrioriry { get; set; } = "Высокий";

        public static string MiddlePrioriry { get; set; } = "Средний";

        public static string LowPrioriry { get; set; } = "Низкий";

        public static string NormalPrioriry { get; set; } = "Нормальный";

        public static bool IsAdminMode
        {
            get { return isAdminMode; }
            set
            {
                isAdminMode = value;
                NotifyStaticPropertyChanged("IsAdminMode");
            }
        }
        #endregion


        #region Settings

        public static Dictionary<string, object> GetSettingsDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("Отступ сверху", Top);
            result.Add("Отступ слева", Left);
            result.Add("Ширина", Width);
            result.Add("Высота", Height);
            result.Add("Строка подключения к БД", ConnectionString);
            result.Add("Количество строк", CountLines);
            result.Add("Путь к ДМЗ", DmzPath);
            result.Add("Использовать экранную клавиатуру", KeyboardNeeded);
            result.Add("Переносить текст в таблице", GridTextWrapping);
            result.Add("Таблицы для массива подсистем", TabsForScan);
            result.Add("Обозначние высокого приоритета", HighPrioriry);
            result.Add("Обозначние среднего приоритета", MiddlePrioriry);
            result.Add("Обозначние низкого приоритета", LowPrioriry);
            result.Add("Обозначние нормального приоритета", NormalPrioriry);
            return result;
        }
        public static void ParseDictionary(Dictionary<string, object> dictionary)
        {
            try
            {
                Top = Convert.ToDouble(dictionary["Отступ сверху"]);
                Left = Convert.ToDouble(dictionary["Отступ слева"]);
                Width = Convert.ToDouble(dictionary["Ширина"]);
                Height = Convert.ToDouble(dictionary["Высота"]);
                ConnectionString = dictionary["Строка подключения к БД"].ToString();
                CountLines = Convert.ToInt32(dictionary["Количество строк"]);
                DmzPath = dictionary["Путь к ДМЗ"].ToString();
                KeyboardNeeded = Convert.ToBoolean(dictionary["Использовать экранную клавиатуру"]);
                GridTextWrapping = Convert.ToBoolean(dictionary["Переносить текст в таблице"]);
                TabsForScan = dictionary["Таблицы для массива подсистем"].ToString();
                HighPrioriry = dictionary["Обозначние высокого приоритета"].ToString();
                MiddlePrioriry = dictionary["Обозначние среднего приоритета"].ToString();
                LowPrioriry = dictionary["Обозначние низкого приоритета"].ToString();
                NormalPrioriry = dictionary["Обозначние нормального приоритета"].ToString();
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Ошибка чтения настроек: {0}", ex.Message));
            }
        }
        /// <summary>
        /// Читаем настройки из XML-файла
        /// </summary>
        public static void ReadSettings()
        {
            try
            {
                var dict = GetSettingsDictionary();
                XDocument xdoc = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SEMHistory\\Config.xml");
                foreach (XElement Element in xdoc.Element("ApplicationConfiguration").Element("Settings").Elements("Setting"))
                {
                    XElement SettingName = Element.Element("Name");
                    XElement SettingValue = Element.Element("Value");
                    if (SettingName != null && SettingValue != null)
                    {
                        object temp = new object();
                        if (dict.ContainsKey(SettingName.Value))
                        {
                            dict[SettingName.Value] = SettingValue.Value;
                        }
                    }
                }
                ParseDictionary(dict);
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Ошибка чтения файла конфигурации: {0}", ex.Message));
            }
        }
        /// <summary>
        /// Сохраняем настройки в XML-файл
        /// </summary>
        public static void SaveSettings()
        {
            try
            {
                //получили словарь настроек
                var dict = GetSettingsDictionary();
                //открыли документ
                XDocument xdoc = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SEMHistory\\Config.xml");
                foreach (var setting in dict)
                {
                    bool flag = false;
                    int counter = 0;
                    foreach (XElement Element in xdoc.Element("ApplicationConfiguration").Elements("Settings").Elements("Setting"))
                    {
                        XElement SettingName = Element.Element("Name");
                        XElement SettingValue = Element.Element("Value");
                        counter++;
                        if (setting.Key == SettingName.Value)
                        {
                            Element.SetElementValue(SettingValue.Name, dict[SettingName.Value]);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        xdoc.Element("ApplicationConfiguration").Element("Settings").Add(new XElement("Setting", new XElement("ID", counter + 1), new XElement("Name", setting.Key), new XElement("Value", setting.Value), new XElement("Description")));
                    }
                }

                xdoc.Save(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SEMHistory\\Config.xml");
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Ошибка записи файла конфигурации: {0}", ex.Message));
            }

        }
        static bool ReadKey()
        {
            try
            {
                RegistryKey usersKey = Registry.Users.OpenSubKey(".Default");
                RegistryKey settingsKey = usersKey.OpenSubKey("SEMSettings");
                ConnectionString = string.Format("Data Source={0};Integrated Security=False; User = {1}; Password = {2}; Initial Catalog = {3}; Connection Timeout = 3;", 
                    settingsKey.GetValue("SQL server").ToString(), settingsKey.GetValue("SQL user").ToString(), settingsKey.GetValue("SQL password").ToString(), 
                    settingsKey.GetValue("SQL database").ToString());
                settingsKey.Close();
                usersKey.Close();                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void GenerateXMLConfig()
        {
            try
            {
                ReadKey();
                #region Config.xml
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SEMHistory");
                XmlTextWriter textWritter = new XmlTextWriter(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SEMHistory\\Config.xml", Encoding.UTF8);
                textWritter.WriteStartDocument();
                textWritter.WriteStartElement("ApplicationConfiguration");
                textWritter.WriteEndElement();
                textWritter.Close();
                //получили словарь настроек
                var dict = GetSettingsDictionary();
                XDocument xDoc = new XDocument();
                var parentElement = new XElement("ApplicationConfiguration");
                var settingsGroupElement = new XElement("Settings");
                int counter = 0;
                foreach (var setting in dict)
                {
                    settingsGroupElement.Add(new XElement("Setting", new XElement("ID", counter), new XElement("Name", setting.Key), new XElement("Value", setting.Value), new XElement("Description")));
                    counter++;
                }
                counter = 0;

                parentElement.Add(settingsGroupElement);
                xDoc.Add(parentElement);
                //xDoc.WriteTo(new XmlTextWriter(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SEMTrends\\Config.xml", Encoding.UTF8));
                xDoc.Save(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\SEMHistory\\Config.xml");
                #endregion
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Ошибка генерации файла конфигурации: {0}", ex.Message));
            }
        }

        #endregion
    }
}
