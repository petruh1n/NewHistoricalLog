using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ServiceLib;
using System.IO;
using System.Xml;

namespace NewHistoricalLog.Models
{

    public class MessageSource : INotifyPropertyChanged
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
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
        private string server = "";
        public string Server
        {
            get { return server; }
            set
            {
                server = value;
                OnPropertyChanged("Server");
            }
        }
        private string database = "";
        public string Database
        {
            get { return database; }
            set
            {
                database = value;
                OnPropertyChanged("Database");
            }
        }
        private string user = "";
        public string User
        {
            get { return user; }
            set
            {
                user = value;
                OnPropertyChanged("User");
            }
        }
        private string password = "";
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }
        public string ConnectionString
        {
            get
            {
                return string.Format("Data Source={0}; Integrated Security=False; Initial Catalog={1}; User={2}; Password={3}; Connection Timeout=3;",
                Server,
                Database,
                User,
                Password);
            }
        }

        public static ObservableCollection<MessageSource> GetSources(string filepath)
        {
            ObservableCollection<MessageSource> result = new ObservableCollection<MessageSource>();
            try
            {                
                XDocument doc = XDocument.Load(filepath);
                int counter = 0;
                foreach(XElement src in doc.Element("Sources").Elements("Source"))
                {
                    try
                    {
                        XElement _name = src.Element("Name");
                        XElement _server = src.Element("Server");
                        XElement _database = src.Element("Database");
                        XElement _user = src.Element("User");
                        XElement _password = src.Element("Password");
                        if (_name != null && _server != null && _database != null && _user != null && _password != null)
                        {
                            result.Add(new MessageSource
                            {
                                Name = _name.Value,
                                Server = _server.Value,
                                Database = _database.Value,
                                User = _user.Value,
                                Password = ServiceLib.EncryptingFunctions.Decrypt(_password.Value)
                            });
                        }
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Ошибка обработки данных для источника номер {0}: {1}", counter, ex.Message);
                    }
                }
                
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка при получении источников сообщений: {0}", ex.Message);                
            }
            if (result.Count == 0)
                result.Add(GenSourceFile(filepath));
            return result;
        }
        public static MessageSource GenSourceFile(string filepath)
        {
            try
            {
                if (!Directory.Exists(new FileInfo(filepath).Directory.FullName))
                    Directory.CreateDirectory(new FileInfo(filepath).Directory.FullName);
                XmlTextWriter textWritter = new XmlTextWriter(filepath, Encoding.UTF8);
                textWritter.WriteStartDocument();
                textWritter.WriteStartElement("Sources");
                textWritter.WriteEndElement();
                textWritter.Close();
                XDocument xDoc = new XDocument();
                var parentElement = new XElement("Sources");
                parentElement.Add(new XElement("Source",
                    new XElement("Name", "По умолчанию"),
                    new XElement("Server", Service.Server),
                    new XElement("Database", Service.Database),
                    new XElement("User", Service.User),
                    new XElement("Password", ServiceLib.EncryptingFunctions.Encrypt(Service.Password))
                    ));
                xDoc.Add(parentElement);
                xDoc.Save(filepath);
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка при генерации источников сообщений: {0}", ex.Message);
            }
            return new MessageSource()
            {
                Name = "По умолчанию",
                Server = Service.Server,
                Database = Service.Database,
                User = Service.User,
                Password = Service.Password
            };
        }
    }
}
