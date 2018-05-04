using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHistoricalLog
{
	public class Service
	{
        #region Global properties
        /// <summary>
        /// Стартовая дата выборки
        /// </summary>
        public static DateTime StartDate { get; set; } = new DateTime();
        /// <summary>
        /// Конечная дата выборки
        /// </summary>
        public static DateTime EndDate { get; set; } = new DateTime();
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
        #endregion
    }
}
