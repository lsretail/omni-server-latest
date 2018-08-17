using LSRetail.Omni.Domain.DataModel.Base.Base;
using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Card : Entity, IDisposable
    {
        public Card(string id) : base(id)
        {
            ContactId = string.Empty;
            ClubId = string.Empty;
            LinkedToAccount = false;
 
            Status = CardStatus.Free;
            DateBlocked = null;
            BlockedBy = string.Empty;
            BlockedReason = string.Empty;
        }

        public Card() : this(string.Empty)
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
        public string ClubId { get; set; }
        [DataMember]
        public string ContactId { get; set; }
        [DataMember]
        public bool LinkedToAccount { get; set; }
        [DataMember]
        public CardStatus Status { get; set; }
        [DataMember]
        public DateTime? DateBlocked { get; set; }
        [DataMember]
        public string BlockedBy { get; set; }
        [DataMember]
        public string BlockedReason { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum CardStatus
    {
        [EnumMember]
        Free = 0,
        [EnumMember]
        Allocated = 1,      
        [EnumMember]
        Active = 2,   
        [EnumMember]
        Blocked = 3,   
    }
}
 