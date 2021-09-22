using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
	public class StaffPermission
	{
		/// <summary>
		/// Initializes a new <see cref="StaffPermission"/> with <see cref="PermissionEntry">permission</see> unknown.
		/// </summary>
		/// <remarks>A parameterless constructor is required for xml serialization.</remarks>
		public StaffPermission()
			: this(PermissionEntry.Unknown)
		{ }

		public StaffPermission(PermissionEntry entry)
		{
			Entry = entry;
		}

		[DataMember]
		public PermissionEntry Entry { get; set; }
		[DataMember]
		public PermissionType Type { get; set; }
		[DataMember]
		public object Value { get; set; }
	}

	public enum PriceOverridePermission
	{
		NoOverrideAllowed = 0,
		LowerOnly = 1,
		HigherOnly = 2,
		HigherAndLower = 3
	}

	public enum PermissionType
	{
		Boolean,
		List,
		Decimal,
		String
	}

	public enum PermissionEntry
	{
		Unknown = 0,
		ManagerPrivileges = 1,

		VoidTransaction = 2,
		VoidLine = 3,
		ReturnInTransaction = 4,
		SuspendTransaction = 5,
		AddPayment = 6,

		PriceOverRide = 7,
		MaxDiscountToGivePercent = 8,
		MaxTotalDiscountPercent = 9,

		XZYReport = 10,
		TenderDeclaration = 11,
		FloatingDeclaration = 12,

		CreateCustomer = 13,
		UpdateCustomer = 14,
		CustomerComments = 15,
		ViewSalesHistory = 16,

        MaxDiscountToGiveAmount = 17,
        MaxTotalDiscountAmount = 18,
	}
}
