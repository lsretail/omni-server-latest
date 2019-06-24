using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSRetail.Omni.Domain.DataModel.Pos.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplTerminalResponse : IDisposable
    {
        public ReplTerminalResponse()
        {
            LastKey = "";
            MaxKey = "";
            RecordsRemaining = 0;
            Terminals = new List<ReplTerminal>();
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
                if (Terminals != null)
                    Terminals.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplTerminal> Terminals { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class ReplTerminal : IDisposable
    {
        public ReplTerminal()
        {
            IsDeleted = false;
            Id = string.Empty;
            Name = string.Empty;
            StoreId = string.Empty;
            EFTStoreId = string.Empty;
            SFTTerminalId = string.Empty;
            HardwareProfile = string.Empty;
            FunctionalityProfile = string.Empty;
            VisualProfile = string.Empty;
            TerminalType = 0;
            HospTypeFilter = string.Empty;
            DefaultHospType = string.Empty;
            MainMenuID = string.Empty;
            Features = new FeatureFlags();
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
        public string StoreId { get; set; }
        [DataMember]
        public string EFTStoreId { get; set; }
        [DataMember]
        public string SFTTerminalId { get; set; }
        [DataMember]
        public string HardwareProfile { get; set; }
        [DataMember]
        public string VisualProfile { get; set; }
        [DataMember]
        public string FunctionalityProfile { get; set; }
        [DataMember]
        public int DeviceType { get; set; }
        /// <summary>
        /// TerminalType.  1 = Mobile POS, 3 = Hosp. Mobile POS
        /// </summary>
        [DataMember]
        public int TerminalType { get; set; }
        [DataMember]
        public string HospTypeFilter { get; set; }
        [DataMember]
        public string DefaultHospType { get; set; }
        [DataMember]
        public string MainMenuID { get; set; }
        [DataMember]
        public FeatureFlags Features { get; set; }
    }
}
