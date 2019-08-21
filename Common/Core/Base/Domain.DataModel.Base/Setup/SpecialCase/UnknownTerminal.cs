using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase
{
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
	public sealed class UnknownTerminal : Terminal
	{
		/// <summary>
		/// Initializes a new instance of <see cref="UnknownTerminal"/>.
		/// </summary>
		/// <remarks>A parameterless constructor is required for xml serialization.</remarks>
		public UnknownTerminal()
		{ }

		public UnknownTerminal(string id) : base(id)
		{
			Unknown = true;
		}

		[DataMember]
		public bool Unknown { get; set; }
	}
}
