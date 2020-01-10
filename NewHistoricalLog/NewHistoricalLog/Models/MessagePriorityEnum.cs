using System.ComponentModel;

namespace NewHistoricalLog.Models
{
    public enum MessagePriorityEnum
    {
        [Description("Нормальный")]
        Normal = 1,
        [Description("Низкий")]
        Low = 2,
        [Description("Средний")]
        Middle = 3,
        [Description("Высокий")]
        High = 4
    }
}
