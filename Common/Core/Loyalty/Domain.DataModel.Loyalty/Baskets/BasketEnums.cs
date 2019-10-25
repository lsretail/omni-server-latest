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
}
