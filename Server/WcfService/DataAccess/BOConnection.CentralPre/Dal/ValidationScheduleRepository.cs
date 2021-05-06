using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Base.Replication;

namespace LSOmni.DataAccess.BOConnection.CentralPre.Dal
{
    public class ValidationScheduleRepository : BaseRepository
    {
        // Key : Id
        const int TABLEID = 10000955;

        public ValidationScheduleRepository(BOConfiguration config) : base(config)
        {
        }

        public List<ReplValidationSchedule> ReplicateValidationSchedule(int batchSize, bool fullReplication, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            if (string.IsNullOrWhiteSpace(lastKey))
                lastKey = "0";

            List<ReplValidationSchedule> list = new List<ReplValidationSchedule>();
            List<JscActions> actions = LoadActions(fullReplication, TABLEID, batchSize, ref lastKey, ref recordsRemaining);

            if (fullReplication == false)
            {
                string tmplastkey = lastKey;
                string mainlastkey = lastKey;

                if (actions.Count == 0)
                {
                    actions = LoadActions(fullReplication, 10000954, batchSize, ref tmplastkey, ref recordsRemaining);
                    if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                        mainlastkey = tmplastkey;
                }
                if (actions.Count == 0)
                {
                    actions = LoadActions(fullReplication, 10001311, batchSize, ref tmplastkey, ref recordsRemaining);
                    if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                        mainlastkey = tmplastkey;
                }
                if (actions.Count == 0)
                {
                    actions = LoadActions(fullReplication, 10001308, batchSize, ref tmplastkey, ref recordsRemaining);
                    if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                        mainlastkey = tmplastkey;
                }
                if (actions.Count == 0)
                {
                    actions = LoadActions(fullReplication, 10001466, batchSize, ref tmplastkey, ref recordsRemaining);
                    if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                        mainlastkey = tmplastkey;
                }
                if (actions.Count == 0)
                {
                    actions = LoadActions(fullReplication, 10001467, batchSize, ref tmplastkey, ref recordsRemaining);
                    if (Convert.ToInt32(tmplastkey) > Convert.ToInt32(mainlastkey))
                        mainlastkey = tmplastkey;
                }

                lastKey = mainlastkey;
                maxKey = mainlastkey;
                if (actions.Count == 0)
                {
                    // if this is update replication and there are no actions for any table, we skip the process
                    return list;
                }
            }

            // get all data
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = "SELECT [ID],[Description] FROM [" + navCompanyName + "LSC Validation Schedule$5ecfc871-5d82-43f1-9c54-59685e82318d]";

                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReplValidationSchedule sched = new ReplValidationSchedule()
                            {
                                Id = SQLHelper.GetString(reader["ID"]),
                                Description = SQLHelper.GetString(reader["Description"])
                            };
                            sched.Lines = GetScheduleLines(sched.Id);
                            list.Add(sched);
                        }
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            return list;
        }

