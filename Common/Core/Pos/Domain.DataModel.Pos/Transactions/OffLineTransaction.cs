
namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions
{
    public class OffLineTransaction
    {
        public RetailTransaction RetailTransaction { get; set; }
        public bool Posted { get; private set; }
        public bool EmailSent { get; private set; }
        public string Email { get; set; }

        public OffLineTransaction(RetailTransaction retailTransaction)
        {
            Email = string.Empty;
            this.RetailTransaction = retailTransaction;
        }

        public OffLineTransaction(RetailTransaction retailTransaction, bool posted, bool emailSent)
            : this(retailTransaction)
        {
            this.Posted = posted;
            this.EmailSent = emailSent;
        }
    }
}
