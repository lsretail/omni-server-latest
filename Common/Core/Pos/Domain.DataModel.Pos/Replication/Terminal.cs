using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
            Id = "";
            Name = "";
            StoreId = "";
            EFTStoreId = "";
            SFTTerminalId = "";
            HardwareProfile = "";
            FunctionalityProfile = "";
            ExitAfterEachTransaction = 9;
            TerminalType = 0;
            HospTypeFilter = "";
            DefaultHospType = "";
            AutoLogOffTimeOut = 0;  //boolean in NAV
            AutoLogOffAfterMin = 0; //
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
        public int ExitAfterEachTransaction { get; set; }
        [DataMember]
        public int AutoLogOffTimeOut
        {
            get { return (AutoLogOffAfterMin == 0) ? 0 : 1; }
            set { }
        }

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
        public int AutoLogOffAfterMin { get; set; }
        [DataMember]
        public string MainMenuID { get; set; }
    }
}
