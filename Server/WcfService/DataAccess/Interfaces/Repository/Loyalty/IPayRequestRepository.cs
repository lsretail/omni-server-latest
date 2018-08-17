using System;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IPayRequestRepository
    {
        Guid NewRequest(string orderId);
        bool CheckRequest(string orderId);
    }
}
