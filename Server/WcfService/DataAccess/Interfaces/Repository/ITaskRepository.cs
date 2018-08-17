using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base.OmniTasks;

namespace LSOmni.DataAccess.Interface.Repository
{
    public interface ITaskRepository
    {
        OmniTask TaskSave(OmniTask task);
        List<OmniTask> TaskGetByFilter(OmniTask filter, bool includelines, int maxtasks);
        List<OmniTaskLine> TaskLineGetByFilter(OmniTaskLine filter);
    }
}
