using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscountResponse : IDisposable
    {
        public ReplDiscountResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Discounts = new List<ReplDiscount>();
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
                if (Discounts != null)
                    Discounts.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplDiscount> Discounts { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscount : IDisposable
    {
        public ReplDiscount()
        {
            IsDeleted = false;
            PriorityNo = 0;
            ItemId = string.Empty;
            VariantId = string.Empty;
            CustomerDiscountGroup = string.Empty;
            LoyaltySchemeCode = string.Empty;
            FromDate = new DateTime(1900, 1, 1);
            ToDate = new DateTime(1900, 1, 1);
            ModifyDate = DateTime.Now;
            UnitOfMeasureId = string.Empty;
            MinimumQuantity = 0M;
            CurrencyCode = string.Empty;
            DiscountValue = 0M;
            OfferNo = string.Empty;
            StoreId = string.Empty;
            Description = string.Empty;
            Details = string.Empty;
            Type = ReplDiscountType.Unknown; //Disc. Offer, Multibuy
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

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public int PriorityNo { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string CustomerDiscountGroup { get; set; }
        [DataMember]
        public string LoyaltySchemeCode { get; set; }
        [DataMember]
        public DateTime FromDate { get; set; }
        [DataMember]
        public DateTime ToDate { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public decimal MinimumQuantity { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public DiscountValueType DiscountValueType { get; set; }
        [DataMember]
        public decimal DiscountValue { get; set; }
        [DataMember]
        public string OfferNo { get; set; }
        [DataMember]
        public DateTime ModifyDate { get; set; }
        [DataMember]
        public ReplDiscountType Type { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Details { get; set; }
        [DataMember]
        public int ValidationPeriodId { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscountValidationResponse : IDisposable
    {
        public ReplDiscountValidationResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            DiscountValidations = new List<ReplDiscountValidation>();
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
                if (DiscountValidations != null)
                    DiscountValidations.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplDiscountValidation> DiscountValidations { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscountValidation : IDisposable
    {
        public ReplDiscountValidation()
        {
            Id = string.Empty;
            Description = string.Empty;
            StartDate = new DateTime(1900, 1, 1);
            EndDate = new DateTime(1900, 1, 1);
            StartTime = new DateTime(1900, 1, 1);
            EndTime = new DateTime(1900, 1, 1);
            MondayStart = new DateTime(1900, 1, 1);
            MondayEnd = new DateTime(1900, 1, 1);
            TuesdayStart = new DateTime(1900, 1, 1);
            TuesdayEnd = new DateTime(1900, 1, 1);
            WednesdayStart = new DateTime(1900, 1, 1);
            WednesdayEnd = new DateTime(1900, 1, 1);
            ThursdayStart = new DateTime(1900, 1, 1);
            ThursdayEnd = new DateTime(1900, 1, 1);
            FridayStart = new DateTime(1900, 1, 1);
            FridayEnd = new DateTime(1900, 1, 1);
            SaturdayStart = new DateTime(1900, 1, 1);
            SaturdayEnd = new DateTime(1900, 1, 1);
            SundayStart = new DateTime(1900, 1, 1);
            SundayEnd = new DateTime(1900, 1, 1);
            TimeWithinBounds = false;
            EndAfterMidnight = false;
            MondayWithinBounds = false;
            MondayEndAfterMidnight = false;
            TuesdayWithinBounds = false;
            TuesdayEndAfterMidnight = false;
            WednesdayWithinBounds = false;
            WednesdayEndAfterMidnight = false;
            ThursdayWithinBounds = false;
            ThursdayEndAfterMidnight = false;
            FridayWithinBounds = false;
            FridayEndAfterMidnight = false;
            SaturdayWithinBounds = false;
            SaturdayEndAfterMidnight = false;
            SundayWithinBounds = false;
            SundayEndAfterMidnight = false;
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

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public DateTime StartDate { get; set; }
        [DataMember]
        public DateTime EndDate { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public DateTime EndTime { get; set; }
        [DataMember]
        public bool TimeWithinBounds { get; set; }
        [DataMember]
        public bool EndAfterMidnight { get; set; }
        [DataMember]
        public DateTime MondayStart { get; set; }
        [DataMember]
        public DateTime MondayEnd { get; set; }
        [DataMember]
        public bool MondayWithinBounds { get; set; }
        [DataMember]
        public DateTime TuesdayStart { get; set; }
        [DataMember]
        public DateTime TuesdayEnd { get; set; }
        [DataMember]
        public bool TuesdayWithinBounds { get; set; }
        [DataMember]
        public DateTime WednesdayStart { get; set; }
        [DataMember]
        public DateTime WednesdayEnd { get; set; }
        [DataMember]
        public bool WednesdayWithinBounds { get; set; }
        [DataMember]
        public DateTime ThursdayStart { get; set; }
        [DataMember]
        public DateTime ThursdayEnd { get; set; }
        [DataMember]
        public bool ThursdayWithinBounds { get; set; }
        [DataMember]
        public DateTime FridayStart { get; set; }
        [DataMember]
        public DateTime FridayEnd { get; set; }
        [DataMember]
        public bool FridayWithinBounds { get; set; }
        [DataMember]
        public DateTime SaturdayStart { get; set; }
        [DataMember]
        public DateTime SaturdayEnd { get; set; }
        [DataMember]
        public bool SaturdayWithinBounds { get; set; }
        [DataMember]
        public DateTime SundayStart { get; set; }
        [DataMember]
        public DateTime SundayEnd { get; set; }
        [DataMember]
        public bool SundayWithinBounds { get; set; }
        [DataMember]
        public bool MondayEndAfterMidnight { get; set; }
        [DataMember]
        public bool TuesdayEndAfterMidnight { get; set; }
        [DataMember]
        public bool WednesdayEndAfterMidnight { get; set; }
        [DataMember]
        public bool ThursdayEndAfterMidnight { get; set; }
        [DataMember]
        public bool FridayEndAfterMidnight { get; set; }
        [DataMember]
        public bool SaturdayEndAfterMidnight { get; set; }
        [DataMember]
        public bool SundayEndAfterMidnight { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum ReplDiscountType
    {
        [EnumMember]
        Multibuy = 0,
        [EnumMember]
        MixAndMatch = 1,
        [EnumMember]
        DiscOffer = 2,
        [EnumMember]
        TotalDiscount = 3,
        [EnumMember]
        TenderType = 4,
        [EnumMember]
        ItemPoint = 5,
        [EnumMember]
        LineDiscount = 6,
        [EnumMember]
        Unknown = 99
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum DiscountValueType
    {
        [EnumMember]
        DealPrice = 0,
        [EnumMember]
        Percent = 1,
        [EnumMember]
        Amount = 2
    }
}
