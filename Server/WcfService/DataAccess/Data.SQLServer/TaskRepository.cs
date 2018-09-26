using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using NLog;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository;
using LSRetail.Omni.Domain.DataModel.Base.OmniTasks;
using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Dal
{
    class TaskRepository : BaseRepository, ITaskRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static object locker = new object();

        public OmniTask TaskSave(OmniTask task)
        {
            if (task == null)
                throw new ApplicationException("Save() task can not be null");

            logger.Log(LogLevel.Info, string.Format("SaveTask: Id:{0}", task.Id));
            lock (locker)
            {
                DateTime timenow = DateTime.Now;
                OmniTask oldtask = null;
                if (Validation.IsValidGuid(task.Id) == false)
                {
                    //if guid not found in db and not sent in then create a new one
                    task.Id = GuidHelper.NewGuidString();
                }
                else
                {
                    oldtask = TaskGetById(task.Id, false, null);

                    // check if another user is setting same status
                    if (oldtask != null)
                    {
                        switch (oldtask.Status)
                        {
                            case OmniTaskStatus.Assigned:
                                if ((task.Status == OmniTaskStatus.Assigned || task.Status == OmniTaskStatus.UnAssigned)
                                    && (task.ModifyUser != oldtask.AssignUser && task.ModifyUser != oldtask.RequestUser))
                                    throw new LSOmniServiceException(StatusCode.TaskStatusCannotChange,
                                        string.Format("Task [{0}] already assigned to user [{1}] which can only change the status", task.Id, oldtask.AssignUser));
                                break;
                            case OmniTaskStatus.Deleted:
                                if (task.Status != OmniTaskStatus.Deleted)
                                    throw new LSOmniServiceException(StatusCode.TaskStatusCannotChange,
                                        string.Format("Task [{0}] already deleted", task.Id));
                                break;
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    using (SqlTransaction trans = connection.BeginTransaction("Task"))
                    {
                        try
                        {
                            using (SqlCommand command = connection.CreateCommand())
                            {
                                command.Transaction = trans;
                                task.ModifyTime = timenow;
                                if (oldtask == null)
                                {
                                    task.CreateTime = timenow;
                                }
                                logger.Log(LogLevel.Info, task.ToString());

                                command.CommandText = "IF EXISTS(SELECT * FROM [Task] WHERE [Id]=@id) " +
                                                      "UPDATE [Task] SET " +
                                                      "[Status]=@f1,[Type]=@f2,[TransactionId]=@f3,[StoreId]=@f4," +
                                                      "[CreateTime]=@f5,[ModifyTime]=@f6,[ModifyUser]=@f7,[ModifyLocation]=@f8," +
                                                      "[RequestUser]=@f9,[RequestUserName]=@f10,[RequestLocation]=@f11," +
                                                      "[AssignUser]=@f12,[AssignUserName]=@f13,[AssignLocation]=@f14" +
                                                      " WHERE [Id]=@id" +
                                                      " ELSE " +
                                                      "INSERT INTO [Task] (" +
                                                      "[Id],[Status],[Type],[TransactionId],[StoreId],[CreateTime],[ModifyTime],[ModifyUser],[ModifyLocation]," +
                                                      "[RequestUser],[RequestUserName],[RequestLocation],[AssignUser],[AssignUserName],[AssignLocation]" +
                                                      ") VALUES (@id,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11,@f12,@f13,@f14)";

                                command.Parameters.AddWithValue("@f1", task.Status.ToString());
                                command.Parameters.AddWithValue("@f2", task.Type);
                                command.Parameters.AddWithValue("@f3", task.TransactionId);
                                command.Parameters.AddWithValue("@f4", task.StoreId);
                                command.Parameters.AddWithValue("@f5", (task.CreateTime == DateTime.MinValue ? MinDate : task.CreateTime));
                                command.Parameters.AddWithValue("@f6", (task.ModifyTime == DateTime.MinValue ? MinDate : task.ModifyTime));
                                command.Parameters.AddWithValue("@f7", task.ModifyUser);
                                command.Parameters.AddWithValue("@f8", task.ModifyLocation);
                                command.Parameters.AddWithValue("@f9", task.RequestUser);
                                command.Parameters.AddWithValue("@f10", task.RequestUserName);
                                command.Parameters.AddWithValue("@f11", task.RequestLocation);
                                command.Parameters.AddWithValue("@f12", task.AssignUser);
                                command.Parameters.AddWithValue("@f13", task.AssignUserName);
                                command.Parameters.AddWithValue("@f14", task.AssignLocation);
                                command.Parameters.AddWithValue("@id", task.Id);
                                TraceSqlCommand(command);
                                command.ExecuteNonQuery();

                                RegisterTaskLog(task, oldtask, connection, trans);

                                command.CommandText = "IF EXISTS(SELECT * FROM [TaskLine] WHERE [Id]=@id) " +
                                                      "UPDATE [TaskLine] SET " +
                                                      "[TaskId]=@f1,[LineNumber]=@f2,[Status]=@f3,[ModifyTime]=@f4,[ModifyUser]=@f5,[ModifyLocation]=@f6," +
                                                      "[ItemId]=@f7,[ItemName]=@f8,[VariantId]=@f9,[VariantName]=@f10,[Quantity]=@f11" +
                                                      " WHERE [Id]=@id" +
                                                      " ELSE " +
                                                      "INSERT INTO [TaskLine] (" +
                                                      "[Id],[TaskId],[LineNumber],[Status],[ModifyTime],[ModifyUser],[ModifyLocation]," +
                                                      "[ItemId],[ItemName],[VariantId],[VariantName],[Quantity]" +
                                                      ") VALUES (@id,@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10,@f11)";

                                command.Parameters.Clear();
                                command.Parameters.Add("@id", SqlDbType.NVarChar);
                                command.Parameters.Add("@f1", SqlDbType.NVarChar);
                                command.Parameters.Add("@f2", SqlDbType.Int);
                                command.Parameters.Add("@f3", SqlDbType.NVarChar);
                                command.Parameters.Add("@f4", SqlDbType.DateTime);
                                command.Parameters.Add("@f5", SqlDbType.NVarChar);
                                command.Parameters.Add("@f6", SqlDbType.NVarChar);
                                command.Parameters.Add("@f7", SqlDbType.NVarChar);
                                command.Parameters.Add("@f8", SqlDbType.NVarChar);
                                command.Parameters.Add("@f9", SqlDbType.NVarChar);
                                command.Parameters.Add("@f10", SqlDbType.NVarChar);
                                command.Parameters.Add("@f11", SqlDbType.Int);

                                foreach (OmniTaskLine line in task.Lines)
                                {
                                    line.ModifyTime = timenow;
                                    logger.Log(LogLevel.Info, line.ToString());

                                    OmniTaskLine oldline = TaskLineGetById(task.Id, line.ItemId, line.VariantId, connection, trans);
                                    if (oldline != null)
                                    {
                                        if (oldline.Status == OmniTaskLineStatus.Deleted && line.Status != oldline.Status)
                                            throw new LSOmniServiceException(StatusCode.TaskStatusCannotChange,
                                                string.Format("Line [{0}] has already been deleted", line.LineNumber));
                                    }
                                    else
                                    {
                                        line.Id = GuidHelper.NewGuidString();
                                        line.TaskId = task.Id;
                                        line.ModifyUser = task.ModifyUser;
                                        line.ModifyLocation = task.ModifyLocation;
                                    }

                                    command.Parameters["@f1"].Value = line.TaskId;
                                    command.Parameters["@f2"].Value = line.LineNumber;
                                    command.Parameters["@f3"].Value = line.Status.ToString();
                                    command.Parameters["@f4"].Value = line.ModifyTime;
                                    command.Parameters["@f5"].Value = line.ModifyUser;
                                    command.Parameters["@f6"].Value = line.ModifyLocation;
                                    command.Parameters["@f7"].Value = line.ItemId;
                                    command.Parameters["@f8"].Value = line.ItemDescription;
                                    command.Parameters["@f9"].Value = line.VariantId;
                                    command.Parameters["@f10"].Value = line.VariantDescription;
                                    command.Parameters["@f11"].Value = line.Quantity;
                                    command.Parameters["@id"].Value = line.Id;
                                    TraceSqlCommand(command);
                                    command.ExecuteNonQuery();

                                    RegisterTaskLogLine(line, oldline, connection, trans);
                                }
                                trans.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            logger.Log(LogLevel.Error, ex, "Task: " + task.Id);
                            throw;
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
                return task;
            }
        }

        public List<OmniTask> TaskGetByFilter(OmniTask filter, bool includelines, int maxTasks)
        {
            return TaskGetByFilter(filter, includelines, maxTasks, null);
        }

        public List<OmniTask> TaskGetByFilter(OmniTask filter, bool includelines, int maxTasks, SqlConnection db)
        {
            if (filter == null)
            {
                filter = new OmniTask();
                filter.ModifyTime = DateTime.MinValue;
            }

            bool connectdb = (db == null);
            lock (locker)
            {
                List<OmniTask> tasks = new List<OmniTask>();
                try
                {
                    if (connectdb)
                    {
                        db = new SqlConnection(sqlConnectionString);
                        db.Open();
                    }

                    using (SqlCommand command = db.CreateCommand())
                    {
                        string sql = "SELECT ";
                        if (maxTasks > 0)
                            sql += "TOP " + maxTasks.ToString();
                        sql += "[Id],[Status],[Type],[TransactionId],[CreateTime],[ModifyTime],[ModifyUser],[ModifyLocation],[StoreId]," +
                               "[RequestUser],[RequestUserName],[RequestLocation],[AssignUser],[AssignUserName],[AssignLocation] FROM [Task] ";

                        List<string> sqlwhere = new List<string>();
                        if (string.IsNullOrWhiteSpace(filter.Id) == false)
                        {
                            sqlwhere.Add("[Id]=@f1");
                            command.Parameters.AddWithValue("@f1", filter.Id);
                        }
                        if (string.IsNullOrWhiteSpace(filter.AssignLocation) == false)
                        {
                            sqlwhere.Add("[AssignLocation]=@f2");
                            command.Parameters.AddWithValue("@f2", filter.AssignLocation);
                        }
                        if (string.IsNullOrWhiteSpace(filter.AssignUser) == false)
                        {
                            sqlwhere.Add("[AssignUser]=@f3");
                            command.Parameters.AddWithValue("@f3", filter.AssignUser);
                        }
                        if (string.IsNullOrWhiteSpace(filter.RequestLocation) == false)
                        {
                            sqlwhere.Add("[RequestLocation]=@f4");
                            command.Parameters.AddWithValue("@f4", filter.RequestLocation);
                        }
                        if (string.IsNullOrWhiteSpace(filter.RequestUser) == false)
                        {
                            sqlwhere.Add("[RequestUser]=@f5");
                            command.Parameters.AddWithValue("@f5", filter.RequestUser);
                        }
                        if (filter.ModifyTime != DateTime.MinValue)
                        {
                            sqlwhere.Add("[ModifyTime]>@f6");
                            command.Parameters.AddWithValue("@f6", filter.ModifyTime);
                        }
                        if (string.IsNullOrWhiteSpace(filter.ModifyLocation) == false)
                        {
                            sqlwhere.Add("[ModifyLocation]=@f7");
                            command.Parameters.AddWithValue("@f7", filter.ModifyLocation);
                        }
                        if (string.IsNullOrWhiteSpace(filter.StoreId) == false)
                        {
                            sqlwhere.Add("[StoreId]=@f8");
                            command.Parameters.AddWithValue("@f8", filter.StoreId);
                        }
                        if (string.IsNullOrWhiteSpace(filter.ModifyUser) == false)
                        {
                            sqlwhere.Add("[ModifyUser]=@f9");
                            command.Parameters.AddWithValue("@f9", filter.ModifyUser);
                        }
                        if (filter.Status != OmniTaskStatus.None)
                        {
                            sqlwhere.Add(string.Format("[Status] IN ({0})", EnumHelper.GetEnumSQLInValue(filter.Status)));
                        }
                        if (string.IsNullOrWhiteSpace(filter.Type) == false)
                        {
                            sqlwhere.Add("[Type]=@f10");
                            command.Parameters.AddWithValue("@f10", filter.Type);
                        }

                        if (sqlwhere.Count > 0)
                        {
                            sql += " WHERE ";
                            bool first = true;
                            foreach (string w in sqlwhere)
                            {
                                if (first == false)
                                    sql += " AND ";
                                sql += w;
                                first = false;
                            }
                        }

                        if (maxTasks > 0)
                            sql += " ORDER BY [ModifyTime] DESC";

                        logger.Log(LogLevel.Info, string.Format("GetTask: Id:{0} Status:{1}",
                            filter.Id, filter.Status));

                        command.CommandText = sql;
                        TraceSqlCommand(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OmniTask task = new OmniTask();
                                task.Id = SQLHelper.GetString(reader["Id"]);
                                task.Status = (OmniTaskStatus)EnumHelper.StringToEnum(typeof(OmniTaskStatus), SQLHelper.GetString(reader["Status"]));
                                task.Type = SQLHelper.GetString(reader["Type"]);
                                task.TransactionId = SQLHelper.GetString(reader["TransactionId"]);
                                task.CreateTime = SQLHelper.GetDateTime(reader["CreateTime"]);
                                task.ModifyTime = SQLHelper.GetDateTime(reader["ModifyTime"]);
                                task.ModifyLocation = SQLHelper.GetString(reader["ModifyLocation"]);
                                task.ModifyUser = SQLHelper.GetString(reader["ModifyUser"]);
                                task.StoreId = SQLHelper.GetString(reader["StoreId"]);
                                task.RequestLocation = SQLHelper.GetString(reader["RequestLocation"]);
                                task.RequestUser = SQLHelper.GetString(reader["RequestUser"]);
                                task.RequestUserName = SQLHelper.GetString(reader["RequestUserName"]);
                                task.AssignLocation = SQLHelper.GetString(reader["AssignLocation"]);
                                task.AssignUser = SQLHelper.GetString(reader["AssignUser"]);
                                task.AssignUserName = SQLHelper.GetString(reader["AssignUserName"]);
                                if (includelines)
                                {
                                    task.Lines = TaskLineGetByTaskId(task.Id, filter.Lines, db, null);
                                }
                                tasks.Add(task);
                            }
                            reader.Close();
                        }
                    }
                }

                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex);
                    throw;
                }
                if (connectdb)
                {
                    db.Close();
                    db.Dispose();
                }
                return tasks;
            }
        }

        public List<OmniTaskLine> TaskLineGetByFilter(OmniTaskLine filter)
        {
            return TaskLineGetByFilter(filter, null, null);
        }

        public List<OmniTaskLine> TaskLineGetByFilter(OmniTaskLine filter, SqlConnection db, SqlTransaction trans)
        {
            if (filter == null)
                filter = new OmniTaskLine();

            bool connectdb = (db == null);
            List<OmniTaskLine> lines = new List<OmniTaskLine>();
            try
            {
                if (connectdb)
                {
                    db = new SqlConnection(sqlConnectionString);
                    db.Open();
                }

                using (SqlCommand command = db.CreateCommand())
                {
                    if (trans != null)
                        command.Transaction = trans;

                    string sql = "SELECT [Id],[TaskId],[LineNumber],[Status],[ModifyTime],[ModifyUser],[ModifyLocation]," +
                                 "[ItemId],[ItemName],[VariantId],[VariantName],[Quantity] FROM [TaskLine]";

                    List<string> sqlwhere = new List<string>();
                    if (string.IsNullOrWhiteSpace(filter.Id) == false)
                    {
                        sqlwhere.Add("[Id]=@f1");
                        command.Parameters.AddWithValue("@f1", filter.Id);
                    }
                    if (string.IsNullOrWhiteSpace(filter.ItemId) == false)
                    {
                        sqlwhere.Add("[ItemId]=@f2");
                        command.Parameters.AddWithValue("@f2", filter.ItemId);
                    }
                    if (filter.ModifyTime != DateTime.MinValue)
                    {
                        sqlwhere.Add("[ModifyTime]>@f3");
                        command.Parameters.AddWithValue("@f3", filter.ModifyTime);
                    }
                    if (string.IsNullOrWhiteSpace(filter.ModifyLocation) == false)
                    {
                        sqlwhere.Add("[ModifyLocation]=@f4");
                        command.Parameters.AddWithValue("@f4", filter.ModifyLocation);
                    }
                    if (string.IsNullOrWhiteSpace(filter.ModifyUser) == false)
                    {
                        sqlwhere.Add("[ModifyUser]=@f5");
                        command.Parameters.AddWithValue("@f5", filter.ModifyUser);
                    }
                    if (filter.Status != OmniTaskLineStatus.None)
                    {
                        sqlwhere.Add(string.Format("Status IN ({0})", EnumHelper.GetEnumSQLInValue(filter.Status)));
                    }
                    if (string.IsNullOrWhiteSpace(filter.TaskId) == false)
                    {
                        sqlwhere.Add("[TaskId]=@f6");
                        command.Parameters.AddWithValue("@f6", filter.TaskId);
                    }
                    if (string.IsNullOrWhiteSpace(filter.VariantId) == false)
                    {
                        sqlwhere.Add("[VariantId]=@f7");
                        command.Parameters.AddWithValue("@f7", filter.VariantId);
                    }

                    if (sqlwhere.Count > 0)
                    {
                        sql += " WHERE ";
                        bool first = true;
                        foreach (string w in sqlwhere)
                        {
                            if (first == false)
                                sql += " AND ";
                            sql += w;
                            first = false;
                        }
                    }

                    logger.Log(LogLevel.Info, string.Format("GetTaskLine: Id:{0} TaskId:{1} Status:{2}",
                        filter.Id, filter.TaskId, filter.Status));

                    command.CommandText = sql;
                    TraceSqlCommand(command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OmniTaskLine line = new OmniTaskLine();
                            line.Id = SQLHelper.GetString(reader["Id"]);
                            line.TaskId = SQLHelper.GetString(reader["TaskId"]);
                            line.LineNumber = SQLHelper.GetInt32(reader["LineNumber"]);
                            line.Status = (OmniTaskLineStatus)EnumHelper.StringToEnum(typeof(OmniTaskLineStatus), SQLHelper.GetString(reader["Status"]));
                            line.ModifyTime = SQLHelper.GetDateTime(reader["ModifyTime"]);
                            line.ModifyLocation = SQLHelper.GetString(reader["ModifyLocation"]);
                            line.ModifyUser = SQLHelper.GetString(reader["ModifyUser"]);
                            line.ItemId = SQLHelper.GetString(reader["ItemId"]);
                            line.ItemDescription = SQLHelper.GetString(reader["ItemName"]);
                            line.VariantId = SQLHelper.GetString(reader["VariantId"]);
                            line.VariantDescription = SQLHelper.GetString(reader["VariantName"]);
                            line.Quantity = SQLHelper.GetInt32(reader["Quantity"]);
                            lines.Add(line);
                        }
                        reader.Close();
                    }
                }
                if (connectdb)
                {
                    db.Close();
                    db.Dispose();
                }
                return lines;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
                throw;
            }
        }

        public OmniTask TaskGetById(string id, bool includelines, SqlConnection db)
        {
            OmniTask filter = new OmniTask();
            filter.Id = id;
            filter.ModifyTime = DateTime.MinValue;
            List<OmniTask> tasks = TaskGetByFilter(filter, includelines, 0, db);
            if (tasks.Count == 1)
                return tasks[0];
            return null;
        }

        public OmniTaskLine TaskLineGetById(string taskid, string itemid, string variant, SqlConnection db, SqlTransaction trans)
        {
            OmniTaskLine filter = new OmniTaskLine();
            filter.TaskId = taskid;
            filter.ItemId = itemid;
            filter.VariantId = variant;
            filter.ModifyTime = DateTime.MinValue;
            List<OmniTaskLine> lines = TaskLineGetByFilter(filter, db, trans);
            if (lines.Count == 1)
                return lines[0];
            return null;
        }

        public List<OmniTaskLine> TaskLineGetByTaskId(string taskid, List<OmniTaskLine> lines, SqlConnection db, SqlTransaction trans)
        {
            OmniTaskLine filter = new OmniTaskLine();
            if (lines != null && lines.Count == 1)
            {
                filter = lines[0];
            }
            filter.TaskId = taskid;
            filter.ModifyTime = DateTime.MinValue;
            return TaskLineGetByFilter(filter, db, trans);
        }

        public void RegisterTaskLog(OmniTask newtask, OmniTask oldtask, SqlConnection db, SqlTransaction trans)
        {
            if (oldtask != null)
            {
                if (newtask.IsEquals(oldtask))
                    return;
            }

            try
            {
                using (SqlCommand command = db.CreateCommand())
                {
                    command.Transaction = trans;
                    command.CommandText = "INSERT INTO [TaskLog] (" +
                            "[TaskId],[ModifyTime],[ModifyUser],[ModifyLocation],[StatusFrom],[StatusTo]," +
                            "[RequestUserFrom],[RequestUserTo],[AssignUserFrom],[AssignUserTo]" +
                            ") VALUES (@f1,@f2,@f3,@f4,@f5,@f6,@f7,@f8,@f9,@f10)";

                    command.Parameters.AddWithValue("@f1", newtask.Id);
                    command.Parameters.AddWithValue("@f2", newtask.ModifyTime);
                    command.Parameters.AddWithValue("@f3", newtask.ModifyUser);
                    command.Parameters.AddWithValue("@f4", newtask.ModifyLocation);
                    command.Parameters.AddWithValue("@f6", newtask.Status.ToString());
                    command.Parameters.AddWithValue("@f8", newtask.RequestUser);
                    command.Parameters.AddWithValue("@f10", newtask.AssignUser);
                    command.Parameters.AddWithValue("@f5", (oldtask != null) ? oldtask.Status.ToString() : string.Empty);
                    command.Parameters.AddWithValue("@f7", (oldtask != null) ? oldtask.RequestUser : string.Empty);
                    command.Parameters.AddWithValue("@f9", (oldtask != null) ? oldtask.AssignUser : string.Empty);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        public void RegisterTaskLogLine(OmniTaskLine newline, OmniTaskLine oldline, SqlConnection db, SqlTransaction trans)
        {
            if (oldline != null)
            {
                if (newline.IsEquals(oldline))
                    return;
            }

            try
            {
                using (SqlCommand command = db.CreateCommand())
                {
                    command.Transaction = trans;
                    command.CommandText = "INSERT INTO [TaskLogLine] (" +
                            "[TaskLineId],[ModifyTime],[ModifyUser],[ModifyLocation],[StatusFrom],[StatusTo]" +
                            ") VALUES (@f1,@f2,@f3,@f4,@f5,@f6)";

                    command.Parameters.AddWithValue("@f1", newline.Id);
                    command.Parameters.AddWithValue("@f2", newline.ModifyTime);
                    command.Parameters.AddWithValue("@f3", newline.ModifyUser);
                    command.Parameters.AddWithValue("@f4", newline.ModifyLocation);
                    command.Parameters.AddWithValue("@f6", newline.Status.ToString());
                    command.Parameters.AddWithValue("@f5", (oldline != null) ? oldline.Status.ToString() : string.Empty);
                    TraceSqlCommand(command);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }
    }
}
