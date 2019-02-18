using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum TransactionType
    {
        //Nav POS Transaction  "Logoff,Logon,Sales,Payment,Remove Tender,Float Entry,Change Tender,Tender Decl.,Voided,Open Drawer,NegAdj,PhysInv,Collect,Cancelation"
        [EnumMember]
        Logoff = 0,
        [EnumMember]
        Logon = 1,
        [EnumMember]
        Sales = 2,
        [EnumMember]
        Payment = 3,
        [EnumMember]
        RemoveTender = 4,
        [EnumMember]
        FloatEntry = 5,
        [EnumMember]
        ChangeTender = 6,
        [EnumMember]
        TenderDecl = 7,
        [EnumMember]
        Voided = 8,
        [EnumMember]
        OpenDrawer = 9,
        [EnumMember]
        NegAdj = 10,
        [EnumMember]
        PhysInv = 11,
        [EnumMember]
        Collect = 12,
        [EnumMember]
        Cancelation = 13,
        [EnumMember]
        Unknown = 99,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum LineType
    {
        [EnumMember]
        Item = 0,
        [EnumMember]
        Payment = 1,
        [EnumMember]
        PerDiscount = 2,
        [EnumMember]
        TotalDiscount = 3,
        [EnumMember]
        IncomeExpense = 4,
        [EnumMember]
        FreeText = 5,
        [EnumMember]
        Coupon = 6,
        [EnumMember]
        Shipping = 7,
        [EnumMember]
        Unknown = 99,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum PrintType
    {
        [EnumMember]
        StartTransaction = 0,
        [EnumMember]
        EndTransaction = 1,
        [EnumMember]
        PrintLine = 2,
        [EnumMember]
        PrintBarcode = 3,
        [EnumMember]
        PrintBitmap = 4,
        [EnumMember]
        InsertPage = 5,
        [EnumMember]
        RemovePage = 6,
        [EnumMember]
        Rotation = 7,
        [EnumMember]
        InitPrinter = 8,
        [EnumMember]
        PrintBitmapFile = 9,
        [EnumMember]
        Unknown = 99,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum EntryStatus
    {
        //Nav POS Transaction  has entry status  ",Suspended,Laid away,Voided,Training,InUse"
        [EnumMember]
        Normal = 0,
        [EnumMember]
        Voided = 1,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum DocumentType
    {
        [EnumMember]
        Quote = 0,
        [EnumMember]
        Order = 1,
        [EnumMember]
        Invoice = 2,
        [EnumMember]
        CreditMemo = 3,
        [EnumMember]
        BlankOrder = 4,
        [EnumMember]
        ReturnOrder = 5,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum SourceType
    {
        //NAV POS Transaction "Stationary,Mobile POS,eCommerce,Mobile Loyalty,Mobile Hospitality"
        [EnumMember]
        Standard = 0,
        [EnumMember]
        LSOmni = 1,
        [EnumMember]
        eCommerce = 2,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum BasketCalcType
    {
        //Pre, Final, Collect  - only in North America, eCommerce.  None used by non ecommerce
        //None: used by Loyalty
        //Pre: used in calc, not in checkout after addresss has been added
        //collect: click&collect 
        //final: final, including tax
        [EnumMember]
        None = 0,
        [EnumMember]
        Pre = 1,
        [EnumMember]
        Final = 2,
        [EnumMember]
        Collect = 3,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum DeliveryType
    {
        [EnumMember]
        InStorePickup = 0,
        [EnumMember]
        HomeDelivery = 1,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum PaymentType
    {
        [EnumMember]
        PayOnDelivery = 0,
        [EnumMember]
        CreditCard = 1,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum OrderStatus
    {
        [EnumMember]
        Created = 0,
        [EnumMember]
        Pending = 1,
        [EnumMember]
        Processing = 2,
        [EnumMember]
        Complete = 3,
        [EnumMember]
        Canceled = 4,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum ShippingStatus
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        ShippigNotRequired = 10,
        [EnumMember]
        NotYetShipped = 20,
        [EnumMember]
        PartiallyShipped = 25,
        [EnumMember]
        Shipped = 30,
        [EnumMember]
        Delivered = 40,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum PaymentStatus
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        PreApproved = 10,
        [EnumMember]
        Approved = 20,
        [EnumMember]
        Posted = 25
    }
}
