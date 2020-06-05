using NewHistoricalLog.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHistoricalLog.Common
{
    public class DbHelper
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Строка подключения к серверу БД
        /// </summary>
        //public static string ConnectionString
        //{
        //    get
        //    {
        //        return string.Format("Data Source={0}; Integrated Security=False; Initial Catalog={1}; User={2}; Password={3}; Connection Timeout=3;",
        //        Service.Server,
        //        Service.Database,
        //        Service.User,
        //        Service.Password);
        //    }
        //}
        /// <summary>
        /// Подключиться к БД
        /// </summary>
        /// <param name="SqlConnection"></param>
        /// <returns></returns>
        //public static SqlConnection ConnectToDatabase(MessageSource src)
        //{
        //    try
        //    {
        //        var SqlConnection = new SqlConnection();
        //        SqlConnection.ConnectionString = ConnectionString;
        //        SqlConnection.Open();
        //        return SqlConnection;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Ошибка подключения к БД: {0}", ex.Message);
        //        return null;
        //    }
        //}
        /// <summary>
        /// Подключиться к БД асинхронно
        /// </summary>
        /// <param name="SqlConnection"></param>
        /// <returns></returns>
        public async static Task<SqlConnection> ConnectToDatabaseAsync(MessageSource src)
        {
            try
            {
                var SqlConnection = new SqlConnection();
                SqlConnection.ConnectionString = src.ConnectionString;
                await SqlConnection.OpenAsync();
                return SqlConnection;
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка подключения к БД: {0}", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// Отключиться от БД
        /// </summary>
        /// <param name="SqlConnection"></param>
        /// <returns></returns>
        public static bool DisconnectFromDatabase(SqlConnection SqlConnection)
        {
            try
            {
                SqlConnection.Close();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка отключения от БД: {0}", ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Получить сообщения из БД за определнный промежуток времени
        /// </summary>
        /// <param name="startTime">Начало промежутка</param>
        /// <param name="endTime">Конец промежутка</param>
        /// <returns></returns>
        public async static Task<ObservableCollection<MessageClass>> GetMessages(DateTime startTime, DateTime endTime, MessageSource src)
        {
            try
            {
                var connection = await ConnectToDatabaseAsync(src);
                if(connection!=null)
                {
                    ObservableCollection<MessageClass> result = new ObservableCollection<MessageClass>();
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = string.Format("SELECT DTime, Message, UserName, Place, Value, Priority, DTimeAck "+
                        "FROM dbo.PLCMessage WHERE DTime>=CAST ('{0}' as datetime2) AND DTime<=CAST ('{1}' as datetime2) ORDER BY ID DESC", startTime, endTime);
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        result.Add(new MessageClass
                        {
                            Date = Convert.ToDateTime(reader["DTime"]),
                            Text = reader["Message"].ToString(),
                            User = reader["UserName"].ToString(),
                            Source = reader["Place"].ToString(),
                            MessageValue = reader["Value"].ToString(),
                            Kvited = reader["DTimeAck"].ToString(),
                            Priority = (MessagePriorityEnum)Convert.ToInt32(reader["Priority"])
                        });
                    }
                    reader.Close();
                    DisconnectFromDatabase(connection);
                    return result;
                }
                return new ObservableCollection<MessageClass>();
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка при получении сообщений за период с {0} по {1}: {2}", startTime.ToString(), endTime.ToString(), ex.Message);
                return new ObservableCollection<MessageClass>();
            }
        }
        /// <summary>
        /// Проверить группу пользователя на администратора
        /// </summary>
        /// <returns></returns>
        public async static Task GetUserData(MessageSource src)
        {
            try
            {
                var connection = await ConnectToDatabaseAsync(src);
                if (connection != null)
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = string.Format(@"SELECT Accounts.GroupID FROM Accounts INNER JOIN CurrentUser ON CAST(Accounts.Login AS VARCHAR) = CAST(CurrentUser.Login AS VARCHAR)
                                                    WHERE CurrentUser.Place = 'АРМ: {0}'", Environment.MachineName);
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while(await reader.ReadAsync())
                    {
                        MainModel.AdminMode = Convert.ToInt32(reader["GroupID"]) == Service.AdminUserGroup;
                        MainModel.IngMode = Convert.ToInt32(reader["GroupId"]) <= Service.IngUserGroup;
                    }
                    reader.Close();
                    DisconnectFromDatabase(connection);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка при получении данных пользователя: {0}", ex.Message);
            }
        }
        /// <summary>
        /// Получить данные по подсистемам
        /// </summary>
        /// <returns></returns>
        public async static Task<ObservableCollection<SubSystem>> GetSubSystemInfo(MessageSource src)
        {
            try
            {
                ObservableCollection<SubSystem> ss = new ObservableCollection<SubSystem>();
                var splited = Service.TabsForScan.Split(new char[] { ';' });
                foreach(var spl in splited)
                {
                    if(!string.IsNullOrEmpty(spl))
                    {
                        var subSplited = spl.Split(new char[] { '-' });
                        ss.Add(new SubSystem()
                        {
                            TableName = subSplited[0],
                            Description = subSplited[1],
                        });
                    }
                }
                if(ss.Count>0)
                {
                    var connection = await ConnectToDatabaseAsync(src);
                    if (connection != null)
                    {
                        foreach(var s in ss)
                        {
                            SqlCommand command = connection.CreateCommand();
                            command.CommandText = string.Format("SELECT ParamName FROM dbo.{0} WHERE ParamName <> 'Резерв'", s.TableName);
                            var reader = await command.ExecuteReaderAsync();
                            while(await reader.ReadAsync())
                            {
                                s.Devices.Add(new Device (reader["ParamName"].ToString().Trim()));
                            }
                            reader.Close();
                        }

                        DisconnectFromDatabase(connection);
                    }
                }
                return ss;
            }
            catch (Exception ex)
            {
                logger.Error("Ошибка при получении данных по подсистемам: {0}", ex.Message);
                return new ObservableCollection<SubSystem>();
            }
        }
    }
}
