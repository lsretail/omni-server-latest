using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Dal
{
    public class OfferRepository : BaseRepository
    {
        public OfferRepository(BOConfiguration config) : base(config)
        {
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

        private OfferDetails ReaderToOfferDetail(SqlDataReader reader)
        {
            OfferDetails det = new OfferDetails()
            {
                OfferId = SQLHelper.GetString(reader["Offer No_"]),
                LineNumber = SQLHelper.GetString(reader["Line No_"]),
                Description = SQLHelper.GetString(reader["Description"])
            };

            ImageRepository irep = new ImageRepository(config);
            det.Image = irep.ImageGetByKey("Published Offer Detail Line", det.OfferId, det.LineNumber, string.Empty, 1, false).FirstOrDefault();
            return det;
        }
    }
}
