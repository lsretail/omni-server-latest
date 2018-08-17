using System;
using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    public enum MobileMenuNodeType
    {
        Group,
        Item
    }

    public class MobileMenuNode : Entity, IDisposable
    {
        public MobileMenuNode(string id) : base(id)
        {
            Image = new ImageView();
            MenuNodeLines = new List<MenuNodeLine>();
            MenuGroupNodes = new List<MenuNode>();
            ValidationStartTime = DateTime.Now;
            ValidationEndTime = DateTime.Now;
        }

        public MobileMenuNode() : this(string.Empty)
        {
        }

        public string Description { get; set; }
        public ImageView Image { get; set; }
        public int DisplayOrder { get; set; }
        public List<MenuNodeLine> MenuNodeLines { get; set; }
        public List<MenuNode> MenuGroupNodes { get; set; }
        public DateTime ValidationStartTime { get; set; }
        public DateTime ValidationEndTime { get; set; }
        public bool ValidationTimeWithinBounds { get; set; }
        public bool ValidationEndTimeAfterMidnight { get; set; }
        public string PriceGroup { get; set; }
        public MobileMenuNodeType NodeType { get; set; }
        public NodeLineType NodeLineType { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Image != null)
                    Image.Dispose();
                if (MenuNodeLines != null)
                    MenuNodeLines.Clear();
                if (MenuGroupNodes != null)
                    MenuGroupNodes.Clear();
            }
        }
    }
}
