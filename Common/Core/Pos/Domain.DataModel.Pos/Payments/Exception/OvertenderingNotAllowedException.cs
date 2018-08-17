using LSRetail.Omni.Domain.DataModel.Pos.TenderTypes;

namespace LSRetail.Omni.Domain.DataModel.Pos.Payments.Exception
{
    public sealed class OvertenderingNotAllowedException : System.Exception
    {
        public OvertenderingNotAllowedException(TenderType tenderType)
            : base()
        {
        }
    }
}
