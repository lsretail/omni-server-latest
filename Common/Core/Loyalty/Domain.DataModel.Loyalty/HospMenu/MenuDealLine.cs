using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class MenuDealLine : IDisposable
    {
        public MenuDealLine(string id)
        {
            Id = id;
            DealLineItems = new List<MenuDealLineItem>();
            DealModifierGroupIds = new List<string>();
            DealModifierGroups = new List<DealModifierGroup>();
            Images = new List<ImageView>();
        }

        public MenuDealLine() : this(string.Empty)
        {
        }

        private string selectedId;

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string DefaultItemId { get; set; }
        [DataMember]
        public string DefaultLineId { get; set; }
        [DataMember]
        public decimal MinimumQuantity { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public string UnitOfMeasure { get; set; }
        [DataMember]
        public List<MenuDealLineItem> DealLineItems { get; set; }
        [DataMember]
        public List<ImageView> Images { get; set; }
        [DataMember]
        public List<string> DealModifierGroupIds { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        [DataMember]
        public string DefaultDealLineItemId { get; set; }

        [IgnoreDataMember]
        public List<DealModifierGroup> DealModifierGroups { get; set; }

        [IgnoreDataMember]
        public string SelectedId
        {
            get
            {
                if (string.IsNullOrEmpty(selectedId))
                {
                    if (string.IsNullOrEmpty(DefaultItemId))
                    {
                        if (DealLineItems == null || DealLineItems.Count == 0)
                        {
                            return string.Empty;
                        }

                        return DealLineItems[0].ItemId;
                    }

                    return DefaultItemId;
                }

                return selectedId;
            }
            set
            {
                selectedId = value;
            }
        }

        public MenuDealLine Clone()
        {
            MenuDealLine dealLine = (MenuDealLine)MemberwiseClone();

            dealLine.DealLineItems = new List<MenuDealLineItem>();
            //DealLineItems.ForEach(x => dealLine.DealLineItems.Add(x.Clone()));

            foreach (MenuDealLineItem dealLineItem in DealLineItems)
            {
                dealLine.DealLineItems.Add(dealLineItem.Clone());
            }

            dealLine.DealModifierGroups = new List<DealModifierGroup>();
            //DealModifierGroups.ForEach(x => dealLine.DealModifierGroups.Add(x.Clone()));

            foreach (DealModifierGroup dealModifierGroup in DealModifierGroups)
            {
                dealLine.DealModifierGroups.Add(dealModifierGroup.Clone());
            }

            return dealLine;
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
                DealLineItems?.Clear();
                DealModifierGroupIds?.Clear();
                DealModifierGroups?.Clear();
                Images?.Clear();
            }
        }
    }
}
