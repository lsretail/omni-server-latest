using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Validation : Entity, IDisposable
    {
        public Validation(string id) : base(id)
        {
            Image = new ImageView();
        }

        public Validation() : this(string.Empty)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Image != null)
                    Image.Dispose();
            }
        }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public DateTime? ValidFrom { get; set; }
        [DataMember]
        public DateTime? ValidTo { get; set; }
        [DataMember]
        public ImageView Image { get; set; }
    }
}
