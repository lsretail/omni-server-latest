using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Menu.SpecialCase;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    //discount offer
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class MenuDeal : MenuItem
    {
        public MenuDeal(string id) : base(id)
        {
            DealLines = new List<MenuDealLine>();
            UnknownDealLines = new List<UnknownDealLine>();
        }

        public MenuDeal() : this(string.Empty)
        {
        }

        [DataMember]
        public List<MenuDealLine> DealLines { get; set; }
        /// Unknown deal lines are containers for deallines that we get from the BO when retrieving a transaction,
        /// but aren't part of the menu that the hosp item came from,
        /// and so aren't part of the hosp item's 'normal' set of deallines (those that are defined in the menu it came from).
        public List<UnknownDealLine> UnknownDealLines { get; private set; }

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
