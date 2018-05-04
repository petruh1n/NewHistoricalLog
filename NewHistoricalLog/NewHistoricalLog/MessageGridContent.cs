using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
//using System.Drawing;
using DatabaseToolBox;
using NLog;

namespace NewHistoricalLog
{
	public class MessageGridContent
	{
        static Logger logger = LogManager.GetCurrentClassLogger();

		public DateTime Date { get; set; }
		public string Text { get; set; }
		public string User { get; set; }
		public string Source { get; set; }
		public string Value { get; set; }
		public int Type { get; set; }
        public Brush TypeColor { get; set; }


        public static ObservableCollection<MessageGridContent> LoadMessages(DateTime starTime, DateTime finishTime)
        {
            ObservableCollection<MessageGridContent> result = new ObservableCollection<MessageGridContent>();
            try
            {
                var connection = SQL.GetSqlConnection(Service.ConnectionString);
                connection.Open();
                var data = SQL.GetDataList(connection, 
                    string.Format("SELECT DTime, Message, UserName, Place, Value, Priority FROM dbo.PLCMessage WHERE DTime>=CAST ('{0}' as datetime2) AND DTime<=CAST ('{1}' as datetime2) ORDER BY ID",starTime,finishTime));
                for(int i=0; i<data.Count;i++)
                {
                    var splited = data[i].Split(new char[] { ';' }, StringSplitOptions.None);
                    MessageGridContent gridContent = new MessageGridContent
                    {
                        Date = Convert.ToDateTime(splited[0]),
                        Text = splited[1].Trim(),
                        User = splited[2].Trim(),
                        Source = splited[3].Trim(),
                        Value = splited[4].Trim(),
                        Type = Convert.ToInt32(splited[5])
                    };
                    switch(gridContent.Type)
                    {
                        case 1:
                            gridContent.TypeColor = new SolidColorBrush(Colors.Gray);
                            break;
                        case 2:
                            gridContent.TypeColor = new SolidColorBrush(Colors.Green);
                            break;
                        case 3:
                            gridContent.TypeColor = new SolidColorBrush(Colors.Yellow);
                            break;
                        case 4:
                            gridContent.TypeColor = new SolidColorBrush(Colors.Red);
                            break;
                    }
                    result.Add(gridContent);
                }
            }
            catch(Exception ex)
            {
                logger.Error(String.Format("Ошибка при получении сообщений из БД: {0}", ex.Message));
            }

            return result;
        }
	}
	public enum MessageType
	{
		red=1,
		yellow,
		green,
		gray
	}

    
}
