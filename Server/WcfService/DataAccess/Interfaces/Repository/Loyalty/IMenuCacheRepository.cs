using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IMenuCacheRepository
    {
        MobileMenu MenuGetById(string id, string lastVersion);
        void Save(string id, string version, MobileMenu menuList);
        CacheState Validate(string id);
    }
}
