using System;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup.Exceptions
{
    public class InvalidUsernamePasswordException : Exception
    {
		public InvalidUsernamePasswordException() : base()
		{

		}

		public InvalidUsernamePasswordException(string message) : base(message)
		{

		}
	}
}
