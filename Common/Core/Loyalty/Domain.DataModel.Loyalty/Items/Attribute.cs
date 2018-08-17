using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class RetailAttribute : IDisposable
    {
        public RetailAttribute()
        {
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
        public string LinkField1 { get; set; }
        [DataMember]
        public string LinkField2 { get; set; }
        [DataMember]
        public string LinkField3 { get; set; }
        [DataMember]
        public AttributeLinkType LinkType { get; set; }
        [DataMember]
        public int Sequence { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public AttributeValueType ValueType { get; set; }
        [DataMember]
        public string DefaultValue { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public decimal NumbericValue { get; set; }

        [DataMember]
        public List<AttributeOptionValue> OptionValues { get; set; }

        public override string ToString()
        {
            return string.Format(@"LinkF1:{0} LinkF2:{1} LinkF3:{2} LinkType:{3} Seq:{4} Code:{5} Desc:{6} Value:{7} #Value:{8}",
                LinkField1, LinkField2, LinkField3, LinkType, Sequence, Code, Description, Value, NumbericValue);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class AttributeOptionValue : IDisposable
    {
        public AttributeOptionValue()
        {
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
        public string Code { get; set; }
        [DataMember]
        public int Sequence { get; set; }
        [DataMember]
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format(@"Code:{0} Seq:{1} Value:{2}", Code, Sequence, Value);
        }
    }


    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum AttributeLinkType
    {
        [EnumMember]
        Item = 0,
        [EnumMember]
        Variant = 1,
        [EnumMember]
        DimBaseValue = 2,
        [EnumMember]
        Customer = 3,
        [EnumMember]
        Store = 4,
        [EnumMember]
        Deal = 5,
        [EnumMember]
        Unknown = 100
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum AttributeValueType
    {

        [EnumMember]
        Text = 0,
        [EnumMember]
        Numeric = 1,
        [EnumMember]
        Amount = 2,
        [EnumMember]
        Date = 3,
        [EnumMember]
        File = 4,
        [EnumMember]
        OptionValue = 5,
        [EnumMember]
        TableLink = 6,
        [EnumMember]
        WeightedOption = 7,
        [EnumMember]
        Unknown = 100
    }
}

