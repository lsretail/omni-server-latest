using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Base;

namespace Domain.Notifications
{
    public enum NotificationStatus : int
    {
        New = 0,
        Read = 1,
        Closed = 2,
    }
}
