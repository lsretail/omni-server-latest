using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class MenuNode : Entity, IDisposable
    {
        public MenuNode(string id) : base(id)
        {
            Image = new ImageView();
            MenuNodeLines = new List<MenuNodeLine>();
            MenuGroupNodes = new List<MenuNode>();
            ValidationStartTime = DateTime.Now;
            ValidationEndTime = DateTime.Now;
        }

        public MenuNode() : this(string.Empty)
        {
        }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public ImageView Image { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        [DataMember]
        public List<MenuNodeLine> MenuNodeLines { get; set; }
        [DataMember]
        public List<MenuNode> MenuGroupNodes { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ValidationStartTime { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ValidationEndTime { get; set; }
        /// <summary>
        /// Validation TimeWithinBounds 
        /// (true: valid between 10:00-18:00)
        /// (false: valid outside 10:00-18:00, ie. anytime other than this between this time)
        /// </summary>
        [DataMember]
        public bool ValidationTimeWithinBounds { get; set; }
        [DataMember]
        public bool ValidationEndTimeAfterMidnight { get; set; }
        [DataMember]
        public string PriceGroup { get; set; }
        [DataMember]
        public bool NodeIsItem { get; set; }

        public List<MenuNode> FlattenChildNodes()
        {
            if (NodeIsItem)
            {
                return new List<MenuNode>() { this };
            }
            else
            {
                List<MenuNode> flattenedChildNodes = new List<MenuNode>();
                flattenedChildNodes.Add(this);

                foreach (MenuNode node in this.MenuGroupNodes)
                {
                    flattenedChildNodes.AddRange(node.FlattenChildNodes());
                }
                return flattenedChildNodes;
            }
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
                if (Image != null)
                    Image.Dispose();
                if (MenuNodeLines != null)
                    MenuNodeLines.Clear();
                if (MenuGroupNodes != null)
                    MenuGroupNodes.Clear();
            }
        }

        /// <summary>
        /// Is this menunode valid at this time, i.e. does the current time fall within the menunode's validation period
        /// </summary>
        /// <returns><c>true</c> if the menunode is valid; otherwise, <c>false</c>.</returns>
        /// <param name="currentTime">Current time</param>
        public bool IsValid(DateTime currentTime)
        {
            if (this.ValidationTimeWithinBounds)
            {
                if (this.ValidationStartTime <= currentTime && currentTime <= this.ValidationEndTime)
                    return true;
                else
                    return false;
            }
            else
            {
                if (this.ValidationStartTime <= currentTime && currentTime <= this.ValidationEndTime)
                    return false;
                else
                    return true;
            }
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class MenuNodeLine : IDisposable
    {
        public MenuNodeLine(string id)
        {
            Id = id;
            NodeLineType = NodeLineType.Unknown;
        }

        public MenuNodeLine()
            : this(string.Empty)
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
        public string Id { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        /// <summary>
        /// Menu node line type (product, item, deal) 
        /// </summary>
        [DataMember]
        public NodeLineType NodeLineType { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum NodeLineType
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Recipe = 1,
        [EnumMember]
        Product = 2,
        [EnumMember]
        Deal = 3,
        [EnumMember]
        ProductOrRecipe = 4,
    }
}
