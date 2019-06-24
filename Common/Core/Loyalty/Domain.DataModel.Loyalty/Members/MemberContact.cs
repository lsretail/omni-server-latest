using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Members
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017"), KnownType(typeof(UnknownMemberContact))]
    public class MemberContact : Entity, IDisposable
    {
        #region Member variables

        private OneList basket;
        private OneList wishList;
        private List<Notification> notifications;

        #endregion

        #region Properties

        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Initials { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string MiddleName { get; set; }
        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public List<Address> Addresses { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public string MobilePhone { get; set; }
        [DataMember]
        public Gender Gender { get; set; }
        [DataMember]
        public MaritalStatus MaritalStatus { get; set; }
        [DataMember]
        public DateTime BirthDay { get; set; }

        //Due to circular reference
        [DataMember]
        public Device LoggedOnToDevice { get; set; }

        [DataMember]
        public string Name
        {
            get
            {
                string name = FirstName;
                if (string.IsNullOrEmpty(MiddleName) == false)
                {
                    name += " " + MiddleName;
                }
                name += " " + LastName;
                return name;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                string[] names = value.Split(' ');
                if (names.Count() == 1)
                {
                    FirstName = value;
                    MiddleName = string.Empty;
                    LastName = string.Empty;
                }
                else if (names.Count() == 2)
                {
                    FirstName = names[0];
                    MiddleName = string.Empty;
                    LastName = names[1];
                }
                else if (names.Count() > 2)
                {
                    FirstName = names[0];
                    MiddleName = string.Empty;
                    LastName = names[names.Count() - 1];
                    for (int i = 1; i < (names.Count() - 1); i++)
                    {
                        MiddleName += names[i];
                    }
                }
            }
        }

        [DataMember]
        public List<Notification> Notifications
        {
            get { return notifications; }
            set
            {
                if (value == null)
                    notifications = new List<Notification>();
                else
                    notifications = value.OrderBy(x => x.Status)
                                     .ThenByDescending(x => x.ExpiryDate)
                                     .ThenByDescending(x => x.Description)
                                     .ToList();
            }
        }

        [DataMember]
        public List<PublishedOffer> PublishedOffers { get; set; }
        [DataMember]
        public List<Profile> Profiles { get; set; }
        [DataMember]
        public List<SalesEntry> SalesEntries { get; set; }
        [IgnoreDataMember]
        public List<SalesEntry> TransactionOrderedByDate
        {
            get { return SalesEntries?.OrderByDescending(x => x.DocumentRegTime).ToList(); }//
        }

        [DataMember]
        public OneList WishList
        {
            get
            {
                if (wishList == null)
                {
                    wishList = new OneList()
                    {
                        CardId = (Cards.Count == 0) ? string.Empty : Cards[0].Id,
                        ListType = ListType.Wish
                    };
                }
                return wishList;
            }
            set 
            { 
                wishList = value; 
            }
        }

        [DataMember]
        public OneList Basket
        {
            get
            {
                if (basket == null)
                    basket = new OneList()
                    {
                        CardId = (Cards.Count == 0) ? string.Empty : Cards[0].Id,
                        ListType = ListType.Basket
                    };
                return basket;
            }
            set
            {
                basket = value;
            }
        }

        [DataMember]
        public OmniEnvironment Environment { get; set; }
        [DataMember]
        public string AlternateId { get; set; }
        [DataMember]
        public List<Card> Cards { get; set; }
        [DataMember]
        public Account Account { get; set; }
        [DataMember]
        public long RV { get; set; }

        #endregion

        #region Constructors

        public MemberContact()
            : this(null)
        {
            UserName = string.Empty;
        }

        public MemberContact(string id)
            : base(id)
        {
            UserName = string.Empty;
            Password = string.Empty;
            Initials = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;

            LoggedOnToDevice = null;
            BirthDay = new DateTime(1900, 1, 1);
            Environment = new OmniEnvironment();
            Addresses = new List<Address>();
            Account = null;
            notifications = new List<Notification>();
            PublishedOffers = new List<PublishedOffer>();
            Profiles = new List<Profile>();
            SalesEntries = new List<SalesEntry>();
            Cards = new List<Card>();
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

        #endregion

        public Card GetCard(string id)
        {
            return Cards.Find(c => c.Id == id);
        }

        public static bool NameContainsFirstName(string name)
        {
            return !string.IsNullOrEmpty(name);
        }

        public static bool NameContainsLastName(string name)
        {
            string[] names = name.Trim(' ').Split(' ');
            return names.Count() >= 2;
        }

        public string FormatPoints(DecimalUtils.DecimalFormat decimalFormat)
        {
            string points = "0";
            if (Account != null)
            {
                points = Account.PointBalance.ToString("N0", DecimalUtils.GetCultureInfo(decimalFormat));
            }
            return points;
        }

        //note> only clones the basic members, references to custom items remain the sam
        public MemberContact ShallowCopy()
        {
            MemberContact temp = (MemberContact)MemberwiseClone();
            return temp;
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public enum Gender
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Male = 1,
        [EnumMember]
        Female = 2,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/POS/2014/10")]
    public enum MaritalStatus
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Single = 1,
        [EnumMember]
        Married = 2,
        [EnumMember]
        Divorced = 3,
        [EnumMember]
        Widowed = 4,
    }
}
