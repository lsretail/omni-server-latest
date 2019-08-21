using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
	public sealed class UnknownStaff : Staff
	{
		/// <summary>
		/// Initializes a new instance of <see cref="UnknownStaff"/>.
		/// </summary>
		/// <remarks>A parameterless constructor is required for xml serialization.</remarks>
		public UnknownStaff()
		{ }

		public UnknownStaff(string staffId) : base(staffId)
		{
		}
	}
}
