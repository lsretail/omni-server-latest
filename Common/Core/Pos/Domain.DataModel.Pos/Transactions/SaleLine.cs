using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Pos.Items;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Calculators.Price;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Calculators.Tax;
using LSRetail.Omni.Domain.DataModel.Pos.Transactions.Discounts;
using LSRetail.Omni.Domain.DataModel.Pos.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017"), KnownType(typeof(RetailItem))]
    public class SaleLine : BaseLine
    {
        public SaleLine() : this(0)
        {

        }

        public SaleLine(int lineNumber) : base(lineNumber)
        {
            UnitPrice = new Money();
            UnitPriceWithTax = new Money();
            GrossAmount = new Money();
            NetAmount = new Money();
            TaxAmount = new Money();

            LineDiscount = new Discount(null, 0, 0, DiscountEntryType.Amount);
            ManualDiscount = new Discount(null, 0, 0, DiscountEntryType.Amount);
            Discounts = new List<DiscountLine>();
        }

        // This one is for serialization
        public SaleLine(int lineNumber, Money unitPrice, Money unitPriceWithTax, decimal qty, Money netAmount, Money grossAmount, Money taxAmount, bool priceOverridden, bool voided, Discount discountAmount, Discount manualDiscount)
            : base(lineNumber)
        {
            this.UnitPriceWithTax = unitPriceWithTax;
            this.UnitPrice = unitPrice;
            this.Quantity = qty;
            this.NetAmount = netAmount;
            this.TaxAmount = taxAmount;
            this.GrossAmount = grossAmount;
            this.PriceOverridden = priceOverridden;

            LineDiscount = discountAmount;
            ManualDiscount = manualDiscount;
            Discounts = new List<DiscountLine>();

            Voided = voided;
        }

        public SaleLine(int lineNumber, Item item, decimal qty, Currency currency, PriceCalculator priceCalculator, bool isReturnLine = false) : base(lineNumber)
        {
            this.PriceCalculator = priceCalculator;

            this.Dirty = true;
            this.ContainsExternalPrices = false;
            this.ContainsExternalValues = false;
            this.Item = item;
            this.Quantity = qty;

            this.UnitPriceWithTax = new Money(0, currency);// new Money(item.Price.Value, currency);
            this.UnitPrice = new Money(0, currency);
            this.PriceOverridden = false;
            this.TaxAmount = new Money(0, currency);

            this.GrossAmount = new Money(0, currency);
            this.NetAmount = new Money(0, currency);

            LineDiscount = new Discount(currency, 0, 0, DiscountEntryType.Amount);
            ManualDiscount = new Discount(null, 0, 0, DiscountEntryType.Amount);
            Discounts = new List<DiscountLine>();

            this.IsReturnLine = isReturnLine;
            this.FromRecommendation = false;
            Voided = false;
        }

        [DataMember]
        public bool Dirty { get; set; }                         // True if the SaleLine contains changes that warrant a calculation of the line
        [DataMember]
        public bool ContainsExternalPrices { get; set; }
        [DataMember]
        public bool ContainsExternalValues { get; set; }          // True if the SaleLine contains data from the back-end server calculation and not from the local database on the device.

        [IgnoreDataMember]
        public PriceCalculator PriceCalculator { get; set; }
        //private TaxCalculator TaxCalculator;

        [DataMember]
        internal RetailItem retailItem { get; set; }
        [DataMember]
        public string ExternalId { get; set; }

        [DataMember]
        public Item Item
        {
            get
            {
                return retailItem;
            }
            set
            {
                if (value is RetailItem)
                {
                    retailItem = (RetailItem)value;
                }
            }
        }

        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal ReturnQuantity { get; set; }

        [DataMember]
        public bool PriceOverridden { get; set; }
        [DataMember]
        public Money UnitPrice { get; set; }
        [DataMember]
        public Money UnitPriceWithTax { get; set; }
        [DataMember]
        public Money GrossAmount { get; set; }
        [DataMember]
        public Money NetAmount { get; set; }
        [DataMember]
        public Money TaxAmount { get; set; }

        [DataMember]
        public Discount ManualDiscount { get; set; }
        [DataMember]
        public Discount LineDiscount { get; set; }
        [DataMember]
        public List<DiscountLine> Discounts { get; set; }

        //Return by receipt field
        [DataMember]
        public int OriginalTransactionLineNo { get; set; }
        [DataMember]
        public bool FromRecommendation { get; set; }

        [DataMember]
        public bool IsReturnLine { get; set; }

        public void Void()
        {
            Voided = true;
            ContainsExternalValues = false;
            Dirty = true;
        }

        public void Unvoid()
        {
            Voided = false;
            ContainsExternalValues = false;
            Dirty = true;
        }

        public void IncrementQty()
        {
            if (this.Quantity >= MPOSFeatures.MaximumQty)
                return;

            this.Quantity++;
            ContainsExternalValues = false;
            Dirty = true;
        }

        public void DecrementQty()
        {
            this.Quantity--;
            ContainsExternalValues = false;
            Dirty = true;
        }

        public void SetQty(decimal qty)
        {
            this.Quantity = Math.Min(qty, MPOSFeatures.MaximumQty);
            ContainsExternalValues = false;
            Dirty = true;
        }

        public void SetUOM(UnitOfMeasure uom)
        {
            this.Item.UnitOfMeasure = uom;
        }

        public void PriceOverride(decimal price)
        {
            this.UnitPriceWithTax.Value = price;
            this.PriceOverridden = true;
            ContainsExternalPrices = false;
            ContainsExternalValues = false;
            Dirty = true;
        }

        public void SetManualDiscount(decimal discountAmount, decimal percentage, DiscountEntryType discountEntryType)
        {
            ManualDiscount = new Discount(null, discountAmount, percentage, discountEntryType);
            Discounts.Clear();
            LineDiscount.Amount.Value = 0;
            LineDiscount.Percentage = 0;
            ContainsExternalValues = false;
            Dirty = true;
        }

        public void ImportExternalValues(decimal unitPrice, decimal unitPriceWithTax, decimal netAmount, decimal taxAmount, Discount discountAmount, Discount manualDiscountAmount)
        {
            System.Diagnostics.Debug.WriteLine("SaleLine.ImportExternalValues() - Importing external values ...");
            System.Diagnostics.Debug.WriteLine("SaleLine.ImportExternalValues() - UnitPrice: " + unitPrice.ToString());
            System.Diagnostics.Debug.WriteLine("SaleLine.ImportExternalValues() - UnitPriceWithTax: " + unitPriceWithTax.ToString());
            System.Diagnostics.Debug.WriteLine("SaleLine.ImportExternalValues() - NetAmount: " + netAmount.ToString());
            System.Diagnostics.Debug.WriteLine("SaleLine.ImportExternalValues() - TaxAmount: " + taxAmount.ToString());
            System.Diagnostics.Debug.WriteLine("SaleLine.ImportExternalValues() - Discount: " + discountAmount.ToString());
            System.Diagnostics.Debug.WriteLine("SaleLine.ImportExternalValues() - ManualDiscount: " + manualDiscountAmount.ToString());

            this.UnitPrice.Value = unitPrice;
            this.UnitPriceWithTax.Value = unitPriceWithTax;
            this.NetAmount.Value = netAmount;
            this.TaxAmount.Value = taxAmount;
            this.LineDiscount = discountAmount;
            this.ManualDiscount = manualDiscountAmount;
            this.Discounts.Clear();

            this.ContainsExternalPrices = true;
            this.ContainsExternalValues = true;
            Dirty = true;
        }

        public void Calculate(TaxCalculator taxCalculator, RetailTransaction transaction)
        {
            // We do not need to recalculate a line that has not changed
            if (Dirty == false)
                return;

            // 1 - Establish the prices
            GetPrices(taxCalculator, transaction);

            // 2 - Calculate the gross amounts
            this.GrossAmount.Value = UnitPriceWithTax.Value * Quantity;

            // 3 - Handle all discounts
            if (Item is RetailItem)
            {
                if (ManualDiscount.Amount.Value > 0 || ManualDiscount.Percentage > 0)
                {
                    LineDiscount.Amount.Value = ManualDiscount.Percentage > 0 ? (ManualDiscount.Percentage / 100) * (GrossAmount.Value) : ManualDiscount.Amount.Value;
                }
            }
            Dirty = false;
        }

        /// <summary>
        /// Populates the following member variables: unitPrice & unitPriceWithTax
		/// with data from the item contained in the saleline
        /// </summary>
        /// <param name="taxCalculator"></param>
        /// <param name="transaction"></param>
        private void GetPrices(TaxCalculator taxCalculator, RetailTransaction transaction)
        {
            // If the prices are coming from an external back-end system we use these values as is
            if (this.ContainsExternalPrices)
                return;

            // Look up the prices from the local database
            decimal taxPercentage = taxCalculator.GetItemTaxPercentage(this.Item as RetailItem, transaction);
            decimal taxPercentageCoefficient = taxPercentage / 100;

            Item = PriceCalculator.CalculateItemPrice(Item as RetailItem, transaction);
            if (PriceOverridden == false)
                this.UnitPriceWithTax = this.Item.Price;
            this.UnitPrice.Value = this.UnitPriceWithTax.Value / (1 + taxPercentageCoefficient);
        }

        public static SaleLine FromRepository(
            int lineNumber,
            Item item,
            decimal qty,
            Currency currency,
            PriceCalculator priceCalculator,
            bool isReturnLine,
            bool isVoided,
            decimal manualPrice,
            List<Discount> discounts,
            decimal manualDiscountPercentige,
            decimal manualDiscountAmount,
            decimal netPrice,
            decimal price,
            decimal netAmount,
            decimal taxAmount,
            int seatId,
            string externalIdRO,
            string kitchenRoutingRO,
            string lineKitchenStatusRO,
            string lineKitchenStatusCodeRO,
            string itemDescriptionRO,
            string menuTypeId,
            string menuTypeCode)
        {
            SaleLine saleLine = new SaleLine(lineNumber, item, qty, currency, priceCalculator, isReturnLine);
            saleLine.Voided = isVoided;
            if (manualPrice != 0)
            {
                saleLine.PriceOverride(manualPrice);
            }

            #region Discounts

            if (manualDiscountPercentige > 0)
                saleLine.SetManualDiscount(0, manualDiscountPercentige, DiscountEntryType.Percentage);
            else if (manualDiscountAmount > 0)
                saleLine.SetManualDiscount(manualDiscountAmount, 0, DiscountEntryType.Amount);

            saleLine.Discounts.Clear();

            saleLine.ContainsExternalPrices = true;
            saleLine.ContainsExternalValues = true;

            #endregion

            decimal actualNetPrice = netPrice;
            decimal actualPrice = price;
            decimal actualNetAmount = netAmount;
            decimal actualTaxAmount = taxAmount;

            saleLine.UnitPrice.Value = actualNetPrice;
            saleLine.UnitPriceWithTax.Value = actualPrice;

            saleLine.NetAmount.Value = actualNetAmount;

            saleLine.TaxAmount.Value = actualTaxAmount;

            return saleLine;
        }
    }
}