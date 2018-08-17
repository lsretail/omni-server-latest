using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Pos.Items
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ItemImage : Entity
    {
        public ItemImage()
            : this(null)
        {
        }

        public ItemImage(string id)
            : base(id)
        {
            this.DefaultImage = false;
            this.Image = string.Empty;
			this.ImgSize = new ImageSize();
        }

        #region Properties

        [DataMember]
        public bool DefaultImage { get; set; }
        [DataMember]
        public string Image { get; set; }
        [DataMember]
        public ImageSize ImgSize { get; set; }
        [DataMember]
        public bool Crossfade { get; set; }

        #endregion
    }
}
