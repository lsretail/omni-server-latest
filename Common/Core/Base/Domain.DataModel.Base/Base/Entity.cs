using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Base
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public abstract class Entity : IEntity
    {
        [DataMember]
        public string Id { get; set; }

        protected Entity()
            : this(null)
        {
        }

        protected Entity(string id)
        {
            Id = (id == null) ? NewKey() : id;
        }

        private string NewKey()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }
    }
}
