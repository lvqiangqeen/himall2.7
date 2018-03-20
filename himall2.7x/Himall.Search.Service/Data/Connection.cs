using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Search.Service.Data
{
    public class Connection
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["mysql"].ConnectionString;
        public static MySqlConnection GetConnection(string _connection = "")
        {

            if (string.IsNullOrEmpty(connectionString) && string.IsNullOrEmpty(_connection))
                throw new ApplicationException("配置文件中未找到商品搜索的有效配置");
            return new MySqlConnection(!string.IsNullOrEmpty(_connection) ? _connection : connectionString) ;
        }

    }
}