        private List<ValidationScheduleLine> GetScheduleLines(string id)
        {
            List<ValidationScheduleLine> lines = new List<ValidationScheduleLine>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Line No_],[Description],[Date Schedule ID],[Time Schedule ID],[Priority],[Comment] " +
                            "FROM [" + navCompanyName + "LSC Validation Schedule Line$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                            "WHERE [Validation Schedule ID]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ValidationScheduleLine rec = new ValidationScheduleLine()
                            {
                                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                                Description = SQLHelper.GetString(reader["Description"]),
                                Comment = SQLHelper.GetString(reader["Comment"]),
                                Priority = SQLHelper.GetInt32(reader["Priority"])
                            };
                            rec.DateSchedule = GetDateSchedule(SQLHelper.GetString(reader["Date Schedule ID"]));
                            rec.TimeSchedule = GetTimeSchedule(SQLHelper.GetString(reader["Time Schedule ID"]));
                            lines.Add(rec);
                        }
                    }
                    connection.Close();
                }
            }
            return lines;
        }

        private VSDateSchedule GetDateSchedule(string id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Description],[Mondays],[Tuesdays],[Wednesdays],[Thursdays],[Fridays],[Saturdays],[Sundays],[Valid All Weekdays] " +
                            "FROM [" + navCompanyName + "LSC Date Schedule$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                            "WHERE [ID]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            VSDateSchedule rec = new VSDateSchedule()
                            {
                                Id = id,
                                Description = SQLHelper.GetString(reader["Description"]),
                                Mondays = SQLHelper.GetBool(reader["Mondays"]),
                                Tuesdays = SQLHelper.GetBool(reader["Mondays"]),
                                Wednesdays = SQLHelper.GetBool(reader["Mondays"]),
                                Thursdays = SQLHelper.GetBool(reader["Mondays"]),
                                Fridays = SQLHelper.GetBool(reader["Mondays"]),
                                Saturdays = SQLHelper.GetBool(reader["Mondays"]),
                                Sundays = SQLHelper.GetBool(reader["Mondays"]),
                                ValidAllWeekdays = SQLHelper.GetBool(reader["Mondays"])
                            };
                            rec.Lines = GetDateScheduleLines(id);
                            return rec;
                        }
                    }
                    connection.Close();
                }
            }
            return null;
        }

        private List<VSDateScheduleLine> GetDateScheduleLines(string id)
        {
            List<VSDateScheduleLine> lines = new List<VSDateScheduleLine>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Line No_],[Starting Date],[Ending Date],[Exclude] " +
                            "FROM [" + navCompanyName + "LSC Date Schedule Line$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                            "WHERE [Date Schedule ID]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lines.Add(new VSDateScheduleLine()
                            {
                                LineNo = SQLHelper.GetInt32(reader["Line No_"]),
                                StartingDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Starting Date"]), config.IsJson),
                                EndingDate = ConvertTo.SafeJsonDate(SQLHelper.GetDateTime(reader["Ending Date"]), config.IsJson),
                                Exclude = SQLHelper.GetBool(reader["Exclude"])
                            });
                        }
                    }
                    connection.Close();
                }
            }
            return lines;
        }

        private VSTimeSchedule GetTimeSchedule(string id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Description],[Schedule Type] " +
                            "FROM [" + navCompanyName + "LSC Time Schedule$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                            "WHERE [ID]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            VSTimeSchedule rec = new VSTimeSchedule()
                            {
                                Id = id,
                                Description = SQLHelper.GetString(reader["Description"]),
                                Type = (VSTimeScheduleType)SQLHelper.GetInt32(reader["Schedule Type"])
                            };
                            rec.Lines = GetTimeScheduleLines(id);
                            return rec;
                        }
                    }
                    connection.Close();
                }
            }
            return null;
        }

        private List<VSTimeScheduleLine> GetTimeScheduleLines(string id)
        {
            List<VSTimeScheduleLine> lines = new List<VSTimeScheduleLine>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [Period],[Time From],[Time To],[Time To Is Past Midnight],[Dining Duration Code],[Selected by Default],[Reservation Interval (Min_)] " +
                            "FROM [" + navCompanyName + "LSC Time Schedule Line$5ecfc871-5d82-43f1-9c54-59685e82318d] " +
                            "WHERE [Time Schedule ID]=@id";
                    command.Parameters.AddWithValue("@id", id);
                    TraceSqlCommand(command);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lines.Add(new VSTimeScheduleLine()
                            {
                                Period = SQLHelper.GetString(reader["Period"]),
                                TimeFrom = SQLHelper.GetDateTime(reader["Time From"]),
                                TimeTo = SQLHelper.GetDateTime(reader["Time To"]),
                                TimeToIsPastMidnight = SQLHelper.GetBool(reader["Time To Is Past Midnight"]),
                                DiningDurationCode = SQLHelper.GetString(reader["Dining Duration Code"]),
                                SelectedByDefault = SQLHelper.GetBool(reader["Selected by Default"]),
                                ReservationInterval = SQLHelper.GetInt32(reader["Reservation Interval (Min_)"])
                            });
                        }
                    }
                    connection.Close();
                }
            }
            return lines;
        }
    }
}
