using System;

using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Pos.Utils
{
    public enum PedBannerVisibility
    {
        Never,
        WhenDisconnected,
        Always
    }

    public class MPOSFeatures
    {
        public static bool MemberManagement = true;

        // We dont want those here as const since else warnings are generated when
        // this feature is turned off in code
        public static bool UseSignatureScreenInsteadOfPrintingReceipts = true;
        public static bool EmailReceipt = false;
        public static bool ReceiptPrintingOptional = true;
        public static bool ArabicLocalization = false;
        public static bool AutoLogoffActive = false;    // NOTE: This seems to be causing problems on the iOS side of things. Leave disabled for now.

        public const bool AbleToPayWithPoints = true;
        public const bool AbleToPayWithForeignCurrency = true;
        public static bool InstantItemSearch = true;

        /// <summary>
        /// Should we print the payment receipts with the retail transaction receipt, or just the retail transaction receipt? (Printed after posting to BO)
        /// </summary>
        public static bool PrintPaymentReceiptsWithRetailTransactionReceipt = false;

        public const ContactSearchType DefaultLoyaltyContactSearchType = ContactSearchType.Name;
        public const CustomerSearchType DefaultCustomerSearchType = CustomerSearchType.Name;
        public const long MaximumPayment = 100000000;
        public const int MaximumQty = 10000;
        public const int DefaultNumberOfCustomers = 500;
        public const int DefaultNumberOfTransactions = 500;

        public const int ShowHintCount = 1;

        public const string CurrecyCodeForLoyaltyPoints = "LOY";
        public static bool HasStockRoomFeatures = false;

        public static bool PersistentLogin = false; // If set to true then device remains logged in until log out is pressed no matter if App was shut down.

        public static PedBannerVisibility PedBannerVisibility = PedBannerVisibility.WhenDisconnected;

        public static bool AllowOffline = true;

        // The following ones should not be treated as constants, they will be fetched from the Omni server, and their value will 
        // be determined by the server.
        // -----------------------------------------------------------------------------

        public static bool HasRecomendsList = false;
        public static bool InventoryNavigation = true; // Determines if button to launch the Inventory button should be visible or not
        public static bool InventoryLookup = true; // Determines if invetory lookup should be enabled or not

        // -----------------------------------------------------------------------------
    }
}