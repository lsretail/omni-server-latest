using System;

using LSOmni.BLL;

namespace LSOmni.Service
{
    public partial class LSOmniBase
    {
        public virtual string MyCustomFunction(string data)
        {
            CustomBLL myBLL = new CustomBLL(config);
            return myBLL.MyCustomFunction(data);
        }
    }
}
