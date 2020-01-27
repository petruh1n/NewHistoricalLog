using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
//using System.Drawing;
using System.Data.SqlClient;
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
        public string Kvited { get; set; }
        public string Prior { get; set; }


        public static ObservableCollection<MessageGridContent> LoadMessages(DateTime starTime, DateTime finishTime)
        {
            ObservableCollection<MessageGridContent> result = new ObservableCollection<MessageGridContent>();
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = Service.SqlConnectionString;
                string querry = string.Format("SELECT DTime, Message, UserName, Place, Value, Priority, DTimeAck FROM dbo.PLCMessage WHERE DTime>=CAST ('{0}' as datetime2) AND DTime<=CAST ('{1}' as datetime2) ORDER BY ID", starTime, finishTime);
                //var data = SQL.GetDataList(connection, 
                //    string.Format("SELECT DTime, Message, UserName, Place, Value, Priority, DTimeAck FROM dbo.PLCMessage WHERE DTime>=CAST ('{0}' as datetime2) AND DTime<=CAST ('{1}' as datetime2) ORDER BY ID",starTime,finishTime));
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = querry;
                SqlDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    MessageGridContent gridContent = new MessageGridContent
                    {
                        Date = Convert.ToDateTime(reader["DTime"]),
                        Text = reader["Message"].ToString().Trim(),
                        User = reader["UserName"].ToString().Trim(),
                        Source = reader["Place"].ToString().Trim(),
                        Value = reader["Value"].ToString().Trim().ToUpper() == "NAN" ? "" : reader["Value"].ToString().Trim(),
                        Type = string.IsNullOrEmpty(reader["Priority"].ToString()) | Convert.ToInt32(reader["Priority"]) == 0 ? 1: Convert.ToInt32(reader["Priority"]),
                        Kvited = reader["DTimeAck"].ToString()
                    };
                    switch (gridContent.Type)
                    {
                        case 1:
                            gridContent.TypeColor = new SolidColorBrush(Color.FromRgb(192, 192, 192));
                            gridContent.Prior = Service.NormalPrioriry;
                            break;
                        case 2:
                            gridContent.TypeColor = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                            gridContent.Prior = Service.LowPrioriry;
                            break;
                        case 3:
                            gridContent.TypeColor = new SolidColorBrush(Color.FromRgb(255, 255, 0));
                            gridContent.Prior = Service.MiddlePrioriry;
                            break;
                        case 4:
                            gridContent.TypeColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                            gridContent.Prior = Service.HighPrioriry;
                            break;
                    }
                    result.Add(gridContent);
                }
                reader.Close();                
                connection.Close();
            }
            catch(Exception ex)
            {
                logger.Error(String.Format("Ошибка при получении сообщений из БД: {0}", ex.Message));
            }

            return result;
        }
	}
	//public enum MessageType
	//{
	//	red=1,
	//	yellow,
	//	green,
	//	gray
	//}

    
}
