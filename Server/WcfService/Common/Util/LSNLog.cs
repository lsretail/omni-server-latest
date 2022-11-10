using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using LSRetail.Omni.Domain.DataModel.Base;
using NLog;

namespace LSOmni.Common.Util
{
	public class LSLogger
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public LSLogger()
		{
		}

		public void Info(string lskey, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Info(stackTrace.GetFrame(1).GetMethod().Name + " | Key:" + lskey + " | " + message, args);
		}

		public void Warn(Exception ex, string message)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Warn(ex, stackTrace.GetFrame(1).GetMethod().Name + " Key:Empty | " + message);
		}

		public void Warn(string lskey, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Warn(stackTrace.GetFrame(1).GetMethod().Name + " Key:" + lskey + " | " + message, args);
		}

		public void Warn(string lskey, Exception ex, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Warn(ex, stackTrace.GetFrame(1).GetMethod().Name + " Key:" + lskey + " | " + message, args);
		}

		public void Error(string lskey, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Error(stackTrace.GetFrame(1).GetMethod().Name + " Key:" + lskey + " | " + message, args);
		}

		public void Error(string lskey, Exception ex)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Error(ex, stackTrace.GetFrame(1).GetMethod().Name + " Key:" + lskey + " | ");
		}

		public void Error(string lskey, Exception ex, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Error(ex, stackTrace.GetFrame(1).GetMethod().Name + " Key:" + lskey + " | " + message, args);
		}

		public void Debug(string lskey, string message)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Debug(stackTrace.GetFrame(1).GetMethod().Name + " | Key:" + lskey + " | " + message);
		}

		public void Debug(string lskey, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Debug(stackTrace.GetFrame(1).GetMethod().Name + " | Key:" + lskey + " | " + message, args);
		}

		public void Debug(BOConfiguration lskey, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Debug(stackTrace.GetFrame(1).GetMethod().Name + " | Key:" + lskey.LSKey.Key + " | AppId:" + lskey.AppId + " | " + message, args);
		}

		public void Trace(string lskey, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Trace(stackTrace.GetFrame(1).GetMethod().Name + " Key:" + lskey + " | " + message, args);
		}

		public void Fatal(string lskey, string message)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Fatal(stackTrace.GetFrame(1).GetMethod().Name + " | Key:" + lskey + " | " + message);
		}

		public void Fatal(string lskey, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Fatal(stackTrace.GetFrame(1).GetMethod().Name + " | Key:" + lskey + " | " + message, args);
		}

		public void Fatal(string lskey, Exception ex, string message)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Fatal(ex, stackTrace.GetFrame(1).GetMethod().Name + " | Key:" + lskey + " | " + message);
		}

		public void Fatal(string lskey, Exception ex, string message, params object[] args)
		{
			StackTrace stackTrace = new StackTrace();
			logger.Fatal(ex, stackTrace.GetFrame(1).GetMethod().Name + " | Key:" + lskey + " | " + message, args);
		}

		public Statistics StatisticStartMain(BOConfiguration config, string url)
        {
			StackTrace stackTrace = new StackTrace();
			string name = stackTrace.GetFrame(1).GetMethod().Name;
			return new Statistics()
			{
				URL = url,
				Active = IsInfoEnabled,
				MainFunction = new StatisticData()
				{
					FunctionName = name,
					StartTime = DateTime.Now
				},
				SubFunctions = new List<StatisticData>()
			};
        }

		public void StatisticStartSub(bool webservice, ref Statistics data, out int index)
		{
			index = 0;
			if (data == null || data.Active == false)
				return;

			index = data.SubFunctions.Count;
			StackTrace stackTrace = new StackTrace();
			data.SubFunctions.Add(new StatisticData()
			{
				WebService = webservice,
				FunctionName = stackTrace.GetFrame(1).GetMethod().Name,
				StartTime = DateTime.Now
			});
		}

		public void StatisticEndSub(ref Statistics data, int index)
		{
			if (data == null || data.Active == false)
				return;

            data.SubFunctions[index].EndTime = DateTime.Now;
		}

		public void StatisticEndMain(Statistics data)
		{
			if (data == null || data.Active == false)
				return;

			long ticks = DateTime.Now.Ticks;
			data.MainFunction.EndTime = DateTime.Now;

			StringBuilder message = new StringBuilder();
			message.AppendLine($"[{ticks}][{data.URL}][{data.MainFunction.FunctionName}][{data.MainFunction.StartTime:s}][{data.MainFunction.EndTime - data.MainFunction.StartTime}]");
			foreach (StatisticData sub in data.SubFunctions)
            {
				message.AppendLine($"- [{ticks}][{data.URL}][{sub.FunctionName}][{(sub.WebService ? "WS" : "DB")}][{sub.StartTime:s}][{sub.EndTime - sub.StartTime}]");
			}

			logger.Info(message.ToString());
		}

		public bool IsInfoEnabled
		{
			get { return logger.IsInfoEnabled; }
		}

		public bool IsWarnEnabled
		{
			get { return logger.IsWarnEnabled; }
		}

		public bool IsDebugEnabled
		{
			get { return logger.IsDebugEnabled; }
		}

		public bool IsErrorEnabled
		{
			get { return logger.IsErrorEnabled; }
		}

		public bool IsTraceEnabled
		{
			get { return logger.IsTraceEnabled; }
		}

		public bool IsFatalEnabled
		{
			get { return logger.IsFatalEnabled; }
		}
	}

	public class Statistics
	{
		public string URL { get; set; }
		public bool Active { get; set; }
		public StatisticData MainFunction { get; set; }
		public List<StatisticData> SubFunctions { get; set; }
	}

	public class StatisticData
	{
		public string FunctionName { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool WebService { get; set; }
	}
}
