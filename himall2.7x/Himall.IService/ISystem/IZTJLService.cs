using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices
{
    public interface IZTJLService : IService
    {
        void ImportOrder( ZTJLOrderModel model );
    }
}
