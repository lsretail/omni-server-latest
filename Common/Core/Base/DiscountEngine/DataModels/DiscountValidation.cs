using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.DiscountEngine.DataModels
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/DiscountEngine/2017")]
    public class DiscountValidation
    {
        public DiscountValidation()
        {
            Id = string.Empty;
            Description = string.Empty;
            StartDate = new DateTime(1900, 1, 1);
            EndDate = new DateTime(1900, 1, 1);
            StartTime = new DateTime(1900, 1, 1);
            EndTime = new DateTime(1900, 1, 1);
            MondayStart = new DateTime(1900, 1, 1);
            MondayEnd = new DateTime(1900, 1, 1);
            TuesdayStart = new DateTime(1900, 1, 1);
            TuesdayEnd = new DateTime(1900, 1, 1);
            WednesdayStart = new DateTime(1900, 1, 1);
            WednesdayEnd = new DateTime(1900, 1, 1);
            ThursdayStart = new DateTime(1900, 1, 1);
            ThursdayEnd = new DateTime(1900, 1, 1);
            FridayStart = new DateTime(1900, 1, 1);
            FridayEnd = new DateTime(1900, 1, 1);
            SaturdayStart = new DateTime(1900, 1, 1);
            SaturdayEnd = new DateTime(1900, 1, 1);
            SundayStart = new DateTime(1900, 1, 1);
            SundayEnd = new DateTime(1900, 1, 1);
            TimeWithinBounds = false;
            EndAfterMidnight = false;
            MondayWithinBounds = false;
            MondayEndAfterMidnight = false;
            TuesdayWithinBounds = false;
            TuesdayEndAfterMidnight = false;
            WednesdayWithinBounds = false;
            WednesdayEndAfterMidnight = false;
            ThursdayWithinBounds = false;
            ThursdayEndAfterMidnight = false;
            FridayWithinBounds = false;
            FridayEndAfterMidnight = false;
            SaturdayWithinBounds = false;
            SaturdayEndAfterMidnight = false;
            SundayWithinBounds = false;
            SundayEndAfterMidnight = false;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime EndDate { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartTime { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime EndTime { get; set; }

        [DataMember]
        public bool TimeWithinBounds { get; set; }

        [DataMember]
        public bool EndAfterMidnight { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime MondayStart { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime MondayEnd { get; set; }

        [DataMember]
        public bool MondayWithinBounds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TuesdayStart { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TuesdayEnd { get; set; }

        [DataMember]
        public bool TuesdayWithinBounds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime WednesdayStart { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime WednesdayEnd { get; set; }

        [DataMember]
        public bool WednesdayWithinBounds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ThursdayStart { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ThursdayEnd { get; set; }

        [DataMember]
        public bool ThursdayWithinBounds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime FridayStart { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime FridayEnd { get; set; }

        [DataMember]
        public bool FridayWithinBounds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime SaturdayStart { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime SaturdayEnd { get; set; }

        [DataMember]
        public bool SaturdayWithinBounds { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime SundayStart { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime SundayEnd { get; set; }

        [DataMember]
        public bool SundayWithinBounds { get; set; }

        [DataMember]
        public bool MondayEndAfterMidnight { get; set; }

        [DataMember]
        public bool TuesdayEndAfterMidnight { get; set; }

        [DataMember]
        public bool WednesdayEndAfterMidnight { get; set; }

        [DataMember]
        public bool ThursdayEndAfterMidnight { get; set; }

        [DataMember]
        public bool FridayEndAfterMidnight { get; set; }

        [DataMember]
        public bool SaturdayEndAfterMidnight { get; set; }

        [DataMember]
        public bool SundayEndAfterMidnight { get; set; }
    }
}
