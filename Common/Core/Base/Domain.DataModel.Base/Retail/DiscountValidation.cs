using System;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
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

        public bool OfferIsValid()
        {
            bool withinBounds = true;
            DateTime start = new DateTime();
            DateTime end = new DateTime();
            bool afterMidnight = false;

            if (this != null)
            {
                switch (DateTime.Now.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        {
                            withinBounds = this.MondayWithinBounds;
                            start = this.MondayStart;
                            end = this.MondayEnd;
                            afterMidnight = this.MondayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Tuesday:
                        {
                            withinBounds = this.TuesdayWithinBounds;
                            start = this.TuesdayStart;
                            end = this.TuesdayEnd;
                            afterMidnight = this.TuesdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Wednesday:
                        {
                            withinBounds = this.WednesdayWithinBounds;
                            start = this.WednesdayStart;
                            end = this.WednesdayEnd;
                            afterMidnight = this.WednesdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Thursday:
                        {
                            withinBounds = this.ThursdayWithinBounds;
                            start = this.ThursdayStart;
                            end = this.ThursdayEnd;
                            afterMidnight = this.ThursdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Friday:
                        {
                            withinBounds = this.FridayWithinBounds;
                            start = this.FridayStart;
                            end = this.FridayEnd;
                            afterMidnight = this.FridayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Saturday:
                        {
                            withinBounds = this.SaturdayWithinBounds;
                            start = this.SaturdayStart;
                            end = this.SaturdayEnd;
                            afterMidnight = this.SaturdayEndAfterMidnight;
                            break;
                        }
                    case DayOfWeek.Sunday:
                        {
                            withinBounds = this.SundayWithinBounds;
                            start = this.SundayStart;
                            end = this.SundayEnd;
                            afterMidnight = this.SundayEndAfterMidnight;
                            break;
                        }
                }

                if (this.IsMinDate(start) && this.IsMinDate(end))
                {
                    withinBounds = this.TimeWithinBounds;
                    start = this.StartTime;
                    end = this.EndTime;
                    afterMidnight = this.EndAfterMidnight;
                }
            }

            if (this.IsMinDate(start) && this.IsMinDate(end))
                return withinBounds;
            if (DateTime.Now >= start && DateTime.Now <= end)
                return withinBounds;
            if (afterMidnight && (DateTime.Now >= start || DateTime.Now <= end))
                return withinBounds;

            return false;
        }

        private bool IsMinDate(DateTime date)
        {
            if (date == DateTime.MinValue)
                return true;
            if ((date.Year == 1754 || date.Year == 1753 || date.Year == 1900) && date.Month == 1 && date.Day == 1)
                return true;
            return false;
        }
    }
}
