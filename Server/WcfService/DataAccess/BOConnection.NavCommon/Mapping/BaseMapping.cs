using System;

namespace LSOmni.DataAccess.BOConnection.NavCommon.Mapping
{
    public abstract class BaseMapping
    {
        protected bool IsJson = false;

        protected int LineNumberToNav(int lineNumber)
        {
            //multiply with 1000 for nav, if not already done!
            return (lineNumber >= 1000 ? lineNumber : lineNumber * 10000);
        }

        protected int LineNumberFromNav(int lineNumber)
        {
            //div by 1000 for nav, if not already done!
            if ((lineNumber % 10000) == 0)
            {
                return (lineNumber >= 10000 ? lineNumber / 10000 : lineNumber);
            }
            else
            {
                return lineNumber;
            }
        }
    }
}
