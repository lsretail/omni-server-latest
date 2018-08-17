using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IAccountRepository
    {
        Account AccountGetById(string id);
    }
}
