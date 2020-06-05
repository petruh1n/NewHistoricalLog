using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using NLog;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using Microsoft.Win32;
using System.Windows;

namespace NewHistoricalLog
{
	public class Service
	{
        static Logger logger = LogManager.GetCurrentClassLogger();
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            if (StaticPropertyChanged != null)
                StaticPropertyChanged(null, new PropertyChangedEventArgs(propertyName));
        }
        static string dmzPath="";
        static string server = "";
        static string database = "";
        static string user = "";
        static string password = "";
        static TextWrapping wraptext = TextWrapping.Wrap;
        static int countLines = 50;
        static List<bool> columnVisibilityList = new List<bool>() { true, true, true, true, true };
        static bool keyboardneeded=false;
        static double mainFontSize=14;

        #region Global properties  

        /// <summary>
        /// Адрес сервера SQL
        /// </summary>
        public static string Server
        {
            get { return server; }
            set
            {
                server = value;
                OnStaticPropertyChanged("Server");
            }
        }        
        /// <summary>
        /// Имя БД
        /// </summary>
        public static string Database
        {
            get { return database; }
            set
            {
                database = value;
                OnStaticPropertyChanged("Database");
            }
        }        
        /// <summary>
        /// Имя пользователя БД
        /// </summary>
        public static string User
        {
            get { return user; }
            set
            {
                user = value;
                OnStaticPropertyChanged("User");
            }
        }        
        /// <summary>
        /// Пароль пользователя БД
        /// </summary>
        public static string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnStaticPropertyChanged("Password");
            }
        }              
        /// <summary>
        /// Переносить текст в сообщении
        /// </summary>
        public static TextWrapping WrapText
        {
            get { return wraptext; }
            set
            {
                wraptext = value;
                OnStaticPropertyChanged("WrapText");
            }
        }         
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
        public static int CountLines
        {
            get { return countLines; }
            set
            {
                countLines = value;
                OnStaticPropertyChanged("CountLines");
            }
        }
        /// <summary>
        /// Видимость колонок в основном гриде
        /// </summary>
        public static List<bool> ColumnVisibilityList
        {
            get { return columnVisibilityList; }
            set
            {
                columnVisibilityList = value;
                OnStaticPropertyChanged("ColumnVisibilityList");
            }
        }
        /// <summary>
        /// Монитор, на котором будет открыто окно приложения
        /// </summary>
        public static int Monitor { get; set; } = 0;
        /// <summary>
        /// Путь к ДМЗ
        /// </summary>
        public static string DmzPath
        {
            get { return dmzPath; }
            set
            {
                dmzPath = value;
                OnStaticPropertyChanged("DmzPath");
            }
        }
        /// <summary>
        /// Флаг необходимсоти экранной клавиатуры
        /// </summary>
        public static bool KeyboardNeeded
        {
            get { return keyboardneeded; }
            set
            {
                keyboardneeded = value;
                OnStaticPropertyChanged("KeyboardNeeded");
            }
        }
        /// <summary>
        /// Нужно ли переносить текст в гриде на новую строку
        /// </summary>
        public static bool GridTextWrapping { get; set; } = false;
        /// <summary>
        /// Список таблиц, в которых находятся подситемы
        /// </summary>
        public static string TabsForScan { get; set; } = "ZDV-Задвижки;VS-Вспомсистемы;";        
        /// <summary>
        /// Идентификатор группы администратора
        /// </summary>
        public static int AdminUserGroup { get; set; } = 1;
        /// <summary>
        /// Путь для сохранения по умолчанию
        /// </summary>
        public static string SavePath { get; set; } = String.Format("{0}\\SEMHistory\\Export", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
        /// <summary>
        /// Заоголовок для печати и экспорта
        /// </summary>
        public static string PrintTitle { get; set; } = "Заголовок печати и экспорта";
        public static bool FollowFix { get; set; } = true;
        /// <summary>
        /// Идентификатор группы инженера
        /// </summary>
        public static int IngUserGroup { get; set; } = 2;
        /// <summary>
        /// Размер шрифта
        /// </summary>        
        public static double MainFontSize
        {
            get { return mainFontSize; }
            set
            {
                mainFontSize = value;
                OnStaticPropertyChanged("MainFontSize");
            }
        }
        /// <summary>
        /// Разрешена запись на съемные носители
        /// </summary>
        public static bool AllowRemoteDevices { get; set; } = false;
        public static bool AllowAdminAccessToWindows { get; set; } = false;
        public static string SourceFilePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "SemSettings", "History", "Sources.xml");

        #endregion
        public static string GetStringFromBoolList(List<bool> list)
        {
            string str = "";
            for(int i=0;i<list.Count;i++)
            {
                str += string.Format("{0};", list[i]);
            }
            return str;
        }
        public static List<bool> GetBoolListFromString(string str)
        {
            List<bool> result = new List<bool>();
            var sStr = str.Split(new char[] { ';' },StringSplitOptions.RemoveEmptyEntries);
            for(int i=0;i<sStr.Length;i++)
            {
                result.Add(Convert.ToBoolean(sStr[i]));
            }
            return result;
        }
        #region Settings
        const string SettingsAdress = @"\SemSettings\History\Config.xml";
        public static Dictionary<string, object> GetSettingsDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result.Add("Отступ сверху", Top);
            result.Add("Отступ слева", Left);
            result.Add("Ширина", Width);
            result.Add("Высота", Height);
            result.Add("Адрес сервера", Server);
            result.Add("Имя БД", Database);
            result.Add("Имя пользователя БД", User);
            result.Add("Пароль пользователя БД", ServiceLib.EncryptingFunctions.Encrypt(Password));
            result.Add("Количество строк", CountLines);
            result.Add("Путь к ДМЗ", DmzPath);
            result.Add("Использовать экранную клавиатуру", KeyboardNeeded);
            result.Add("Переносить текст в таблице", WrapText==TextWrapping.Wrap);            
            result.Add("Путь для сохранения", SavePath);
            result.Add("Заголовок печати и экспорта", PrintTitle);
            result.Add("Идентификатор группы администратора",AdminUserGroup);
            result.Add("Видимость колонок", GetStringFromBoolList(ColumnVisibilityList));
            result.Add("Таблицы для фильтров", TabsForScan);
            result.Add("Следить за работой iFix", FollowFix);
            result.Add("Идентификатор группы инженера", IngUserGroup);
            result.Add("Размер шрифта", MainFontSize);
            result.Add("Разрешена запись на съемные носители", AllowRemoteDevices);
            result.Add("Администратору разрешен выход в Windows", AllowAdminAccessToWindows);
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
                Server = (dictionary["Адрес сервера"]).ToString();
                Database = (dictionary["Имя БД"]).ToString();
                User = (dictionary["Имя пользователя БД"]).ToString();
                Password = ServiceLib.EncryptingFunctions.Decrypt((dictionary["Пароль пользователя БД"]).ToString());
                CountLines = Convert.ToInt32(dictionary["Количество строк"]);
                DmzPath = dictionary["Путь к ДМЗ"].ToString();
                KeyboardNeeded = Convert.ToBoolean(dictionary["Использовать экранную клавиатуру"]);
                WrapText = Convert.ToBoolean(dictionary["Переносить текст в таблице"])?TextWrapping.Wrap:TextWrapping.NoWrap;                
                SavePath = dictionary["Путь для сохранения"].ToString();
                PrintTitle = dictionary["Заголовок печати и экспорта"].ToString();
                AdminUserGroup = Convert.ToInt32(dictionary["Идентификатор группы администратора"]);
                ColumnVisibilityList = GetBoolListFromString(dictionary["Видимость колонок"].ToString());
                TabsForScan = dictionary["Таблицы для фильтров"].ToString();
                FollowFix = Convert.ToBoolean(dictionary["Следить за работой iFix"]);
                IngUserGroup = Convert.ToInt32(dictionary["Идентификатор группы инженера"]);
                MainFontSize = Convert.ToDouble(dictionary["Размер шрифта"]);
                AllowRemoteDevices = Convert.ToBoolean(dictionary["Разрешена запись на съемные носители"]);
                AllowAdminAccessToWindows = Convert.ToBoolean(dictionary["Администратору разрешен выход в Windows"]);
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
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + SettingsAdress))
                {
                    var dict = GetSettingsDictionary();
                    XDocument xdoc = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + SettingsAdress);
                    foreach (XElement Element in xdoc.Element("ApplicationConfiguration").Element("Settings").Elements("Setting"))
                    {
                        XElement SettingName = Element.Element("Name");
                        XElement SettingValue = Element.Element("Value");
                        if (SettingName != null && SettingValue != null)
                        {
                            if (dict.ContainsKey(SettingName.Value))
                            {
                                dict[SettingName.Value] = SettingValue.Value;
                            }
                        }
                    }
                    ParseDictionary(dict);
                }
                else
                {
                    GenerateXML_Config();
                }
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
                XDocument xdoc = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + SettingsAdress);
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

                            Element.SetElementValue(SettingValue.Name, dict[SettingName.Value] == null ? "" : dict[SettingName.Value]);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        xdoc.Element("ApplicationConfiguration").Element("Settings").Add(new XElement("Setting", new XElement("ID", counter + 1), new XElement("Name", setting.Key), new XElement("Value", setting.Value), new XElement("Description")));
                    }
                }

                xdoc.Save(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + SettingsAdress);
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Ошибка записи файла конфигурации: {0}", ex.Message));
            }

        }
        /// <summary>
        /// Сгенерировать новый XML файл Config.XML
        /// </summary>
        public static void GenerateXML_Config()
        {
            try
            {
                ReadKey();
                #region Config.xml
                if (!Directory.Exists(new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + SettingsAdress).Directory.FullName))
                    Directory.CreateDirectory(new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + SettingsAdress).Directory.FullName);
                XmlTextWriter textWritter = new XmlTextWriter(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + SettingsAdress, Encoding.UTF8);
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
                xDoc.Save(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + SettingsAdress);
                #endregion
            }
            catch (Exception ex)
            {
                logger.Error(String.Format("Ошибка генерации файла конфигурации: {0}", ex.Message));
            }

        }
        static bool ReadKey()
        {
            try
            {
                RegistryKey usersKey = Registry.Users.OpenSubKey(".Default");
                RegistryKey settingsKey = usersKey.OpenSubKey("SEMSettings");
                Server = settingsKey.GetValue("SQL server").ToString();
                Database = settingsKey.GetValue("SQL database").ToString();
                User = settingsKey.GetValue("SQL user").ToString();
                Password = ServiceLib.EncryptingFunctions.Decrypt(settingsKey.GetValue("SQL password").ToString());
                settingsKey.Close();
                usersKey.Close();                
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }   
}
