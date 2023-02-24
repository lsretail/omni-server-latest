using System;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Scheme : Entity, IDisposable
    {
        public Scheme(string id) : base(id)
        {
            Description = string.Empty;
            Perks = string.Empty;
            PointsNeeded = 0;
            NextScheme = null;
            Club = new Club();
        }

        public Scheme() : this(string.Empty)
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
                if (Club != null)
                    Club.Dispose();
            }
        }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Perks { get; set; } 
        [DataMember]
        public Int64 PointsNeeded { get; set; } //points to qualify to next membership level
        [DataMember]
        public Scheme NextScheme { get; set; } //next Membership level
        [DataMember]
        public Club Club { get; set; }

        public int UpdateSequence { get; set; }
        public string NextSchemeCode { get; set; }
        public string ClubCode { get; set; }
    }
}
 