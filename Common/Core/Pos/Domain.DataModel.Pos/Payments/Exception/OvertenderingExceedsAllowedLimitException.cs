namespace LSRetail.Omni.Domain.DataModel.Pos.Payments.Exception
{
    public sealed class OvertenderingExceedsAllowedLimitException : System.Exception
    {
        public OvertenderingExceedsAllowedLimitException(decimal maximumOvertenderAmount)
            : base()
        {
        }
    }
}
