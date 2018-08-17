using System;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    // Those interfaces are deliberately kept empty.

    public interface IServiceContext
    {
        
    }

    public interface ILocalServiceContext : IServiceContext, IDisposable
    {

    }
}
