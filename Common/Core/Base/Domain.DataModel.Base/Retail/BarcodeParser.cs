using System;
using System.Collections.Generic;
using System.Linq;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    public class BarcodeParseResult : IEquatable<BarcodeParseResult>
    {
        #region Properties

        public string Barcode { get; set; }
        public BarcodeMask Mask { get; set; }
        public List<BarcodeSegmentValue> BarcodeSegmentsValues { get; } = new List<BarcodeSegmentValue>();

        public string Coupon => Lookup(Segment.SegmentType.Coupon)?.Value;
        public string LicenseNo => Lookup(Segment.SegmentType.LicenseNo)?.Value;
        public string EmployeeNr => Lookup(Segment.SegmentType.EmployeeNr)?.Value;
        public string CustomerNr => Lookup(Segment.SegmentType.CustomerNr)?.Value;
        public string DataEntryNr => Lookup(Segment.SegmentType.DataEntryNr)?.Value;
        public string NumberSeries => Lookup(Segment.SegmentType.NumberSeries)?.Value;
        public string LotNo => Lookup(Segment.SegmentType.LotNo)?.Value;
        public string SerialNo => Lookup(Segment.SegmentType.SerialNo)?.Value;
        public string System => Lookup(Segment.SegmentType.System)?.Value;

        public int Quantity
        {
            get
            {
                BarcodeSegmentValue segment = Lookup(Segment.SegmentType.Quantity);
                return segment != null ? int.Parse(segment.Value) : 0;
            }
        }

        public int QuantityDecimals
        {
            get
            {
                BarcodeSegmentValue segment = Lookup(Segment.SegmentType.Quantity);
                return (segment?.Segment == null) ? 0 : segment.Segment.Decimals;
            }
        }

        public decimal Price
        {
            get
            {
                BarcodeSegmentValue segment = Lookup(Segment.SegmentType.Price);
                return segment != null ? decimal.Parse(segment.Value) : 0m;
            }
        }

        public int PriceDecimals
        {
            get
            {
                BarcodeSegmentValue segment = Lookup(Segment.SegmentType.Price);
                return (segment?.Segment == null) ? 0 : segment.Segment.Decimals;
            }
        }

        public string ItemBarcode
        {
            get
            {
                if (Mask == null)
                {
                    return Barcode;
                }
                bool replaceSystem = IdentifyingSegmentType == Segment.SegmentType.Price || IdentifyingSegmentType == Segment.SegmentType.Quantity;
                string result = Mask.Prefix;
                foreach (BarcodeSegmentValue segment in BarcodeSegmentsValues)
                {
                    if (segment.Segment.Type == Segment.SegmentType.Price || segment.Segment.Type == Segment.SegmentType.Quantity || (replaceSystem && segment.Segment.Type == Segment.SegmentType.System))
                    {
                        for (int i = 0; i < segment.Segment.Length; i++)
                        {
                            result += "0";
                        }
                    }
                    else
                    {
                        result += segment.Value;
                    }
                }
                if (String.IsNullOrEmpty(result))
                {
                    return Barcode;
                }
                return result;
            }
        }

        public Segment.SegmentType IdentifyingSegmentType
        {
            get
            {
                if (BarcodeSegmentsValues == null)
                    return Segment.SegmentType.Unknown;

                foreach (BarcodeSegmentValue value in BarcodeSegmentsValues)
                {
                    if (Segment.IdentifyingTypes.Contains(value.Segment.Type))
                    {
                        return value.Segment.Type;
                    }
                }
                return Segment.SegmentType.Unknown;
            }
        }

        #endregion

        public BarcodeParseResult(string barcode, BarcodeMask mask)
        {
            this.Barcode = barcode;
            this.Mask = mask;
        }

        public class BarcodeSegmentValue : IEquatable<BarcodeSegmentValue>
        {
            public string Value { get; set; }
            public Segment Segment { get; set; }

            #region Compare/Equals/Hashcode

            public bool Equals(BarcodeSegmentValue other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return string.Equals(Value, other.Value) && Equals(Segment, other.Segment);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != this.GetType())
                    return false;
                return Equals((BarcodeSegmentValue)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ (Segment != null ? Segment.GetHashCode() : 0);
                }
            }

            public static bool operator ==(BarcodeSegmentValue left, BarcodeSegmentValue right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(BarcodeSegmentValue left, BarcodeSegmentValue right)
            {
                return !Equals(left, right);
            }

            #endregion
        }

        private BarcodeSegmentValue Lookup(Segment.SegmentType key)
        {
            return BarcodeSegmentsValues.FirstOrDefault(x => x.Segment.Type == key);
        }

        #region Compare/Equals/Hashcode

        public bool Equals(BarcodeParseResult other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return BarcodeSegmentsValues.Except(other.BarcodeSegmentsValues).Any() == false &&
                       other.BarcodeSegmentsValues.Except(BarcodeSegmentsValues).Any() == false &&
                       Quantity == other.Quantity && Price == other.Price &&
                       string.Equals(Coupon, other.Coupon) && string.Equals(LicenseNo, other.LicenseNo) &&
                       string.Equals(EmployeeNr, other.EmployeeNr) && string.Equals(CustomerNr, other.CustomerNr) &&
                       string.Equals(DataEntryNr, other.DataEntryNr) && string.Equals(NumberSeries, other.NumberSeries) &&
                       string.Equals(LotNo, other.LotNo) && string.Equals(SerialNo, other.SerialNo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((BarcodeParseResult)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (BarcodeSegmentsValues != null ? BarcodeSegmentsValues.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Quantity;
                hashCode = (hashCode * 397) ^ Price.GetHashCode();
                hashCode = (hashCode * 397) ^ (Coupon != null ? Coupon.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LicenseNo != null ? LicenseNo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EmployeeNr != null ? EmployeeNr.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CustomerNr != null ? CustomerNr.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DataEntryNr != null ? DataEntryNr.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NumberSeries != null ? NumberSeries.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LotNo != null ? LotNo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SerialNo != null ? SerialNo.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(BarcodeParseResult left, BarcodeParseResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BarcodeParseResult left, BarcodeParseResult right)
        {
            return !Equals(left, right);
        }

        #endregion
    }

    public class BarcodeParser
    {
        public static BarcodeParseResult Parse(IEnumerable<BarcodeMask> masks, string barcode)
        {
            foreach (BarcodeMask mask in masks)
            {
                BarcodeParseResult result = Match(mask, barcode);
                if (result != null)
                {
                    return result;
                }
            }
            return new BarcodeParseResult(barcode, null);
        }

        private static BarcodeParseResult Match(BarcodeMask mask, string barcode)
        {
            if (String.IsNullOrEmpty(mask.Prefix))
                return null;

            if (barcode.StartsWith(mask.Prefix) == false)
                return null;

            BarcodeParseResult result = new BarcodeParseResult(barcode, mask);
            string barcodeProccessed = barcode.Substring(mask.Prefix.Length);
            foreach (Segment segment in mask.Segments)
            {
                result.BarcodeSegmentsValues.Add(
                    new BarcodeParseResult.BarcodeSegmentValue
                    {
                        Segment = segment,
                        Value = barcodeProccessed.Substring(0, segment.Length)
                    }
                );
                barcodeProccessed = barcodeProccessed.Substring(segment.Length);
            }
            return result;
        }
    }
}
