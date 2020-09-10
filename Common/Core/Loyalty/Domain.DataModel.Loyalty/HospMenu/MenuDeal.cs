using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    //discount offer
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class MenuDeal : MenuItem
    {
        public MenuDeal(string id) : base(id)
        {
            DealLines = new List<MenuDealLine>();
        }

        public MenuDeal() : this(string.Empty)
        {
        }

        [DataMember]
        public List<MenuDealLine> DealLines { get; set; }

        public override MenuItem Clone()
        {
            MenuDeal deal = (MenuDeal)base.Clone();

            deal.DealLines = new List<MenuDealLine>();
            foreach (MenuDealLine dealLine in DealLines)
            {
                deal.DealLines.Add(dealLine.Clone());
            }
            return deal;
        }

        public override string ToString()
        {
            return string.Format("Id:{0} Des:{1} Det:{2} Pri:{3} DL.Count:{4} Img.Count:{5}",
                Id, Description, Detail, Price, DealLines.Count, Images.Count);
        }

        public override void SatisfyModifierGroupsMinSelectionRestrictions()
        {
            foreach (MenuDealLine dealLine in this.DealLines)
            {
                foreach (MenuDealLineItem dli in dealLine.DealLineItems)
                    dli.MenuItem.SatisfyModifierGroupsMinSelectionRestrictions();

                foreach (DealModifierGroup dmg in dealLine.DealModifierGroups)
                    dmg.SatisfyMinSelectionRestriction();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DealLines != null)
                    DealLines.Clear();
                if (Images != null)
                    Images.Clear();
            }

            base.Dispose(disposing);
        }
    }
}
