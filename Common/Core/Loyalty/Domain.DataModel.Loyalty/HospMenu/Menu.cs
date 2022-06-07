using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Menu
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Menu : Entity, IDisposable
    {
        public Menu(string id) : base(id)
        {
            DefaultMenu = true;
            Image = new ImageView();
            MenuNodes = new List<MenuNode>();
            ValidationStartTime = DateTime.Now;
            ValidationEndTime = DateTime.Now;
        }

        public Menu() : this(string.Empty)
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
                if (Image != null)
                    Image.Dispose();
                if (MenuNodes != null)
                    MenuNodes.Clear();
            }
        }

        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ValidDescription { get; set; }
        [DataMember]
        public ImageView Image { get; set; }
        [DataMember]
        public bool DefaultMenu { get; set; }
        [DataMember]
        public List<MenuNode> MenuNodes { get; set; }
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
        public int OrderSequenceNumber { get; set; }

        /// <summary>
        /// Is this menu valid at this time, i.e. does the current time fall within the menu's validation period
        /// </summary>
        /// <returns><c>true</c> if the menu is valid; otherwise, <c>false</c>.</returns>
        /// <param name="currentTime">Current time</param>
        public bool IsValid(DateTime currentTime)
        {
            if (ValidationTimeWithinBounds)
            {
                if (ValidationStartTime <= currentTime && currentTime <= ValidationEndTime)
                    return true;
                else
                    return false;
            }
            else if (ValidationEndTimeAfterMidnight)
            {
                if (ValidationStartTime <= currentTime && currentTime <= ValidationEndTime.AddDays(1))
                    return true;
                else
                    return false;
            }
            else
            {
                if (ValidationStartTime <= currentTime && currentTime <= ValidationEndTime)
                    return false;
                else
                    return true;
            }
        }
    }
}
