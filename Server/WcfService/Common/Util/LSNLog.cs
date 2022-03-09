using System;
using System.Diagnostics;
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
}
