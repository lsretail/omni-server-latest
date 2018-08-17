using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Menu;

namespace LSRetail.Omni.Domain.DataModel.Base.Favorites
{
    public interface IFavorite : IEntity, IEquatable<IFavorite>
    {
        string Name { get; set; }
    }
}
