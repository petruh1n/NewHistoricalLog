using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
