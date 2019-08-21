using System.Runtime.Serialization;
using System.Xml.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
	[XmlType(Namespace = "Pos")]
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
	public abstract class BaseLine : Entity
	{
		[DataMember]
		public int LineNumber { get; set; }
		[DataMember]
		public bool Voided { get; set; }

		/// <summary>
		/// Initializes a new <see cref="BaseLine"/>.
		/// </summary>
		/// <remarks>A parameterless constructor is required for xml serialization.</remarks>
		public BaseLine()
		{ }

		public BaseLine(int lineNumber)
		{
			this.LineNumber = lineNumber;
			Voided = false;
		}
	}
}
