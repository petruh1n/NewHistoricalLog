using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHistoricalLog
{
	public class MessageGridContent
	{
		public DateTime Date { get; set; }
		public string Text { get; set; }
		public string User { get; set; }
		public string Source { get; set; }
		public string Value { get; set; }
		public MessageType Type { get; set; }
	}
	public enum MessageType
	{
		red=1,
		yellow,
		green,
		gray
	}
}
