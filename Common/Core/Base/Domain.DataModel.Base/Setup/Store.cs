using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017"), KnownType(typeof(UnknownStore))]
    [System.Xml.Serialization.XmlInclude(typeof(UnknownStore))]
	public class Store : Entity
    {
        public Store(string id) : base(id)
        {
            IsClickAndCollect = false;
            Description = string.Empty;
            Phone = string.Empty;
            TaxGroupId = string.Empty;
            Latitude = 0.0;
            Longitude = 0.0;
            Distance = 0.0;
            Address = new Address();
            Currency = new UnknownCurrency();
            Images = new List<ImageView>();
            StoreHours = new List<StoreHours>();
            StoreServices = new List<StoreServices>();
            SourcingLocations = new List<SourcingLocation>();
            HospSalesTypes = new List<SalesType>();
            Attributes = new List<RetailAttribute>();
        }

        public Store()
            : this(string.Empty)
        {
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
                Address?.Dispose();
                Images?.Clear();
                StoreHours?.Clear();
                StoreServices?.Clear();
                SourcingLocations?.Clear();
                Attributes?.Clear();
                HospSalesTypes?.Clear();
            }
        }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public Address Address { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public List<ImageView> Images { get; set; }
        [DataMember]
        public double Latitude { get; set; }
        [DataMember]
        public double Longitude { get; set; }
        /// <summary>
        /// Distance from current location
        /// </summary>
        [DataMember]
        public double Distance { get; set; }
        [DataMember]
        public bool IsClickAndCollect { get; set; }
        [DataMember]
        public bool IsLoyalty { get; set; }
        [DataMember]
        public bool IsWebStore { get; set; }
        [DataMember]
        public bool UseSourcingLocation { get; set; }
        [DataMember]
        public List<SalesType> HospSalesTypes { get; set; }
        [DataMember]
        public List<SourcingLocation> SourcingLocations { get; set; }
        [DataMember]
        public string WebOmniTerminal { get; set; }
        [DataMember]
        public string WebOmniStaff { get; set; }
        [DataMember]
        public Currency Currency { get; set; }
        [DataMember]
        public string CultureName { get; set; }
        [DataMember]
        public string UseDefaultCustomer { get; set; }
        [DataMember]
        public Customer DefaultCustomer { get; set; }
        [DataMember]
        public string FunctionalityProfileId { get; set; }
        [DataMember]
        public string TaxGroupId { get; set; }
        [DataMember]
        public List<StoreHours> StoreHours { get; set; }
        [DataMember]
        public List<StoreServices> StoreServices { get; set; }
        [DataMember]
        public List<RetailAttribute> Attributes { get; set; }

        public string FormatAddress
        {
            get
            {
                string address = string.Empty;
                if (Address != null)
                {
                    address += Address.Address1;

                    if (!string.IsNullOrEmpty(Address.Address2))
                        address += Environment.NewLine + Address.Address2;

                    address += Environment.NewLine + Address.City;

                    if (!string.IsNullOrEmpty(Address.StateProvinceRegion))
                        address += ", " + Address.StateProvinceRegion;

                    if (!string.IsNullOrEmpty(Address.PostCode))
                        address += ", " + Address.PostCode;
                }

                return address;
            }
        }

        public string FormatStoreHours
        {
            get
            {
                if (StoreHours == null || !StoreHours.Any())
                {
                    return string.Empty;
                }

                var storeHours = string.Empty;

                foreach (var storeHour in StoreHours)
                {
                    storeHours += $"{storeHour.NameOfDay} {storeHour.OpenFrom:t} - {storeHour.OpenTo:t}" + Environment.NewLine;
                }
                
                return storeHours.TrimEnd(Environment.NewLine.ToCharArray());
            }
        }

        public string FormatStoreHoursName
        {
            get
            {
                if (StoreHours == null || !StoreHours.Any())
                {
                    return string.Empty;
                }

                var storeHours = string.Empty;

                foreach (var storeHourGroup in StoreHours.GroupBy(x => x.DayOfWeek))
                {
                    var storeOpeningHours = storeHourGroup.FirstOrDefault(x => x.StoreId == Id);

                    if (storeOpeningHours == null)
                    {
                        storeOpeningHours = storeHourGroup.FirstOrDefault();
                    }

                    if (storeOpeningHours != null)
                    {
                        storeHours += $"{storeOpeningHours.NameOfDay} "+ Environment.NewLine;
                    }
                }

                return storeHours.TrimEnd(Environment.NewLine.ToCharArray());
            }
        }

        public string FormatStoreHoursOpen
        {
            get
            {
                if (StoreHours == null || !StoreHours.Any())
                {
                    return string.Empty;
                }

                var storeHours = string.Empty;

                foreach (var storeHourGroup in StoreHours.GroupBy(x => x.DayOfWeek))
                {
                    var storeOpeningHours = storeHourGroup.FirstOrDefault(x => x.StoreId == Id);

                    if (storeOpeningHours == null)
                    {
                        storeOpeningHours = storeHourGroup.FirstOrDefault();
                    }

                    if (storeOpeningHours != null)
                    {
                        storeHours += $"{storeOpeningHours.OpenFrom:t} - {storeOpeningHours.OpenTo:t}" + Environment.NewLine;
                    }
                }
                //var storeHours = string.Empty;

                /*foreach (var storeHour in StoreHours)
                {
                    storeHours += $"{storeHour.OpenFrom:t} - {storeHour.OpenTo:t}" + Environment.NewLine;
                }*/

                return storeHours.TrimEnd(Environment.NewLine.ToCharArray());
            }
        }

        public ImageView DefaultImage
        {
            get { return (Images != null && Images.Count > 0) ? Images[0] : null; }
        }

        #region opening hours

        /*
          Example how to display opening hours...
          Opening hours:  
             Mon - Fri :  store.OpenFromWeekdays() - store.OpenToWeekdays()
             Sat : store.OpenFromSaturday() - store.OpenToSaturday()
             Sun : store.OpenFromSunday() - store.OpenToSunday()
        */
        public string OpenFromWeekdays()
        {
            string open = "";
            foreach (StoreHours storeHrs in StoreHours)
            {
                //assuming that opening hours are same monday - friday
                // check monday
                if (storeHrs.DayOfWeek == 1)
                {
                    if (storeHrs.OpenFrom.Hour == 0 && storeHrs.OpenFrom.Minute == 0 && storeHrs.OpenFrom.Second == 0)
                        open = ""; //closed
                    else
                        open = string.Format("{0}:{1}", storeHrs.OpenFrom.ToString("HH"), storeHrs.OpenFrom.ToString("mm"));
                    break;
                }
            }
            return open;
        }

        public string OpenToWeekdays()
        {
            string open = "";
            foreach (StoreHours storeHrs in StoreHours)
            {
                //assuming that opening hours are same monday - friday
                // check monday
                if (storeHrs.DayOfWeek == 1)
                {
                    if (storeHrs.OpenTo.Hour == 0 && storeHrs.OpenTo.Minute == 0 && storeHrs.OpenTo.Second == 0)
                        open = ""; //closed
                    else
                        open = string.Format("{0}:{1}", storeHrs.OpenTo.ToString("HH"), storeHrs.OpenTo.ToString("mm"));
                    break;
                }
            }
            return open;
        }

        public string OpenFromSaturday()
        {
            string open = "";
            foreach (StoreHours storeHrs in StoreHours)
            {
                // check Saturday, if closed on saturday then dayofweek does not exist
                if (storeHrs.DayOfWeek == 6)
                {
                    open = string.Format("{0}:{1}", storeHrs.OpenFrom.ToString("HH"), storeHrs.OpenFrom.ToString("mm"));
                    break;
                }
            }
            return open;
        }

        public string OpenToSaturday()
        {
            string open = "";
            foreach (StoreHours storeHrs in StoreHours)
            {
                // check Saturday, if closed on saturday then dayofweek does not exist
                if (storeHrs.DayOfWeek == 6)
                {
                    open = string.Format("{0}:{1}", storeHrs.OpenTo.ToString("HH"), storeHrs.OpenTo.ToString("mm"));
                    break;
                }
            }
            return open;
        }

        public string OpenFromSunday()
        {
            string open = "";
            foreach (StoreHours storeHrs in StoreHours)
            {
                // check Sunday, if closed on saturday then dayofweek does not exist
                if (storeHrs.DayOfWeek == 0)
                {
                    open = string.Format("{0}:{1}", storeHrs.OpenFrom.ToString("HH"), storeHrs.OpenFrom.ToString("mm"));
                    break;
                }
            }
            return open;
        }

        public string OpenToSunday()
        {
            string open = "";
            foreach (StoreHours storeHrs in StoreHours)
            {
                // check Sunday, if closed on saturday then dayofweek does not exist
                if (storeHrs.DayOfWeek == 0)
                {
                    open = string.Format("{0}:{1}", storeHrs.OpenTo.ToString("HH"), storeHrs.OpenTo.ToString("mm"));
                    break;
                }
            }
            return open;
        }

        #endregion opening hours

        #region Distance

        public struct Position
        {
            public double Latitude;
            public double Longitude;
        }

        [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
        public enum DistanceType
        {
            Miles,
            Kilometers
        };

        /// Returns the distance in miles or kilometers of any two  
        /// latitude / longitude points using the Haversine formula  
        public double CalculateDistance(Position pos1, Position pos2, DistanceType type)
        {
            double R = (type == DistanceType.Miles) ? 3960 : 6371;
            double dLat = this.toRadian(pos2.Latitude - pos1.Latitude);
            double dLon = this.toRadian(pos2.Longitude - pos1.Longitude);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(this.toRadian(pos1.Latitude)) * Math.Cos(this.toRadian(pos2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = R * c;
            return d;
        }

        /// Convert to Radians.  
        private double toRadian(double val)
        {
            return (Math.PI / 180) * val;
        }

        #endregion
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class SalesType
    {
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Description { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum StoreGetType
    {
        [EnumMember]
        All,
        [EnumMember]
        ClickAndCollect,
        [EnumMember]
        WebStore
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class SourcingLocation : Entity
    {
        [DataMember]
        public int Priority { get; set; }
        [DataMember]
        public bool CanShip { get; set; }
        [DataMember]
        public bool CanCollect { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
}
