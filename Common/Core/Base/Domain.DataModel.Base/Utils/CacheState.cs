
namespace LSRetail.Omni.Domain.DataModel.Base.Utils
{
    public enum CacheState
    {
        NotExist = 0,        //oject does not exist
        Exists = 1,          //oject found and has not expired
        ExistsButExpired = 2,      //oject found but has expired
    }
}
 