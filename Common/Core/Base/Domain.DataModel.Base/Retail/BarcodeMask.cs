using System;
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    public class BarcodeMask : Entity, IEquatable<BarcodeMask>
    {
        public string Prefix { get; set; }
        public string Mask { get; set; }
        public List<Segment> Segments { get; } = new List<Segment>();

        public Segment.SegmentType SegmentType
        {
            get
            {
                if (Segments == null)
                    return Segment.SegmentType.Unknown;

                foreach (Segment segment in Segments)
                {
                    if (Segment.IdentifyingTypes.Contains(segment.Type))
                    {
                        return segment.Type;
                    }
                }
                return Segment.SegmentType.Unknown;
            }
        }

        public BarcodeMask(string prefix, string mask, params Segment[] segements)
        {
            this.Prefix = prefix;
            this.Mask = mask;
            this.Segments.AddRange(segements);
        }

        #region Compare/Equals/Hashcode

        public bool Equals(BarcodeMask other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(Prefix, other.Prefix) && string.Equals(Mask, other.Mask) && Equals(Segments, other.Segments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((BarcodeMask)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Prefix != null ? Prefix.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Mask != null ? Mask.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Segments != null ? Segments.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(BarcodeMask left, BarcodeMask right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BarcodeMask left, BarcodeMask right)
        {
            return !Equals(left, right);
        }

        #endregion
    }

    public class Segment : IEquatable<Segment>
    {
        public enum SegmentType
        {
            Quantity = 'Q',
            Price = 'P',
            Coupon = 'R',
            LicenseNo = 'L',
            EmployeeNr = 'E',
            CustomerNr = 'D',
            DataEntryNr = 'A',
            NumberSeries = 'N',
            LotNo = 'T',
            SerialNo = 'V',
            ItemNo = 'I',
            System = 'M',
            Unknown
        }

        // What SegmentTypes identify the type of barcode...

        public SegmentType Type { get; set; }
        public int Length { get; set; }
        public int Decimals { get; set; }

        public static List<Segment.SegmentType> IdentifyingTypes { get; } = new List<Segment.SegmentType> {
                SegmentType.Quantity,
                SegmentType.Price,
                SegmentType.Coupon,
                SegmentType.LicenseNo,
                SegmentType.EmployeeNr,
                SegmentType.CustomerNr,
                SegmentType.DataEntryNr,
                SegmentType.NumberSeries,
                SegmentType.LotNo
            };

        public Segment(string c, int length) : this(string.IsNullOrEmpty(c) ? char.MinValue : c.ToCharArray()[0], length)
        {
        }
        public Segment(char c, int length)
        {
            this.Type = (SegmentType)c;
            if (Enum.GetName(typeof(SegmentType), this.Type) == null)
            {
                this.Type = Segment.SegmentType.Unknown;
            }
            this.Length = length;
        }

        #region Compare/Equals/Hashcode

        public bool Equals(Segment other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Type == other.Type && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Segment)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Type * 397) ^ Length;
            }
        }

        public static bool operator ==(Segment left, Segment right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Segment left, Segment right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}