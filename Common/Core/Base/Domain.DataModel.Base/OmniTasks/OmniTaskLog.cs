using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.OmniTasks
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class OmniTaskLog : IDisposable
    {
        public OmniTaskLog()
        {
            Id = 0;
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

        /// <summary>
        /// TaskLog Id
        /// </summary>
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string TaskId { get; set; }
        [DataMember]
        public DateTime ModifyTime { get; set; }
        /// <summary>
        /// Last User that modified the task
        /// </summary>
        [DataMember]
        public string ModifyUser { get; set; }
        /// <summary>
        /// Last Location that modified the task
        /// </summary>
        [DataMember]
        public string ModifyLocation { get; set; }
        /// <summary>
        /// Status of task before changees
        /// </summary>
        [DataMember]
        public OmniTaskStatus StatusFrom { get; set; }
        /// <summary>
        /// Status of task after changees
        /// </summary>
        [DataMember]
        public OmniTaskStatus StatusTo { get; set; }
        /// <summary>
        /// Old User that requested the task
        /// </summary>
        [DataMember]
        public string RequestUserFrom { get; set; }
        /// <summary>
        /// New User that requested the task
        /// </summary>
        [DataMember]
        public string RequestUserTo { get; set; }
        /// <summary>
        /// User assigned to task before changes
        /// </summary>
        [DataMember]
        public string AssignUserFrom { get; set; }
        /// <summary>
        /// User assigned to task after changes
        /// </summary>
        [DataMember]
        public string AssignUserTo { get; set; }
    }
}
