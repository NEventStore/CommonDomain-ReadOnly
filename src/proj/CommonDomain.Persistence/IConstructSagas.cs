using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonDomain.Persistence
{
    public interface IConstructSagas
    {
        TSaga Build<TSaga>(Guid id) where TSaga : ISaga;
    }
}
