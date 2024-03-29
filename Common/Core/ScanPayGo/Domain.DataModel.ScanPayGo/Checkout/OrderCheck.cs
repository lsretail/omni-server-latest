﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Checkout
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Spg/2021")]
    public class OrderCheck : IDisposable
    {
        public OrderCheck()
        {
            Lines = new List<OrderCheckLines>();
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
                if (Lines != null)
                    Lines.Clear();
            }
        }

        [DataMember]
        public bool OrderPayed { get; set; }
        [DataMember]
        public bool DoCheck { get; set; }
        [DataMember]
        public int NumberOfItemsToCheck { get; set; }
        [DataMember]
        public DateTime StatusDate { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public List<OrderCheckLines> Lines { get; set; }
        [DataMember]
        public List<OrderCheckPayment> Payments { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Spg/2021")]
    public class OrderCheckLines : IDisposable
    {
        public OrderCheckLines()
        {
            DocumentID = string.Empty;
            ItemId = string.Empty;
            ItemDescription = string.Empty;
            VariantCode = string.Empty;
            VariantDescription = string.Empty;
            UnitofMeasureCode = string.Empty;   
            UOMDescription = string.Empty;
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
        public string DocumentID;
        [DataMember]
        public int LineNo { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string VariantCode { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }
        [DataMember]
        public string UnitofMeasureCode { get; set; }
        [DataMember]
        public string UOMDescription { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public bool AlwaysCheck { get; set; }
        [IgnoreDataMember]
        public bool QuantityCounted { get; set; }

        public static bool operator ==(OrderCheckLines checkLine1, OrderCheckLines checkLine2)
        {
            if (ReferenceEquals(checkLine1, checkLine2))
            {
                return true;
            }

            if (checkLine1?.ItemId == checkLine2?.ItemId && checkLine1?.VariantCode == checkLine2?.VariantCode && checkLine1?.UnitofMeasureCode == checkLine2?.UnitofMeasureCode && checkLine1?.QuantityCounted == checkLine2?.QuantityCounted)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(OrderCheckLines checkLine1, OrderCheckLines checkLine2)
        {
            return !(checkLine1 == checkLine2);
        }

        public OrderCheckLines Clone()
        {
            return (OrderCheckLines) MemberwiseClone();
        }
        protected bool Equals(OrderCheckLines other)
        {
            return DocumentID == other.DocumentID && LineNo == other.LineNo && ItemId == other.ItemId && ItemDescription == other.ItemDescription && VariantCode == other.VariantCode && VariantDescription == other.VariantDescription && UnitofMeasureCode == other.UnitofMeasureCode && UOMDescription == other.UOMDescription && Quantity == other.Quantity && Amount == other.Amount && AlwaysCheck == other.AlwaysCheck && QuantityCounted == other.QuantityCounted;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OrderCheckLines)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (DocumentID != null ? DocumentID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LineNo;
                hashCode = (hashCode * 397) ^ (ItemId != null ? ItemId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ItemDescription != null ? ItemDescription.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VariantCode != null ? VariantCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VariantDescription != null ? VariantDescription.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UnitofMeasureCode != null ? UnitofMeasureCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UOMDescription != null ? UOMDescription.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Quantity.GetHashCode();
                hashCode = (hashCode * 397) ^ Amount.GetHashCode();
                hashCode = (hashCode * 397) ^ AlwaysCheck.GetHashCode();
                hashCode = (hashCode * 397) ^ QuantityCounted.GetHashCode();
                return hashCode;
            }
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Spg/2021")]
    public class OrderCheckPayment : IDisposable
    {
        public OrderCheckPayment()
        {
            CardType = string.Empty;
            ExternalRef = string.Empty;
            AutorizationCode = string.Empty;
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
        public string CardType;
        [DataMember]
        public string ExternalRef { get; set; }
        [DataMember]
        public decimal PaymentAmount { get; set; }
        [DataMember]
        public string AutorizationCode { get; set; }
    }
}