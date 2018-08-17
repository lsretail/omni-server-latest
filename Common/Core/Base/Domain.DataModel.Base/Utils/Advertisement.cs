using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Utils
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class Advertisement : IDisposable
    {
        //Advertise 
        public Advertisement(string id)
        {
            Id = id;  //Ad Id
            MenuIds = new List<string>();   //Menu Ids, diffent menus can have different ads
            Description = string.Empty; //short description, also tell it ad only applies to which stores?
            //StoreIds = new List<string>();  //gives the app a change to say ad is only for store S0001 ?
            ImageView = new ImageView();
            ExpirationDate = new DateTime(2100, 1, 1); //json does not handle datetime.minvalue very well
            RV = 0; // version returned
            AdType = AdvertisementType.None;
            AdValue = string.Empty;
        } 

        public Advertisement()  : this(string.Empty)
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
                if (MenuIds != null)
                    MenuIds.Clear();
                if (ImageView != null)
                    ImageView.Dispose();
            }
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        //[DataMember]
        //public List<string> StoreIds { get; set; }
        [DataMember]
        public ImageView ImageView { get; set; }
        [DataMember]
        public DateTime ExpirationDate { get; set; }
        [DataMember]
        public long RV { get; set; }
        [DataMember]
        public AdvertisementType AdType { get; set; }
        [DataMember]
        public string AdValue { get; set; }
        [DataMember]
        public List<string> MenuIds { get; set; }
#if WCFSERVER 
        //not all data goes to wcf clients
        //[System.Xml.Serialization.XmlIgnore] 
        public string xmlData { get; set; }
#endif
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    [Flags]
    public enum AdvertisementType
    {
        [EnumMember]
        None = 0,        //simply an image, nothing behind it
        [EnumMember]
        ItemId = 1,      //itemId was returned
        [EnumMember]
        Url = 2,         //ad has a url tied  to it
        [EnumMember]
        MenuNodeId = 3,  //menu node id - category level.  Hospitality only..
        [EnumMember]
        Deal = 4,    // 
        //[EnumMember]
        //FaceBook = 4,    //open facebook app from our app.  fb://profile/113810631976867  if facebook app is installed
        //[EnumMember]
        //YouTube = 5,     //open YouTube app from our app.  vnd.youtube://" + id
        //[EnumMember]
        //Twitter = 6,     //open Twitter app from our app.  ?
        //[EnumMember]
        //Html = 7,        // html string returned.  Show more detail in htmlviewer ?
        //[EnumMember]
        //OfferId = 8,     //OfferId was returned -   ? user may not have this offer any more
        //[EnumMember]
        //CouponId = 9,    //CouponId was returned  - ? user may not have this offer any more
    }
}
