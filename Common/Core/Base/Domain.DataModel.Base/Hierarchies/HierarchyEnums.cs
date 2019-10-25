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
        CustomerGroup
    }
}
