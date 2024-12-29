using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Base
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public abstract class Entity : IEntity, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        public string Id { get; set; }

        protected Entity()
            : this(null)
        {
        }

        protected Entity(string id)
        {
            Id = id == null ? NewKey() : id;
        }

        protected static string NewKey()
        {
            return Guid.NewGuid().ToString().ToUpper();
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
