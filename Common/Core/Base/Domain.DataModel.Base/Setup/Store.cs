using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Setup.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017"), KnownType(typeof(UnknownStore))]
    public class Store : Entity
    {
        public Store(string id) : base(id)
        {
            IsClickAndCollect = false;
            Description = string.Empty;
            Phone = string.Empty;
            Address = new Address();
            Images = new List<ImageView>();
            Latitude = 0.0;
            Longitude = 0.0;
            Distance = 0.0;
            Currency = new UnknownCurrency();
            StoreHours = new List<StoreHours>();
            StoreServices = new List<StoreServices>();
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
                if (Address != null)
                    Address.Dispose();
                if (Images != null)
                    Images.Clear();
                if (StoreHours != null)
                    StoreHours.Clear();
                if (StoreServices != null)
                    StoreServices.Clear();
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
}
