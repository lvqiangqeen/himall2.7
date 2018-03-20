using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Himall.Search.Service.Model;
using System.Data;
using Himall.Core;
using System.Text;

namespace Himall.Search.Service.Data
{
    public class IndexDao
    {
        /// <summary>
        /// 分词
        /// </summary>
        private Segment _segment;
        public IndexDao(Segment segment)
        {
            _segment = segment;
        }

        /// <summary>
        /// 获取已存在的词语
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        private List<SegmentWords> GetExistSegmentWords(List<string> words, string connection = "")
        {
            if (words.Count == 0) return null;
            MySqlConnection conn = Connection.GetConnection(connection);
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT * FROM Himall_SegmentWords WHERE Word IN(SELECT ColWord FROM (");
            MySqlCommand cmd = new MySqlCommand();
            for (int i = 0; i < words.Count; i++)
            {
                sql.AppendFormat("SELECT @word{0} AS ColWord {1} ", i.ToString(), (i == words.Count - 1 ? string.Empty : " UNION ALL "));
                cmd.Parameters.Add(new MySqlParameter("@word" + i.ToString(), MySqlDbType.String));
                cmd.Parameters[i].Value = words[i];
            }
            sql.Append(") AS _TempTable)");
            cmd.CommandText = sql.ToString();
            cmd.Connection = conn;
            if (conn.State != ConnectionState.Open)
                conn.Open();
            MySqlDataReader reader = cmd.ExecuteReader();
            List<SegmentWords> lst = new List<SegmentWords>();
            while (reader.Read())
            {
                lst.Add(new SegmentWords()
                {
                    Id = (long)reader["Id"],
                    Word = reader["Word"].ToString()
                });
            }
            reader.Close();
            reader.Dispose();
            conn.Close();
            conn.Dispose();

            return lst;
        }

        /// <summary>
        /// 添加词语
        /// </summary>
        /// <param name="words"></param>
        private void AddSegmentWords(List<string> words, string connection = "")
        {
            if (words.Count == 0) return;
            MySqlConnection conn = Connection.GetConnection(connection);
            StringBuilder sql = new StringBuilder();

            //添加新词语
            sql.Append("INSERT INTO HIMALL_SEGMENTWORDS(Word) SELECT ColWord FROM (");
            MySqlCommand cmd = new MySqlCommand();
            for (int i = 0; i < words.Count; i++)
            {
                sql.AppendFormat("SELECT @word{0} AS ColWord {1} ", i.ToString(), (i == words.Count - 1 ? string.Empty : " UNION ALL "));
                cmd.Parameters.Add(new MySqlParameter("@word" + i.ToString(), MySqlDbType.String));
                cmd.Parameters[i].Value = words[i];
            }
            sql.Append(") AS _TempTable ");
            cmd.CommandText = sql.ToString();

            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
        }

        /// <summary>
        /// 添加商品与分词结果关系
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="words"></param>
        private void AddRefSegmentWords(long productId, List<string> words, string connection = "")
        {
            if (words.Count == 0) return;
            MySqlConnection conn = Connection.GetConnection(connection);
            StringBuilder sql = new StringBuilder();

            sql.Append("DELETE FROM Himall_ProductWords WHERE ProductId=@productId;");
            sql.Append("INSERT INTO Himall_ProductWords(WordId,ProductId) SELECT Id,@productId FROM Himall_SegmentWords WHERE Word IN(SELECT ColWord FROM (");
            MySqlCommand cmd = new MySqlCommand();
            for (int i = 0; i < words.Count; i++)
            {
                sql.AppendFormat("SELECT @word{0} AS ColWord {1} ", i.ToString(), (i == words.Count - 1 ? string.Empty : " UNION ALL "));
                cmd.Parameters.Add(new MySqlParameter("@word" + i.ToString(), MySqlDbType.String));
                cmd.Parameters[i].Value = words[i];
            }
            sql.Append(") AS _TempTable)");
            cmd.Parameters.Add(new MySqlParameter("@productId", MySqlDbType.Int64));
            cmd.Parameters[cmd.Parameters.Count - 1].Value = productId;

            cmd.CommandText = sql.ToString();

            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.ExecuteNonQuery();


            conn.Close();
            conn.Dispose();

        }

        /// <summary>
        /// 删除指定商品Id的索引关系
        /// </summary>
        /// <param name="productId"></param>
        public void DeleteSegmentWords(long productId, string connection = "")
        {
            MySqlConnection conn = Connection.GetConnection(connection);
            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM Himall_ProductWords WHERE ProductId=@ProductId;");
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = sql.ToString();
            cmd.Parameters.Add(new MySqlParameter("@ProductId", MySqlDbType.Int64));
            cmd.Parameters[0].Value = productId;
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
        }

        public void InitSegmentWords(string connection = "")
        {
            MySqlConnection conn = Connection.GetConnection(connection);
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT Id,ProductName FROM Himall_Products";
            cmd.Connection = conn;
            if (conn.State != ConnectionState.Open)
                conn.Open();
            MySqlDataReader reader = cmd.ExecuteReader();
            List<InitProductModel> list = new List<InitProductModel>();
            while (reader.Read())
            {
                list.Add(new InitProductModel()
                {
                    Id = long.Parse(reader["Id"].ToString()),
                    ProductName = reader["ProductName"].ToString()
                });
            }
            reader.Close();
            reader.Dispose();
            conn.Close();
            conn.Dispose();

            foreach(InitProductModel model in list)
            {
                List<string> words = _segment.DoSegment(model.ProductName);
                InsertSegmentWords(model.Id, words, connection);
            }
        }
        /// <summary>
        /// 商品分词
        /// </summary>
        /// <param name="id"></param>
        /// <param name="words"></param>
        /// <param name="trans"></param>
        public void InsertSegmentWords(long id, List<string> words, string connection = "")
        {
            try
            {
                if (words.Count <= 0) return;

                //保留原始分词结果
                List<string> _temp = new List<string>();
                _temp.AddRange(words);

                //获取将要新增但已存在的词语对象
                List<SegmentWords> lstExists = GetExistSegmentWords(words, connection);
                foreach (SegmentWords word in lstExists)
                    words.Remove(word.Word);

                //添加数据库中不存在的词语
                AddSegmentWords(words, connection);

                //添加商品与词语关联
                AddRefSegmentWords(id, _temp, connection);
            }
            catch (Exception e)
            {
                Log.Error(string.Format("创建商品【id={0}】索引失败：{1}", id.ToString(), e.Message));
            }
        }
    }

    public class InitProductModel
    {
        public long Id { get; set; }
        public string ProductName { get; set; }
    }
}
