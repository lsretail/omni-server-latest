using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    public static class TaskExtensions
    {
        public static async Task<bool> TimeoutAfter(this Task<bool> task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
                return await task;
            else
                throw new TimeoutException();
        }
    }
}
