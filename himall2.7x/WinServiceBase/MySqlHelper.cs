using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace WinServiceBase
{
    public static class MySqlHelper
    {
        private static readonly string mysqlconn = ConfigurationManager.AppSettings["ConnString"];

        public static MySqlConnection OpenConnection()
        {
            var connection = new MySqlConnection(mysqlconn);
            connection.Open();
            return connection;
        }
    }
}
