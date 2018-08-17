using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Menu;

namespace LSRetail.Omni.Domain.DataModel.Base.Favorites
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017"), KnownType(typeof(MenuItem)), KnownType(typeof(MenuDeal)), KnownType(typeof(Recipe)), KnownType(typeof(Product))]
    public abstract class Favorite : Entity, IFavorite
    {
        [DataMember]
        public virtual string Name { get; set; }

        protected Favorite()
        {
        }

        protected Favorite(string id)
            : base(id)
        {
        }

        public virtual bool Equals(IFavorite favorite)
        {
            return base.Equals(favorite);
        }
    }
}
