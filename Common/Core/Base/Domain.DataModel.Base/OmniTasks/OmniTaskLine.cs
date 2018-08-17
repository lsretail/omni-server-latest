using System;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.OmniTasks
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class OmniTaskLine : Entity, IDisposable
    {
		public OmniTaskLine() : this(string.Empty)
		{
		}

		public OmniTaskLine(string id) : base(id)
		{
			ItemId = string.Empty;
			ItemDescription = string.Empty;
			VariantId = string.Empty;
			VariantDescription = string.Empty;
			ModifyLocation = string.Empty;
			ModifyUser = string.Empty;
			TaskId = string.Empty;
            ModifyTime = DateTime.Now;
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

        public override string ToString()
        {
            return string.Format("ID:{0} Item:{1} Variant:{2} Status:{3} ModUser:{4} ModLoc:{5}",
                Id, ItemId, VariantId, Status.ToString(), ModifyUser, ModifyLocation);
        }

        public bool IsEquals(object obj)
        {
            OmniTaskLine task = (OmniTaskLine)obj;

            if (Id.Equals(task.Id) == false)
                return false;
            if (ModifyLocation.Equals(task.ModifyLocation) == false)
                return false;
            if (ModifyUser.Equals(task.ModifyUser) == false)
                return false;
            if (ItemId.Equals(task.ItemId) == false)
                return false;
            if (ItemDescription.Equals(task.ItemDescription) == false)
                return false;
            if (VariantId.Equals(task.VariantId) == false)
                return false;
            if (VariantDescription.Equals(task.VariantDescription) == false)
                return false;
            if (Status.Equals(task.Status) == false)
                return false;

            return true;
        }

        public void SetStatus(string user, string location, OmniTaskLineStatus status)
        {
            ModifyUser = user;
            ModifyLocation = location;
            Status = status;
        }

        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string ItemDescription { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string VariantDescription { get; set; }
        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public string ModifyLocation { get; set; }
        [DataMember]
        public DateTime ModifyTime { get; set; }
        [DataMember]
        public string ModifyUser { get; set; }
        [DataMember]
        public OmniTaskLineStatus Status { get; set; }
        [DataMember]
        public string TaskId { get; set; }
        [DataMember]
        public int Quantity { get; set; }
    }
}
