using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class OfferRepository : BaseRepository
    {
        public OfferRepository() : base()
        {
        }

        public List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems)
        {
            List<LoyItem> list = new List<LoyItem>();

            PublishedOffer puboffer = PublishedOfferGetById(pubOfferId);
            if (puboffer == null)
                return list;

            switch (puboffer.Code)
            {
                case OfferDiscountType.Promotion:
                case OfferDiscountType.Deal:
                    Offer offer = OfferGetById(puboffer.OfferId);
                    break;

                case OfferDiscountType.Multibuy:
                case OfferDiscountType.MixAndMatch:
                case OfferDiscountType.DiscountOffer:
                case OfferDiscountType.TotalDiscount:
                case OfferDiscountType.TenderType:
                case OfferDiscountType.ItemPoint:
                case OfferDiscountType.LineDiscount:
                    break;

                case OfferDiscountType.Coupon:
                    break;
            }
            return list;
        }

        public PublishedOffer PublishedOfferGetById(string pubOfferId)
        {
            PublishedOffer puboffer = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[No_],mt.[Primary Text],mt.[Secondary Text],mt.[Discount Type],mt.[Discount No_],mt.[Offer Category],vp.[Description],vp.[Ending Date]" +
                                          " FROM [" + navCompanyName + "Published Offer] mt" +
                                          " LEFT JOIN [" + navCompanyName + "Validation Period] vp ON vp.[ID]=mt.[Validation Period ID]" +
                                          " WHERE mt.[No_]=@id";

                    command.Parameters.AddWithValue("@id", pubOfferId);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            puboffer = ReaderToPubOffer(reader);
                        }
                    }
                }
                connection.Close();
            }
            return puboffer;
        }

        public Offer OfferGetById(string id)
        {
            Offer offer = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[No_],mt.[Type],mt.[Description],mt.[Offer Type],vp.[Ending Date]" +
                                          " FROM [" + navCompanyName + "Offer] mt" +
                                          " LEFT JOIN [" + navCompanyName + "Validation Period] vp ON vp.[ID]=mt.[Validation Period ID]" +
                                          " WHERE mt.[No_]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            offer = ReaderToOffer(reader);
                        }
                    }
                }
                connection.Close();
            }
            return offer;
        }

        public List<OfferDetails> OfferDetailGetByOfferId(string id)
        {
            List<OfferDetails> list = new List<OfferDetails>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT mt.[Offer No_],mt.[Line No_],mt.[Description]" +
                                          " FROM [" + navCompanyName + "Published Offer Detail Line] mt" +
                                          " WHERE mt.[Offer No_]=@id";

                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(ReaderToOfferDetail(reader));
                        }
                    }
                }
                connection.Close();
            }
            return list;
        }

        private PublishedOffer ReaderToPubOffer(SqlDataReader reader)
        {
            PublishedOffer offer = new PublishedOffer()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                Description = SQLHelper.GetString(reader["Primary Text"]),
                Details = SQLHelper.GetString(reader["Secondary Text"]),
                Code = (OfferDiscountType)SQLHelper.GetInt32(reader["Discount Type"]),
                Type = (OfferType)SQLHelper.GetInt32(reader["Offer Category"]),
                ExpirationDate = SQLHelper.GetDateTime(reader["Ending Date"]),
                OfferId = SQLHelper.GetString(reader["Discount No_"]),
                ValidationText = SQLHelper.GetString(reader["Description"])
            };

            offer.OfferDetails = OfferDetailGetByOfferId(offer.Id);

            ImageRepository irep = new ImageRepository();
            offer.Images = irep.ImageGetByKey("Published Offer", offer.Id, string.Empty, string.Empty, 0, false);
            return offer;
        }

        private Offer ReaderToOffer(SqlDataReader reader)
        {
            return new Offer()
            {
                Id = SQLHelper.GetString(reader["No_"]),
                PrimaryText = SQLHelper.GetString(reader["Description"]),
                Code = (OfferDiscountType)SQLHelper.GetInt32(reader["Offer Type"]),
                Type = (OfferDiscountType)SQLHelper.GetInt32(reader["Type"]),
                ExpirationDate = SQLHelper.GetDateTime(reader["Ending Date"])
            };
        }

        private OfferDetails ReaderToOfferDetail(SqlDataReader reader)
        {
            OfferDetails det = new OfferDetails()
            {
                OfferId = SQLHelper.GetString(reader["Offer No_"]),
                LineNumber = SQLHelper.GetString(reader["Line No_"]),
                Description = SQLHelper.GetString(reader["Description"])
            };

            ImageRepository irep = new ImageRepository();
            det.Image = irep.ImageGetByKey("Published Offer Detail Line", det.OfferId, det.LineNumber, string.Empty, 1, false).FirstOrDefault();
            return det;
        }
    }
}
