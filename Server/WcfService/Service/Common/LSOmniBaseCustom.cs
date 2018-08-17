using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LSOmni.BLL;

namespace LSOmni.Service
{
    public partial class LSOmniBase
    {
        public virtual string MyCustomFunction(string data)
        {
            CustomBLL myBLL = new CustomBLL();
            return myBLL.MyCustomFunction(data);
        }
    }
}
