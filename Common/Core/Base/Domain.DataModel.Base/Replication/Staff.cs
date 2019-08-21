using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplStaffResponse : IDisposable
    {
        public ReplStaffResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Staff = new List<ReplStaff>();
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
                if (Staff != null)
                    Staff.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplStaff> Staff { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplStaff : IDisposable
    {
        public ReplStaff()
        {
            IsDeleted = false;
            Id = string.Empty;
            Name = string.Empty;
            NameOnReceipt = string.Empty;
            Password = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            BlockingDate = new DateTime(1900, 1, 1);
            ChangePassword = 0;
            Blocked = 0;
            StoreID = string.Empty;
            InventoryMainMenu = string.Empty;
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
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string NameOnReceipt { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime BlockingDate { get; set; }
        [DataMember]
        public int ChangePassword { get; set; }
        [DataMember]
        public int Blocked { get; set; }
        [DataMember]
        public string StoreID { get; set; }
        [DataMember]
        public string InventoryMainMenu { get; set; }
        [DataMember]
        public bool InventoryActive { get; set; }
    }
}
