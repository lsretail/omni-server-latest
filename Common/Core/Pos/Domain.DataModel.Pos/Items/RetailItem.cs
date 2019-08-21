using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Pos.Items.SpecialCase;

namespace LSRetail.Omni.Domain.DataModel.Pos.Items
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017"), KnownType(typeof(UnknownRetailItem))]
    public class RetailItem : Item
    {
        #region Constructors

        public RetailItem()
            : this(null)
        {
        }

        public RetailItem(string id)
            :base( id)
        {
            //description = string.Empty;
            Barcode = null;
            //price = null;
            TaxGroupId = string.Empty;
            //unitOfMeasure = null;
            SelectedVariant = null;
            Variants = new List<VariantRegistration>();
            //discountAllowed = true;
            ZeroPriceValid = true;
            ScaleItem = false;
            MustKeyInPrice = false;
            MustKeyInQty = false;
            MustKeyInComment = false;
            Blocked = false;
            CrossSellingExists = false;
			//unitPrice = 0;
            BlockedDate = null;
            ActivateDate = null;
            Images = new List<ItemImage>();
            /*
	            [TAXITEMGROUPID] [nvarchar](10) NULL,
	        */
        }

        #endregion

        #region Properties

        [DataMember]
        public Barcode Barcode { get; set; }
        [DataMember]
        public string TaxGroupId { get; set; }
        [DataMember]
        public VariantRegistration SelectedVariant { get; set; }
        [DataMember]
        public List<VariantRegistration> Variants { get; set; }
        [DataMember]
        public List<ExtendedVariant> ExtendedVariants { get; set; }
        [DataMember]
        public bool ZeroPriceValid { get; set; }
        [DataMember]
        public bool ScaleItem { get; set; }
        [DataMember]
        public bool MustKeyInPrice { get; set; }
        [DataMember]
        public bool MustKeyInQty { get; set; }
        [DataMember]
        public bool MustKeyInComment { get; set; }
        [DataMember]
        public bool Blocked { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? BlockedDate { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? ActivateDate { get; set; }
        [DataMember]
        public List<ItemImage> Images { get; set; }
        [DataMember]
        public bool CrossSellingExists { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        //		public decimal UnitPrice
        //		{
        //			get { return this.unitPrice; }
        //			set { this.unitPrice = value; }
        //		}
        

        #endregion
    }
}
