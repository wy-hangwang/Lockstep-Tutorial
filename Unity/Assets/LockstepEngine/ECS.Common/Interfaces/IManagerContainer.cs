using System.Collections.Generic;
using System.Linq;

namespace Lockstep.Game
{
    public interface IManagerContainer 
    {
        IList<BaseService> AllMgrs { get; }
        T GetManager<T>() where T : BaseService;
        void RegisterManager(BaseService baseService);
    }
}