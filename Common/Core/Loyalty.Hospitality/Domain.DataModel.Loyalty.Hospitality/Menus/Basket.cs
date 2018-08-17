using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Hospitality.Menus
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class Basket : Entity, IAggregateRoot
    {
        public enum BasketState
        {
            Normal = 0,
            Dirty = 1,
            Calculating = 2
        }

        public Basket()
        {
            Init();
        }

		public Basket(string id, List<BasketItem> items, List<PublishedOffer> publishedOffers)
            : base(id)
        {
            Items = items;
			PublishedOffers = publishedOffers;
            State = BasketState.Normal;
        }

        [DataMember]
        public List<BasketItem> Items { get; private set; }

		[DataMember]
		public List<PublishedOffer> PublishedOffers { get; private set; }

        [IgnoreDataMember]
        public BasketState State { get; set; }

        [IgnoreDataMember]
        public decimal ShippingPrice { get; set; }

        [IgnoreDataMember]
        public decimal NetAmount { get; set; }

        [IgnoreDataMember]
        public decimal TAXAmount { get; set; }

        [IgnoreDataMember]
        public decimal Amount { get; set; }

        private void Init()
        {
            Items = new List<BasketItem>();
			PublishedOffers = new List<PublishedOffer>();
            State = BasketState.Normal;
        }

        public void AddItemToBasket(BasketItem item)
        {
            Items.Insert(0, item);
            State = BasketState.Dirty;
        }

		public void AddPublishedOfferToBasket(PublishedOffer offer)
        {
            PublishedOffers.Add(offer);
            State = BasketState.Dirty;
        }

        public void RemoveItemFromBasket(int position)
        {
            Items.RemoveAt(position);
            State = BasketState.Dirty;
        }

		public void RemoveItemFromBasket(BasketItem item)
		{
			Items.Remove(item);
			State = BasketState.Dirty;
		}

		public void RemovePublishedOfferFromBasket(PublishedOffer offer)
        {
			PublishedOffers.Remove(offer);
            State = BasketState.Dirty;
        }

        public void RemovePublishedOfferFromBasket(int position)
        {
            PublishedOffers.RemoveAt(position);
            State = BasketState.Dirty;
        }
			

        public void Clear()
        {
            State = BasketState.Normal;

            PublishedOffers.Clear();
            Items.Clear();

            ShippingPrice = 0m;
            NetAmount = 0m;
            TAXAmount = 0m;
            Amount = 0m;
        }
    }
}
