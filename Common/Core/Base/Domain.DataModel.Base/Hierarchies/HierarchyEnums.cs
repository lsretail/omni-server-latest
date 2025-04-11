using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Hierarchies
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum HierarchyValueType
    {
        [EnumMember]
        Node,
        [EnumMember]
        Leaf
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum HierarchyType
    {
        [EnumMember]
        ItemDeal,
        [EnumMember]
        StoreWarehouseCustomer,
        [EnumMember]
        Vendor
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum HierarchyLeafType
    {
        [EnumMember]
        Item,
        [EnumMember]
        Deal,
        [EnumMember]
        Store,
        [EnumMember]
        StoreGroup,
        [EnumMember]
        WarehouseLocation,
        [EnumMember]
        Customer,
        [EnumMember]
        CustomerGroup,
        [EnumMember]
        Vendor,
        [EnumMember]
        ItemCategory
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum HierarchyDealType
    {
        [EnumMember]
        Item,
        [EnumMember]
        ProductGroup,
        [EnumMember]
        ItemCategory,
        [EnumMember]
        All,
        [EnumMember]
        PluMenu,
        [EnumMember]
        Modifier,
        [EnumMember]
        SpecialGroup,
        [EnumMember]
        Deal
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2021")]
    public enum VSTimeScheduleType
    {
        [EnumMember]
        NotSpecified,
        [EnumMember]
        DiningHours,
        [EnumMember]
        ValidationSchedule
    }
}
