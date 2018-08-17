using System;

namespace LSRetail.Omni.Domain.DataModel.Pos.DTO
{
    /// <summary>
    /// Exception to carry information to the UI layer on what error should be displayed to the user 
    /// </summary>
    public class UIException : System.Exception
    {
        public UIErrorMsgCode ErrorMsgCode { get; set; }

        public UIException()
        {
        }

        public UIException(UIErrorMsgCode errorMsgCode, Exception innerException = null)
            : base(string.Format("UIException, error message code {0}", errorMsgCode.ToString()), innerException)
        {
            this.ErrorMsgCode = errorMsgCode;
        }
    }
}
