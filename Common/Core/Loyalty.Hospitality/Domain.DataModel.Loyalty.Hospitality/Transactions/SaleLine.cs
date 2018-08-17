using System;
using System.Runtime.Serialization;
using Domain.Transactions;
using LSRetail.Omni.Domain.DataModel.Base.Menu;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Transactions
{
    public class SaleLine : BaseLine, IEquatable<SaleLine>
	{
	    //private List<ExtraInfoLine> extraInfoLines;



        //public List<ExtraInfoLine> ExtraInfoLines
        //{
        //    get { return extraInfoLines; }
        //    set { extraInfoLines = value; }
        //}

        [DataMember]
	    public MenuItem Item { get; set; }
	    [DataMember]
	    public decimal Quantity { get; set; }
	    [DataMember]
	    public string Amount { get; set; }
	    [DataMember]
	    public string DiscountAmount { get; set; }

		#region Constructors

        public SaleLine(string id)
            : base(id)
        {
            //item = null;
            Quantity = 0;
            Amount = "";
            DiscountAmount = "";
            //extraInfoLines = new List<ExtraInfoLine>();

        }

        public SaleLine()
            : base()
        {
            //item = null;
            Quantity = 0;
            Amount = "";
            DiscountAmount = "";
            //extraInfoLines = new List<ExtraInfoLine>();

        }

		/*public string FormatQuantity(decimal qty)
		{
			string returnString = "";
			if (uom == null)
				returnString += qty.ToString("N0");
			else
				returnString += qty.ToString("N" + uom.Decimals) + " " + uom.Description;
			return returnString;
		}*/
		#endregion

        public bool Equals(SaleLine saleLine)
        {
            if (Item.Equals(saleLine.Item) == false)
                return false;

            if (Quantity != saleLine.Quantity)
                return false;

            return true;
        }

		public SaleLine Clone()
		{
            SaleLine saleLineClone = (SaleLine)MemberwiseClone();
			saleLineClone.Item = this.Item.Clone();
			return saleLineClone;
		}
	}
}

