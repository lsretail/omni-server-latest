using System;
using System.Linq;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    public class ShoppingListLine : Entity
    {
        #region Member variables

        private LoyItem item;
        private decimal quantity;
        private bool marked;
        private VariantRegistration variantReg;
        private UnitOfMeasure uom;

        #endregion

        #region Properties

        public LoyItem Item
        {
            get { return item; }
            set { item = value; }
        }

        public decimal Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        public bool Marked
        {
            get { return marked; }
            set { marked = value; }
        }

        public string FormatQty
        {
            get { return this.FormatQuantity(quantity); }
        }

        public UnitOfMeasure Uom
        {
            get { return uom; }
            set { uom = value; } 
        }

        public VariantRegistration VariantReg
        {
            get { return variantReg; }
            set { variantReg = value; }
        }

        public ImageView Image
        {
            get
            {
                if (VariantReg != null && VariantReg.Images != null && VariantReg.Images.Count > 0)
                {
                    return VariantReg.Images[0];
                }
                if (Item != null && Item.Images != null && Item.Images.Count > 0)
                {
                    return Item.Images[0];
                }
                return null;
            }
        }

        #endregion

        #region Constructors

        public ShoppingListLine()
            : this(null)
        {
            item = null;
            quantity = 0M;
            uom = null;
            variantReg = null;
        }

        public ShoppingListLine(string id)
            : base(id)
        {
            item = null;
            quantity = 0M;
            uom = null;
            variantReg = null;
        }

        #endregion

        public string FormatQuantity(decimal qty)
        {
            /*string returnString = "";
            if (uom==null)
                returnString += qty.ToString("N0");
            else
                returnString += qty.ToString("N" + uom.Decimals) + " " + uom.ShortDescription.ToLower();
            return returnString;*/

            string returnString = "";
            if (uom == null)
                returnString += qty.ToString("N0");
            else
            {
                string formatString = "0";
                if (uom.Decimals > 0)
                {
                    formatString += ".";
                    for (int i = 0; i < uom.Decimals; i++)
                    {
                        formatString += "#";
                    }
                }
                returnString = qty.ToString(formatString) + " " + uom.ShortDescription.ToLower();
            }
            return returnString;
        }
        public ShoppingListLine ShallowCopy()
        {
            return (ShoppingListLine)MemberwiseClone();
        }
    }
}

