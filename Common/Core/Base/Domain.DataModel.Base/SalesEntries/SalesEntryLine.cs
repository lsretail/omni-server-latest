using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.SalesEntries
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class SalesEntryLine : Entity, IDisposable
    {
        private ImageView image;
        private bool isChecked;

        public SalesEntryLine(string id) : base(id)
        {
            LineNumber = 1;
            ItemId = string.Empty;
            VariantId = string.Empty;
            ItemDescription = string.Empty;
            VariantDescription = string.Empty;
            UomId = string.Empty;
            ItemImageId = string.Empty;
            StoreId = string.Empty;
            StoreName = string.Empty;
            ExternalId = string.Empty;
            ExtraInformation = string.Empty;
            Quantity = 1.0M;

            LineType = LineType.Item; //never change this unless you know what you are doing !

            NetPrice = 0.0M;
            Price = 0.0M;
            DiscountAmount = 0.0M;
            DiscountPercent = 0.0M;
            NetAmount = 0.0M;
            TaxAmount = 0.0M;
            Amount = 0.0M;

            //Added for SPG
            Image = new ImageView();
            IsChecked = false;
        }

        public SalesEntryLine() : this(null)
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
            }
        }

        /// <summary>
        /// External Id for Sales Line
        /// </summary>
        [DataMember]
        public string ExternalId { get; set; }
        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public int ParentLine { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }
        [DataMember]
        public string UomId { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string StoreName { get; set; }
        [DataMember]
        public bool ClickAndCollectLine { get; set; }
        [DataMember]
        public string ItemImageId { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public LineType LineType { get; set; }
        [DataMember]
        public decimal DiscountAmount { get; set; }
        [DataMember]
        public decimal DiscountPercent { get; set; }
        [DataMember]
        public string ExtraInformation { get; set; }

        [DataMember]
        public decimal NetPrice { get; set; }
        [DataMember]
        public decimal NetAmount { get; set; }
        [DataMember]
        public decimal TaxAmount { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        /// <summary>
        /// NetAmount + TaxAmount
        /// </summary>
        [DataMember]
        public decimal Amount { get; set; }

        [IgnoreDataMember]
        public ImageView Image
        {
            get => image;
            set
            {
                image = value; 
                NotifyPropertyChanged();
            }
            
        }

        [IgnoreDataMember]
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                NotifyPropertyChanged();
            }

        }

        public bool ItemHasDiscount => DiscountAmount != 0m;

        public override string ToString()
        {
            string s = string.Format("LineNumber: {0} ItemId: {1} VariantId: {2} UomId: {3} Quantity: {4} LineType: {5} Amount: {6}",
                LineNumber, ItemId, VariantId, UomId, Quantity, LineType.ToString(), Amount.ToString());
            return s;
        }
    }
}
