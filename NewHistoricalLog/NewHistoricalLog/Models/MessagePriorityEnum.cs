using System.ComponentModel;

namespace NewHistoricalLog.Models
{
    public enum MessagePriorityEnum
    {
        [Description("����������")]
        Normal = 1,
        [Description("������")]
        Low = 2,
        [Description("�������")]
        Middle = 3,
        [Description("�������")]
        High = 4
    }
}
