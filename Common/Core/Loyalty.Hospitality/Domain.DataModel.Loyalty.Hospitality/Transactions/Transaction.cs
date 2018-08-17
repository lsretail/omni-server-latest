using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Favorites;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Transactions
{
    public class Transaction : Entity, IFavorite, IAggregateRoot
    {
        private string name;

        public Transaction()
            : this(null)
        {
        }

        public Transaction(string id)
            : base(id)
        {
            this.Store = null;
            this.Terminal = string.Empty;
            this.Staff = string.Empty;
            this.Amount = "";
            this.Date = null;
            SaleLines = new List<SaleLine>();
        }

        [DataMember]
        public Store Store { get; set; }

        [DataMember]
        public string Terminal { get; set; }

        [DataMember]
        public string Staff { get; set; }

        [DataMember]
        public string Amount { get; set; }

        [DataMember]
        public string NetAmount { get; set; }

        [DataMember]
        public string VatAmount { get; set; }

        [DataMember]
        public string DiscountAmount { get; set; }

        [DataMember]
        public List<SaleLine> SaleLines { get; set; }

        [DataMember]
        public DateTime? Date { get; set; }

        [DataMember]
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    return DateToFullShortFormat;
                }
                return name;
            }
            set { name = value; }
        }

        public string AmountForDisplay
        {
            //get { return amount.ToString("c"); }
            get { return Amount; }
        }

        public string DateToFullShortFormat
        {
            get
            {
                if (Date == null)
                {
                    return string.Empty;
                }
                else
                {
                    return Date.Value.ToString("f");
                }
            }

        }

        public string DateToShortFormat
        {
            get
            {
                if (Date == null)
                {
                    return string.Empty;
                }
                else
                {
                    return Date.Value.ToString("d");
                }
            }

        }

        public virtual bool Equals(IFavorite favorite)
        {
            Transaction transaction = favorite as Transaction;

            if (transaction == null)
                return false;

            if (SaleLines.Count != transaction.SaleLines.Count)
                return false;

            var difference = SaleLines.Except(transaction.SaleLines, new SaleLineEqualityComparer()).ToList();

            if (difference.Count > 0)
                return false;

            return true;
        }

        public virtual Transaction Clone()
        {
            Transaction transactionClone = (Transaction)MemberwiseClone();

            transactionClone.SaleLines = new List<SaleLine>();
            //this.SaleLines.ForEach(x => transactionClone.SaleLines.Add(x.Clone()));

            foreach (SaleLine saleLine in SaleLines)
            {
                transactionClone.SaleLines.Add(saleLine.Clone());
            }

            return transactionClone;
        }
    }
}
