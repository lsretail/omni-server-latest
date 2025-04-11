﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplCustomerResponse : IDisposable
    {
        public ReplCustomerResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Customers = new List<ReplCustomer>();
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
                if (Customers != null)
                    Customers.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplCustomer> Customers { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplCustomer : IDisposable
    {
        public ReplCustomer()
        {
            IsDeleted = false;
            Id = string.Empty;
            AccountNumber = string.Empty;
            Name = string.Empty;
            UserName = string.Empty;
            ClubCode = string.Empty;
            SchemeCode = string.Empty;
            PriceGroup = string.Empty;
            DiscountGroup = string.Empty;
            ShippingLocation = string.Empty;
            PaymentTerms = string.Empty;

            Street = string.Empty;
            City = string.Empty;
            ZipCode = string.Empty;
            State = string.Empty;
            County = string.Empty;
            Country = string.Empty;
            URL = string.Empty;
            Email = string.Empty;
            CellularPhone = string.Empty;
            PhoneLocal = string.Empty;
            Currency = string.Empty;
            TaxGroup = string.Empty;
            FirstName = string.Empty;
            MiddleName = string.Empty;
            LastName = string.Empty;
            NamePrefix = string.Empty;
            NameSuffix = string.Empty;
            ReceiptOption = 0;
            ReceiptEmail = string.Empty;
            Blocked = 0;
            IncludeTax = 0;
			
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

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string AccountNumber { get; set; }
        [DataMember]
        public string CustomerId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Street { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string ZipCode { get; set; }
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public string County { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string CellularPhone { get; set; }
        [DataMember]
        public string PhoneLocal { get; set; }
        [DataMember]
        public string Currency { get; set; }
        [DataMember]
        public string TaxGroup { get; set; }
        [DataMember]
        public string PriceGroup { get; set; }
        [DataMember]
        public string DiscountGroup { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string MiddleName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string NamePrefix { get; set; }
        [DataMember]
        public string NameSuffix { get; set; }
        [DataMember]
        public int ReceiptOption { get; set; }
        [DataMember]
        public string ReceiptEmail { get; set; }
        [DataMember]
        public int Blocked { get; set; }
        [DataMember]
        public int IncludeTax { get; set; }
        [DataMember]
        public string PaymentTerms { get; set; }
        [DataMember]
        public string ShippingLocation { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string ClubCode { get; set; }
        [DataMember]
        public string SchemeCode { get; set; }
        [DataMember]
        public SendEmail SendReceiptByEMail { get; set; }
        [DataMember]
        public string GuestType { get; set; }

        [DataMember]
        public List<Card> Cards { get; set; }
    }
}

