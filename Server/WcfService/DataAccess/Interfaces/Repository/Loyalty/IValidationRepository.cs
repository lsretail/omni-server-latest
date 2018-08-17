using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IValidationRepository
    {
        StatusCode ValidateSecurityToken(string securityToken, out string deviceId, out string contactId);

        bool ValidateAccount(string id, string contactId);
        bool ValidateContact(string id, string contactId);
        bool ValidateCard(string id, string contactId);

        bool ValidateContactUserName(string userName, string contactId);
        bool ValidateOneList(string id, string contactId);

    }
}
