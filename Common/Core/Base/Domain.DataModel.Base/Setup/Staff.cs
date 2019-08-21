using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017"), KnownType(typeof(UnknownStaff))]
    [System.Xml.Serialization.XmlInclude(typeof(UnknownStaff))]
	public class Staff : Entity
    {
        public Staff()
        {
        }

        public Staff(string staffId) : base(staffId)
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Password = string.Empty;
            InventoryActive = true;
            InventoryMainMenuId = string.Empty;
            Store = null;
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public bool InventoryActive { get; set; }
        [DataMember]
        public string InventoryMainMenuId { get; set; }
        [DataMember]
        public Store Store { get; set; }
        [DataMember]
        public string NameOnPOS { get; set; }
        [DataMember]
        public string NameOnReceipt { get; set; }
        [DataMember]
        public bool ChangePassword { get; set; }
        [DataMember]
        public bool Blocked { get; set; }
        [DataMember]
        public DateTime? BlockingDate { get; set; }
    }
}