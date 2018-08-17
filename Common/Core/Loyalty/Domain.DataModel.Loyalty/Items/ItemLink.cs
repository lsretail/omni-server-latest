using System;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Items
{
	public enum ItemLinkType
	{
		Item,
		ProductGroup,
		ItemCategory,
		Unknown
	}

	public class ItemLink : Entity
	{
		private ItemLinkType type;
		private string description;
	    private string parentId;
		private ImageView image;

		public ItemLinkType Type
		{
			get { return type; }
			set { type = value; }
		}

		public string Description
		{
			get { return description; }
			set { description = value; }
		}

	    public string ParentId
	    {
            get { return parentId; }
            set { parentId = value; }
	    }

		public ImageView Image
		{
			get { return image; }
			set { image = value; }
		}

		public ItemLink() : this(null)
		{
		}

		public ItemLink(string id) : base(id)
		{
			type = ItemLinkType.Unknown;
			description = string.Empty;
			image = null;
		}
	}
}

