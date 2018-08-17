using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.OmniTasks
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class OmniTask : Entity, IDisposable
    {
        [DataMember]
        public string TransactionId { get; set; }
        [DataMember]
        public string AssignLocation { get; set; }
        [DataMember]
        public string AssignUser { get; set; }
        [DataMember]
        public string AssignUserName { get; set; }
        [DataMember]
        public DateTime CreateTime { get; set; }
        [DataMember]
        public List<OmniTaskLine> Lines { get; set; }
        [DataMember]
        public string ModifyLocation { get; set; }
        [DataMember]
        public DateTime ModifyTime { get; set; }
        [DataMember]
        public string ModifyUser { get; set; }
        [DataMember]
        public string RequestLocation { get; set; }
        [DataMember]
        public string RequestUser { get; set; }
        [DataMember]
        public string RequestUserName { get; set; }
        [DataMember]
        public OmniTaskStatus Status { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public string Type { get; set; }

        // local properties
        public bool HasModifiedLines { get; set; }

        public OmniTask() : this(string.Empty)
        {

        }

        public OmniTask(string id) : base(id)
        {
            TransactionId = string.Empty;
            RequestUserName = string.Empty;
            AssignLocation = string.Empty;
            AssignUser = string.Empty;
            AssignUserName = string.Empty;
            Lines = new List<OmniTaskLine>();
            RequestUser = string.Empty;
            RequestLocation = string.Empty;
            ModifyUser = string.Empty;
            ModifyLocation = string.Empty;
            StoreId = string.Empty;
            Status = OmniTaskStatus.None;
            Type = string.Empty;
            ModifyTime = DateTime.Now;
            CreateTime = DateTime.Now;
            HasModifiedLines = false;
        }

        public OmniTask(string user, string location, string store = "", string type = "") : this(string.Empty)
        {
            RequestUser = user;
            RequestLocation = location;
            ModifyUser = user;
            ModifyLocation = location;
            StoreId = store;
            Status = OmniTaskStatus.None;
            Type = type;
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
            return string.Format("ID:{0} Status:{1} ModUser:{2} ModLoc:{3} AssignUser:{4} AssignLoc:{5}",
                Id, Status.ToString(), ModifyUser, ModifyLocation, AssignUser, AssignLocation);
        }

        public bool IsEquals(object obj)
        {
            OmniTask task = (OmniTask)obj;

            if (Id.Equals(task.Id) == false)
                return false;
            if (AssignLocation.Equals(task.AssignLocation) == false)
                return false;
            if (AssignUser.Equals(task.AssignUser) == false)
                return false;
            if (AssignUserName.Equals(task.AssignUserName) == false)
                return false;
            if (ModifyLocation.Equals(task.ModifyLocation) == false)
                return false;
            if (ModifyUser.Equals(task.ModifyUser) == false)
                return false;
            if (RequestLocation.Equals(task.RequestLocation) == false)
                return false;
            if (RequestUser.Equals(task.RequestUser) == false)
                return false;
            if (RequestUserName.Equals(task.RequestUserName) == false)
                return false;
            if (Status.Equals(task.Status) == false)
                return false;

            return true;
        }

        public void SetStatus(string user, string userName, string location, OmniTaskStatus status)
        {
            ModifyUser = user;
            ModifyLocation = location;
            Status = status;

            if (status == OmniTaskStatus.Assigned)
            {
                AssignUser = user;
                AssignLocation = location;
                AssignUserName = userName;
            }
        }

        public bool SetLineStatus(int lineNumber, string user, string location, OmniTaskLineStatus status)
        {
            OmniTaskLine line = this.Lines.Find(l => l.LineNumber == lineNumber);
            if (line == null)
            {
                return false;
            }

            if (line.Status == status)
            {
                return true;    // No status change
            }

            ModifyUser = user;
            ModifyLocation = location;

            line.SetStatus(user, location, status);
            HasModifiedLines = true;
            return true;
        }

        public OmniTask ShallowCopy()
        {
            return (OmniTask)MemberwiseClone();
        }

        public void AddLine(string retailItemID, string retailItemDescription, string variantId, string variantDescription, int quantity, OmniTaskLineStatus status = OmniTaskLineStatus.None)
        {
            Boolean found = false;
            foreach (OmniTaskLine line in Lines)
            {
                if (line.ItemId == retailItemID && line.VariantId == variantId)
                {
                    found = true;
                    line.Quantity++;
                    line.Status = status;
                    break;
                }
            }

            if (found == false)
            {
                OmniTaskLine line = new OmniTaskLine();
                line.TaskId = this.Id;
                line.ItemId = retailItemID;
                line.ItemDescription = retailItemDescription;
                line.VariantId = variantId;
                line.ModifyUser = this.RequestUser;
                line.ModifyLocation = this.RequestLocation;
                line.Status = status;
                line.LineNumber = this.Lines.Count + 1;
                line.Quantity = quantity;
                line.VariantDescription = variantDescription;
                Lines.Add(line);
            }
            HasModifiedLines = true;
        }

        public bool AllLinesVoid()
        {
            foreach (OmniTaskLine line in Lines)
            {
                if (line.Status != OmniTaskLineStatus.Cancelled && line.Status != OmniTaskLineStatus.NotFound && line.Status != OmniTaskLineStatus.Deleted)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
