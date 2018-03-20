using System;
using System.Security.Cryptography;
using System.Text;

namespace Himall.PaymentPlugin
{
    /// <summary>
    /// 签名
    /// </summary>
    public class Sign
    {
        /// <summary>
        /// 签名字符串
        /// </summary>
        /// <param name="prestr">需要签名的字符串</param>
        /// <param name="key">密钥</param>
        /// <param name="_input_charset">编码格式</param>
        /// <returns>签名结果</returns>
        public static string MD5(string prestr, string key, string _input_charset)
        {
            StringBuilder sb = new StringBuilder(32);

            prestr = prestr + key;

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(_input_charset).GetBytes(prestr));
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }


        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="prestr">需要签名的字符串</param>
        /// <param name="sign">签名结果</param>
        /// <param name="key">密钥</param>
        /// <param name="_input_charset">编码格式</param>
        /// <returns>验证结果</returns>
        public static bool VerifyMD5(string prestr, string sign, string key, string _input_charset)
        {
            string mysign = MD5(prestr, key, _input_charset);
            if (mysign == sign)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public static string SHA1(string str)
        {
            //建立SHA1对象
            SHA1 sha = new SHA1CryptoServiceProvider();
            //将mystr转换成byte[] 
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] dataToHash = enc.GetBytes(str);
            //Hash运算
            byte[] dataHashed = sha.ComputeHash(dataToHash);
            //将运算结果转换成string
            string hash = BitConverter.ToString(dataHashed).Replace("-", "");
            return hash;
        }
    }
}
