using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplRequest : IDisposable
    {
        public ReplRequest()
        {
            StoreId = string.Empty;
            TerminalId = string.Empty;
            LastKey = string.Empty;
            MaxKey = string.Empty;
            BatchSize = 1000;
            FullReplication = true;
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
        /// ID of a Store to replicate data for (Required)
        /// </summary>
        [DataMember(IsRequired = true)]
        public string StoreId { get; set; }
        /// <summary>
        /// ID of a Terminal that request replication data (Required)
        /// </summary>
        [DataMember(IsRequired = true)]
        public string TerminalId { get; set; }
        /// <summary>
        /// Last Key returned from previous replication call (Required)
        /// </summary>
        /// <remarks>
        /// When starting new replication, LastKey should be set to 0.
        /// Result object will include LastKey value for the batch of data being returned.
        /// The LastKey for each ReplEcommXX call need to be stored so once the full replication has been done 
        /// a delta replication can be done (setting FullReplication to false and LastKey to the last value returned). 
        /// </remarks>
        [DataMember(IsRequired = true)]
        public string LastKey { get; set; }
        /// <summary>
        /// Max Key for replication (Required)
        /// </summary>
        /// <remarks>
        /// When starting new replication, MaxKey should be set to 0.
        /// Result object will include MaxKey value for the replication.
        /// When MaxKey and LastKey are same, replication is done.
        /// </remarks>
        [DataMember(IsRequired = true)]
        public string MaxKey { get; set; }
        /// <summary>
        /// Size of a replication batch (Required)
        /// </summary>
        [DataMember(IsRequired = true)]
        public int BatchSize { get; set; }
        /// <summary>
        /// Full or delta replication (Required)
        /// </summary>
        /// <remarks>
        /// True to get all data. LastKey set to 0.
        /// False to get only delta from last replication. Replicate from LastKey value.
        /// </remarks>
        [DataMember(IsRequired = true)]
        public bool FullReplication { get; set; }

        public override string ToString()
        {
            return string.Format("StoreId:{0} TerminalId:{1} LastKey:{2} MaxKey:{3} BatchSize:{4} FullReplication:{5}",
                StoreId, TerminalId, LastKey, MaxKey, BatchSize.ToString(), FullReplication.ToString());
        }
    }
}
