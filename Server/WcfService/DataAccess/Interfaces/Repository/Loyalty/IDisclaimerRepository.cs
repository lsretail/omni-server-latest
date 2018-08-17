
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IDisclaimerRepository
    {
        Disclaimer DisclaimerGetById(string code);
    }
}
