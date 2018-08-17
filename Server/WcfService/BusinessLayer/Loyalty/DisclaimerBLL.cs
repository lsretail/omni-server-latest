using System;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class DisclaimerBLL : BaseLoyBLL
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private IDisclaimerRepository iDisclaimerRepository;

        public DisclaimerBLL(int timeoutInSeconds)
            : base(timeoutInSeconds)
        {
            this.iDisclaimerRepository = GetDbRepository<IDisclaimerRepository>();
        }

        public virtual Disclaimer DisclaimerGetById(string code)
        {
            return this.iDisclaimerRepository.DisclaimerGetById(code);
        }
    }
}

 