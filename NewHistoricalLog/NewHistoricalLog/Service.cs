using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace NewHistoricalLog
{
	public class Service
	{
        static Logger logger = LogManager.GetCurrentClassLogger();

        #region Global properties
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
        public static string FilterPhrase { get; set; } = "";
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
        #endregion


        #region Settings

        public static Dictionary<string, object> GetSettingsDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("Top", Top);
            result.Add("Left", Left);
            result.Add("Width", Width);
            result.Add("Height", Height);
            result.Add("Connection string", ConnectionString);
            result.Add("Count rows in grid", CountLines);
            result.Add("DmzPath", DmzPath);
            result.Add("Need in OnScreen keyboaard", KeyboardNeeded);
            result.Add("Need in grid text wrapping", GridTextWrapping);
            return result;
        }
        public static void ParseDictionary(Dictionary<string, object> dictionary)
        {
            try
            {
                Top = Convert.ToDouble(dictionary["Top"]);
                Left = Convert.ToDouble(dictionary["Left"]);
                Width = Convert.ToDouble(dictionary["Width"]);
                Height = Convert.ToDouble(dictionary["Height"]);
                ConnectionString = dictionary["Connection string"].ToString();
                CountLines = Convert.ToInt32(dictionary["Count rows in grid"]);
                DmzPath = dictionary["DmzPath"].ToString();
                KeyboardNeeded = Convert.ToBoolean(dictionary["Need in OnScreen keyboaard"]);
                GridTextWrapping = Convert.ToBoolean(dictionary["Need in grid text wrapping"]);
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
                XDocument xdoc = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SEMHistory\\Config.xml");
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
                XDocument xdoc = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SEMHistory\\Config.xml");
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

                xdoc.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SEMHistory\\Config.xml");
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Ошибка записи файла конфигурации: {0}", ex.Message));
            }

        }
        public static void GenerateXMLConfig()
        {
            try
            {
                #region Config.xml
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SEMHistory");
                XmlTextWriter textWritter = new XmlTextWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SEMHistory\\Config.xml", Encoding.UTF8);
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
                //xDoc.WriteTo(new XmlTextWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SEMTrends\\Config.xml", Encoding.UTF8));
                xDoc.Save(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\SEMHistory\\Config.xml");
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
