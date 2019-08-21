using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
	public sealed class UnknownStore : Store
	{
		/// <summary>
		/// Initializes a new instance of <see cref="UnknownStore"/>.
		/// </summary>
		/// <remarks>A parameterless constructor is required for xml serialization.</remarks>
		public UnknownStore()
		{ }

		public UnknownStore(string id) : base(id)
		{
		}
	}
}
