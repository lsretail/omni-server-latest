using System;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    public class LSOmniException : Exception
    {
        // public members  
        public new string Message { get; set; }
        public StatusCode StatusCode { get; set; }

        public LSOmniException()
        { }

        public LSOmniException(StatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }

        public LSOmniException(StatusCode statusCode, string message, Exception innerException = null)
            : base(string.Format("StatusCode:[{0}] Error: {1}", statusCode.ToString(), message), innerException)
        {
            this.StatusCode = statusCode;
            this.Message = message;
        }

        public LSOmniException(StatusCode statusCode, Exception innerException = null)
            : base($"StatusCode:[{statusCode.ToString()}]", innerException)
        {
            this.StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Simple exeption to store the statuses returned to webservices.
    /// </summary>
    public class LSOmniServiceException : Exception
    {
        // private members 
        public new string Message { get; set; }
        public StatusCode StatusCode { get; set; }

        /// <summary>
        /// Standard default Constructor
        /// </summary>
        public LSOmniServiceException()
        {
        }

        /// <summary>
        /// Constructor with parameters 
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param> 
        public LSOmniServiceException(StatusCode statusCode, string message, Exception innerException = null)
            : base(string.Format("Error:{0}:{1}", statusCode.ToString(), message), innerException)
        {
            this.StatusCode = statusCode;
            this.Message = message;
        }

        public string GetMessage()
        {
            string msg = Message.Replace("\"", "'");
            if (InnerException != null)
            {
                msg += " IEx1:" + InnerException.Message.Replace("\"", "'");
                if (InnerException.InnerException != null)
                {
                    msg += " IEx2:" + InnerException.InnerException.Message.Replace("\"", "'");
                    if (InnerException.InnerException.InnerException != null)
                    {
                        msg += " IEx3:" + InnerException.InnerException.InnerException.Message.Replace("\"", "'");
                    }
                }
            }
            return msg;
        }
    }
}
