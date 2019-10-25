using LSRetail.Omni.Domain.DataModel.Base;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IValidationRepository
    {
        StatusCode ValidateSecurityToken(string securityToken, out string deviceId, out string contactId);
    }
}
