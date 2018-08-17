using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.OmniTasks
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class OmniTaskLogLine : IDisposable
    {
        public OmniTaskLogLine()
        {
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
        /// TaskLogLine Id
        /// </summary>
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string TaskLineId { get; set; }
        /// <summary>
        /// Last Modified Time
        /// </summary>
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
        public OmniTaskLineStatus StatusFrom { get; set; }
        /// <summary>
        /// Status of task after changees
        /// </summary>
        [DataMember]
        public OmniTaskLineStatus StatusTo { get; set; }
    }
}
