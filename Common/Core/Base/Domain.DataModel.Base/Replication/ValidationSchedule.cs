using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class ReplValidationScheduleResponse : IDisposable
    {
        public ReplValidationScheduleResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Schedules = new List<ReplValidationSchedule>();
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
                if (Schedules != null)
                    Schedules.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplValidationSchedule> Schedules { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class ReplValidationSchedule : IDisposable
    {
        public ReplValidationSchedule()
        {
            Id = string.Empty;
            Description = string.Empty;
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
                if (Lines != null)
                    Lines.Clear();
            }
        }

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public List<ValidationScheduleLine> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class ValidationScheduleLine : IDisposable
    {
        public ValidationScheduleLine()
        {
            Description = string.Empty;
            Comment = string.Empty;
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
        public int LineNo { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public int Priority { get; set; }
        [DataMember]
        public VSDateSchedule DateSchedule { get; set; }
        [DataMember]
        public VSTimeSchedule TimeSchedule { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class VSDateSchedule : IDisposable
    {
        public VSDateSchedule()
        {
            Id = string.Empty;
            Description = string.Empty;
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
                if (Lines != null)
                    Lines.Clear();
            }
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool Mondays { get; set; }
        [DataMember]
        public bool Tuesdays { get; set; }
        [DataMember]
        public bool Wednesdays { get; set; }
        [DataMember]
        public bool Thursdays { get; set; }
        [DataMember]
        public bool Fridays { get; set; }
        [DataMember]
        public bool Saturdays { get; set; }
        [DataMember]
        public bool Sundays { get; set; }
        [DataMember]
        public bool ValidAllWeekdays { get; set; }
        [DataMember]
        public List<VSDateScheduleLine> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class VSDateScheduleLine : IDisposable
    {
        public VSDateScheduleLine()
        {
            StartingDate = new DateTime(1970, 1, 1);
            EndingDate = new DateTime(1970, 1, 1);
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
        public int LineNo { get; set; }
        [DataMember]
        public bool Exclude { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartingDate { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime EndingDate { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class VSTimeSchedule : IDisposable
    {
        public VSTimeSchedule()
        {
            Id = string.Empty;
            Description = string.Empty;
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
                if (Lines != null)
                    Lines.Clear();
            }
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public VSTimeScheduleType Type { get; set; }
        [DataMember]
        public List<VSTimeScheduleLine> Lines { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2021")]
    public class VSTimeScheduleLine : IDisposable
    {
        public VSTimeScheduleLine(bool isJson)
        {
            Period = string.Empty;
            DiningDurationCode = string.Empty;
            TimeFrom = new DateTime((isJson) ? 1970 : 1900, 1, 1);
            TimeTo = new DateTime((isJson) ? 1970 : 1900, 1, 1);
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
        public string Period { get; set; }
        [DataMember]
        public string DiningDurationCode { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeFrom { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TimeTo { get; set; }
        [DataMember]
        public bool TimeToIsPastMidnight { get; set; }
        [DataMember]
        public bool SelectedByDefault { get; set; }
        [DataMember]
        public int ReservationInterval { get; set; }
    }
}
