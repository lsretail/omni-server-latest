using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase;

using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
	/// <summary>
	/// NOTE. UOM is always a member of Item
	/// Item.Id + uom.Id  identifies the uom
	/// </summary>
	[XmlInclude(typeof(UnknownUnitOfMeasure))]
	[DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017"), KnownType(typeof(UnknownUnitOfMeasure))]
	public class UnitOfMeasure : Entity, IDisposable
	{
		public UnitOfMeasure(string id, string itemId) : base(id)
		{
			ShortDescription = ""; // mobile code
			ItemId = itemId;
			Description = "";
			QtyPerUom = 0.0M;
			Decimals = 0;
		}

		public UnitOfMeasure(string id, decimal qty) : base(id)
		{
			QtyPerUom = qty;
		}

		public UnitOfMeasure() : this(string.Empty, string.Empty)
		{
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
		}

		[DataMember]
		public string ItemId { get; set; }
		[DataMember]
		public string Description { get; set; }
		[DataMember]
		public decimal QtyPerUom { get; set; }  //Qty.per Unit of Measure
		[DataMember]
		public int Decimals { get; set; }  //number of decimals in UOM
		[DataMember]
		public string ShortDescription { get; set; }
		[DataMember]
		public decimal Price { get; set; }

		public virtual UnitOfMeasure Clone()
		{
			return (UnitOfMeasure)MemberwiseClone();
		}

		public override string ToString()
		{
			return string.Format(@"Id: {0} ItemId: {1}  Description: {2} QtyPerUom: {3} Decimals: {4} ShortDescription: {5}",
				 Id, ItemId, Description, QtyPerUom, Decimals, ShortDescription);
		}
	}
}
