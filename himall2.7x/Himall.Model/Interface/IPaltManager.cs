using Himall.Model;
using System.Collections.Generic;

namespace Himall.Model
{
    public interface IPaltManager:IManager
    {
        List<AdminPrivilege> AdminPrivileges { set; get; }
    }
}
