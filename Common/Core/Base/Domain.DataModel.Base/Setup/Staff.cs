using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017"), KnownType(typeof(UnknownStaff))]
    [System.Xml.Serialization.XmlInclude(typeof(UnknownStaff))]
	public class Staff : Entity
    {
        public Staff()
        {
        }

        public Staff(string staffId) : base(staffId)
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Password = string.Empty;
            InventoryActive = true;
            InventoryMainMenuId = string.Empty;
            Store = null;
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public bool InventoryActive { get; set; }
        [DataMember]
        public string InventoryMainMenuId { get; set; }
        [DataMember]
        public Store Store { get; set; }
        [DataMember]
        public string NameOnPOS { get; set; }
        [DataMember]
        public string NameOnReceipt { get; set; }
        [DataMember]
        public bool ChangePassword { get; set; }
        [DataMember]
        public bool Blocked { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? BlockingDate { get; set; }
        [DataMember]
        public List<StaffPermission> Permissions { get; set; }

        private StaffPermission GetPermission(PermissionEntry entry)
        {
            return Permissions?.FirstOrDefault(x => x.Entry == entry);
        }

        public bool IsManager()
        {
            var managerPermission = GetPermission(PermissionEntry.ManagerPrivileges);

            if (managerPermission == null)
            {
                return true;
            }

            return (bool)managerPermission.Value;
        }

        public bool CanVoidTransaction()
        {
            var voidPermission = GetPermission(PermissionEntry.VoidTransaction);

            if (voidPermission == null)
            {
                return true;
            }

            return (bool)voidPermission.Value;
        }

        public bool CanVoidLine()
        {
            var voidPermission = GetPermission(PermissionEntry.VoidLine);

            if (voidPermission == null)
            {
                return true;
            }

            return (bool)voidPermission.Value;
        }

        public bool CanReturnInTransaction()
        {
            var returnInTransaction = GetPermission(PermissionEntry.ReturnInTransaction);

            if (returnInTransaction == null)
            {
                return true;
            }

            return (bool)returnInTransaction.Value;
        }

        public bool CanSuspendTransaction()
        {
            var suspendTransaction = GetPermission(PermissionEntry.SuspendTransaction);

            if (suspendTransaction == null)
            {
                return true;
            }

            return (bool)suspendTransaction.Value;
        }

        public bool CanAddPayment()
        {
            var addPayment = GetPermission(PermissionEntry.AddPayment);

            if (addPayment == null)
            {
                return true;
            }

            return (bool)addPayment.Value;
        }

        public PriceOverridePermission CanPriceOverride()
        {
            var priceOverride = GetPermission(PermissionEntry.PriceOverRide);

            if (priceOverride == null)
            {
                return PriceOverridePermission.HigherAndLower;
            }

            return (PriceOverridePermission)priceOverride.Value;
        }

        public decimal MaxDiscountToGivePercent()
        {
            var discountToGive = GetPermission(PermissionEntry.MaxDiscountToGivePercent);

            if (discountToGive == null)
            {
                return 100m;
            }

            return (decimal)discountToGive.Value;
        }

        public decimal MaxTotalDiscountPercent()
        {
            var maxTotalDiscounts = GetPermission(PermissionEntry.MaxTotalDiscountPercent);

            if (maxTotalDiscounts == null)
            {
                return 100m;
            }

            return (decimal)maxTotalDiscounts.Value;
        }

        public decimal MaxDiscountToGiveAmount()
        {
            var discountToGive = GetPermission(PermissionEntry.MaxDiscountToGiveAmount);

            if (discountToGive == null)
            {
                throw new LSOmniException(StatusCode.NoDiscountAmount);
            }

            return (decimal)discountToGive.Value;
        }

        public decimal MaxTotalDiscountAmount()
        {
            var maxTotalDiscounts = GetPermission(PermissionEntry.MaxTotalDiscountAmount);

            if (maxTotalDiscounts == null)
            {
                throw new LSOmniException(StatusCode.NoDiscountAmount);
            }

            return (decimal)maxTotalDiscounts.Value;
        }

        public bool CanPrintXYZReports()
        {
            var xyzReports = GetPermission(PermissionEntry.XZYReport);

            if (xyzReports == null)
            {
                return true;
            }

            return (bool)xyzReports.Value;
        }

        public bool CanTenderDecleration()
        {
            var tenderDecleration = GetPermission(PermissionEntry.TenderDeclaration);

            if (tenderDecleration == null)
            {
                return true;
            }

            return (bool)tenderDecleration.Value;
        }

        public bool CanFloatingDecleration()
        {
            var floatingDecleration = GetPermission(PermissionEntry.FloatingDeclaration);

            if (floatingDecleration == null)
            {
                return true;
            }

            return (bool)floatingDecleration.Value;
        }
    }
}