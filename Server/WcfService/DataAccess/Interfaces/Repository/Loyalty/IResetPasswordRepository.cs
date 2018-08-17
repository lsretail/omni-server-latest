using System;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IResetPasswordRepository
    {
        void ResetPasswordSave(string resetCode, string contactId, string email);
        bool ResetPasswordExists(string resetCode, string contactId);
        void ResetPasswordDelete(string resetCode);
        DateTime ResetPasswordGetDateById(string id);
    }
}
