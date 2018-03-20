using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Data;
using System.Web.Configuration;
using System.IO;
using System.Data.SqlClient;
using System.Data.Common;
using System.Text;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Xml;

namespace Himall.Web.Areas.Web.Controllers
{
    public class InstallerController : Controller
    {
        private IList<string> errorMsgs = null;
        private int usernameMinLength = 3;
        private int usernameMaxLength = 20;
        private string usernameRegex = "[\u4e00-\u9fa5a-zA-Z0-9]+[\u4e00-\u9fa5_a-zA-Z0-9]*";
        private int passwordMaxLength = 16;
        private int passwordMinLength = 6;


        string _dbServer,                       // 数据库服务器
         _dbPort,
         _dbName,                        // 数据库名称
         _dbLoginName,             // 数据库登录名
         _dbPwd,                          // 数据库密码
         _siteName,                     //  网站名称
         _siteAdminName,         //  管理员名称
         _sitePwd,                       //  管理员密码
         _sitePwd2,                       //  管理员密码
         _shopName,                 //  官方自营店名称
         _shopAdminName,     //  官方自营店管理员名称
         _shopPwd,                      // 官方自营店密码
        _shopPwd2;                      // 官方自营店密码

        private bool IsInstalled()
        {
            var t = ConfigurationManager.AppSettings["IsInstalled"];
            return null == t || bool.Parse(t);
        }

        string GetPasswrodWithTwiceEncode(string password, string salt)
        {
            string encryptedPassword = Core.Helper.SecureHelper.MD5(password);//一次MD5加密
            string encryptedWithSaltPassword = Core.Helper.SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            return encryptedWithSaltPassword;
        }

        // GET: Web/Installer
        public ActionResult Agreement()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SaveConfiguration(
            string dbServer,                       // 数据库服务器
                                                   //string dbPort,                          //数据库端口
            string dbName,                        // 数据库名称
            string dbLoginName,             // 数据库登录名
            string dbPwd,                          // 数据库密码
            string siteName,                     //  网站名称
            string siteAdminName,         //  管理员名称
            string sitePwd,                       //  管理员密码
            string sitePwd2,                       //  管理员密码
            string shopName,                 //  官方自营店名称
            string shopAdminName,     //  官方自营店管理员名称
            string shopPwd,                      // 官方自营店密码
            string shopPwd2,                      // 官方自营店密码
            int installData                             //是否安装演示数据
            )
        {
            if (!IsInstalled())
            {

                _dbServer = dbServer;
                _dbPort = "";
                _dbName = dbName;
                _dbLoginName = dbLoginName;
                _dbPwd = dbPwd;
                _siteName = siteName;
                _siteAdminName = siteAdminName;
                _sitePwd = sitePwd;
                _sitePwd2 = sitePwd2;
                _shopName = shopName;
                _shopAdminName = shopAdminName;
                _shopPwd = shopPwd;
                _shopPwd2 = shopPwd2;


                string errorMsg = string.Empty;
                // 检查用户信息
                if (!ValidateUser(out errorMsg))
                {
                    return Json(new
                    {
                        successful = true,
                        errorMsg = errorMsg
                    }, JsonRequestBehavior.AllowGet);
                }

                if (!CreateDtabase(out errorMsg))
                {
                    return Json(new
                    {
                        successful = true,
                        errorMsg = errorMsg
                    }, JsonRequestBehavior.AllowGet);
                }

                // 如果还没有运行过安装测试，则先运行安装测试
                if (!ExecuteTest())
                {
                    return Json(new
                    {
                        successful = true,
                        errorMsg = "数据库链接信息有误"
                    }, JsonRequestBehavior.AllowGet);
                }

                if (!TestPermission())
                {
                    return Json(new
                    {
                        successful = true,
                        errorMsg = "WEB目录读写权限不够."
                    }, JsonRequestBehavior.AllowGet);
                }


                // 创建数据库架构
                if (!CreateDataSchema(out errorMsg))
                {
                    return Json(new
                    {
                        successful = true,
                        errorMsg = errorMsg
                    }, JsonRequestBehavior.AllowGet);
                }

                // 创建管理员账号
                if (!CreateAdministrator(out errorMsg))
                {
                    return Json(new
                    {
                        successful = true,
                        errorMsg = errorMsg
                    }, JsonRequestBehavior.AllowGet);
                }

                // 添加演示数据
                if (installData == 1)
                {//演示数据里已包含初始化数据，不需要单独再初始化
                    if (!AddDemoData(out errorMsg))
                        return Json(new
                        {
                            successful = true,
                            errorMsg = "添加演示数据失败"
                        }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // 添加初始化数据
                    if (!AddInitData(out errorMsg))
                    {
                        return Json(new
                        {
                            successful = true,
                            errorMsg = errorMsg
                        }, JsonRequestBehavior.AllowGet);
                    }
                    if (!UpdateSliderImage(out errorMsg))
                    {
                        return Json(new
                        {
                            successful = true,
                            errorMsg = errorMsg
                        }, JsonRequestBehavior.AllowGet);
                    }
                }



                // 保存web.config文件
                if (!SaveConfig(out errorMsg))
                {
                    return Json(new
                    {
                        successful = true,
                        errorMsg = errorMsg
                    }, JsonRequestBehavior.AllowGet);
                }
                //服务配置
                SetServicesConfig(dbName);
                return Json(new
                {
                    successful = true,
                    msg = "安装成功",
                    status = 1
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    successful = true,
                    msg = "软件已经安装,不需要重新安装.",
                    status = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 设置服务名称
        /// </summary>
        /// <param name="ServiceSuffix"></param>
        private void SetServicesConfig(string ServiceSuffix)
        {
            try
            {
                //初始服务所在根目录
                Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                var servicePathKey = config.AppSettings.Settings["ServicePath"];
                var serviceMainPath = @"系统服务";
                if (servicePathKey != null && servicePathKey.Value != "")
                {
                    serviceMainPath = servicePathKey.Value;
                }
                string siteMapPath = Server.MapPath(Request.ApplicationPath);
                serviceMainPath = Path.Combine(siteMapPath, serviceMainPath);
                //根据配置文件里定义的服务路径，设置服务的服务名
                var serviceMainConfigFile = Path.Combine(siteMapPath, "ServicePathSetting.xml");
                if (System.IO.File.Exists(serviceMainConfigFile))
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(serviceMainConfigFile);
                    var nodelist = xml.SelectNodes("ServiceConfig/Service");
                    string configFileFullPath = string.Empty;
                    XmlNode serviceConfigFileNode;
                    //循环所有服务
                    for (var i = 0; i < nodelist.Count; i++)
                    {
                        serviceConfigFileNode = nodelist.Item(i);
                        var configfileNodes = serviceConfigFileNode.SelectNodes("configfile");
                        if (configfileNodes.Count > 0)
                        {
                            var configPath = configfileNodes.Item(0).InnerText;
                            configFileFullPath = Path.Combine(serviceMainPath, configPath);
                            //设置服务名后辍
                            SaveServiceName(configFileFullPath, ServiceSuffix);
                        }
                        var connconfigfileNodes = serviceConfigFileNode.SelectNodes("connconfigfile");
                        if (connconfigfileNodes.Count > 0)
                        {
                            var configPath = connconfigfileNodes.Item(0).InnerText;
                            configFileFullPath = Path.Combine(serviceMainPath, configPath);
                            //设置服务数据库连接
                            SaveServiceMySqlConnection(configFileFullPath);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Core.Log.Error("设置服务名时出错(SetServicesConfig):"+ex.Message);
            }
        }
        private void SaveServiceName(string configPath, string serviceSuffix)
        {
            var configFileFullPath = configPath;
            if (!string.IsNullOrWhiteSpace(configFileFullPath))
            {
                XmlDocument xml = new XmlDocument();
                XmlNode serviceNode;
                if (System.IO.File.Exists(configFileFullPath))
                {
                    xml.Load(configFileFullPath);
                    serviceNode = xml.SelectSingleNode("Settings/ServiceName");
                    if (serviceNode != null)
                    {
                        serviceNode.InnerText = serviceSuffix;
                    }
                    serviceNode = xml.SelectSingleNode("Settings/DisplayName");
                    if (serviceNode != null)
                    {
                        serviceNode.InnerText = serviceSuffix;
                    }
                    serviceNode = xml.SelectSingleNode("Settings/Description");
                    if (serviceNode != null)
                    {
                        serviceNode.InnerText = serviceSuffix;
                    }
                    xml.Save(configFileFullPath);
                }
            }
        }

        private void SaveServiceMySqlConnection(string configPath)
        {
            try
            {
                if (System.IO.File.Exists(configPath))
                {
                    var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(configPath);
                    // 写入数据库连接信息
                    if (config.ConnectionStrings.ConnectionStrings["Entities"] != null)
                    {
                        config.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString = GetEFConnectionString();
                    }
                    if (config.ConnectionStrings.ConnectionStrings["mysql"] != null)
                    {
                        config.ConnectionStrings.ConnectionStrings["mysql"].ConnectionString = GetSimpleConnectionString();
                    }
                    if (config.AppSettings.Settings["ConnString"] !=null)
                    {
                        config.AppSettings.Settings["ConnString"].Value = GetSimpleConnectionString();
                    }
                    config.Save();
                }
                
            }
            catch (Exception ex)
            {
                Core.Log.Error("设置服务数据库链接异常：" + ex.Message);
            }
        }
        public ActionResult Configuration()
        {
            ViewBag.IsDebug = GetSolutionDebugState();
            return View();
        }

        private bool GetSolutionDebugState()
        {
#if !DEBUG
            return false;
#elif DEBUG
            return true;
#endif
        }

        #region ExecuteTest
        private bool ExecuteTest()
        {
            string errorMsg;
            errorMsgs = new List<string>();

            // 检查数据连接和数据库权限
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            DbTransaction transaction = null;
            DbConnection connection = null;

            try
            {
                if (ValidateConnectionStrings(out errorMsg))
                {
                    using (connection = new MySqlConnection(GetConnectionString()))
                    {
                        connection.Open();

                        DbCommand dbCmd = connection.CreateCommand();
                        transaction = connection.BeginTransaction();

                        dbCmd.Connection = connection;
                        dbCmd.Transaction = transaction;

                        // 创建测试表
                        dbCmd.CommandText = "CREATE TABLE installTest(Test bit NULL)";
                        dbCmd.ExecuteNonQuery();

                        // 删除测试表
                        dbCmd.CommandText = "DROP TABLE installTest";
                        dbCmd.ExecuteNonQuery();

                        transaction.Commit();
                        connection.Close();
                    }
                }
                else
                {
                    errorMsgs.Add(errorMsg);
                }
            }
            catch (Exception ex)
            {
                errorMsgs.Add(ex.Message);

                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        errorMsgs.Add(ex2.Message);
                        return (errorMsgs.Count == 0);
                    }
                }

                if (connection != null && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                }
                return (errorMsgs.Count == 0);
            }

            return (errorMsgs.Count == 0);
        }
        /// <summary>
        /// 测试目录读写权限
        /// </summary>
        /// <returns></returns>
        private bool TestPermission()
        {
            string errorMsg, testPath;
            errorMsgs = new List<string>();
            // 检查config目录的权限
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            //testPath = Request.MapPath( Request.ApplicationPath + "/config/test.txt" );
            //if( !TestFolder( testPath , out errorMsg ) )
            //{
            //    errorMsgs.Add( errorMsg );
            //}

            // 检查web.config文件的修改权限
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);

                if (config.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString == "none")
                    config.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString = "required";
                else
                    config.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString = "none";

                config.Save();
            }
            catch (Exception ex)
            {
                errorMsgs.Add(ex.Message);
            }

            // 检查Storage目录的权限
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            testPath = Request.MapPath(Request.ApplicationPath + "/storage/test.txt");
            if (!TestFolder(testPath, out errorMsg))
            {
                errorMsgs.Add(errorMsg);
            }

            return (errorMsgs.Count == 0);
        }
        #endregion
        private bool ValidateUser(out string msg)
        {
            msg = null;

            // 空检查
            if (
                string.IsNullOrEmpty(_siteAdminName) ||
                string.IsNullOrEmpty(_sitePwd) || string.IsNullOrEmpty(_sitePwd2)
                )
            {
                msg = "管理员账号信息不完整";
                return false;
            }

            // 空检查
            if (
                string.IsNullOrEmpty(_shopAdminName) ||
                string.IsNullOrEmpty(_shopPwd) || string.IsNullOrEmpty(_shopPwd2)
                )
            {
                msg = "店铺管理员账号信息不完整";
                return false;
            }


            // 检查用户名长度
            if (_siteAdminName.Length > usernameMaxLength || _siteAdminName.Length < usernameMinLength)
            {
                msg = string.Format("管理员用户名的长度只能在{0}和{1}个字符之间", usernameMinLength, usernameMaxLength);
                return false;
            }

            // 检查用户名长度
            if (_shopAdminName.Length > usernameMaxLength || _shopAdminName.Length < usernameMinLength)
            {
                msg = string.Format("店铺管理员用户名的长度只能在{0}和{1}个字符之间", usernameMinLength, usernameMaxLength);
                return false;
            }

            // 检查是否和匿名用户名重复
            if (string.Compare(_siteAdminName, "anonymous", true) == 0)
            {
                msg = "不能使用anonymous作为管理员用户名";
                return false;
            }

            // 检查是否和匿名用户名重复
            if (string.Compare(_shopAdminName, "anonymous", true) == 0)
            {
                msg = "不能使用anonymous作为店铺管理员用户名";
                return false;
            }

            // 检查用户名格式
            if (!Regex.IsMatch(_siteAdminName, usernameRegex))
            {
                msg = "管理员用户名的格式不符合要求，用户名一般由字母、数字、下划线和汉字组成，且必须以汉字或字母开头";
                return false;
            }

            // 检查用户名格式
            if (!Regex.IsMatch(_shopAdminName, usernameRegex))
            {
                msg = "店铺管理员用户名的格式不符合要求，用户名一般由字母、数字、下划线和汉字组成，且必须以汉字或字母开头";
                return false;
            }

            // 比较两次密码输入
            if (_sitePwd != _sitePwd2)
            {
                msg = "管理员登录密码两次输入不一致";
                return false;
            }

            // 比较店铺两次密码输入
            if (_shopPwd != _shopPwd2)
            {
                msg = "店铺管理员登录密码两次输入不一致";
                return false;
            }

            // 检查密码长度
            if (
                _sitePwd.Length < passwordMinLength ||
                _sitePwd.Length > passwordMaxLength
                )
            {
                msg = string.Format("管理员登录密码的长度只能在{0}和{1}个字符之间",
                                    passwordMinLength,
                                    passwordMaxLength);
                return false;
            }

            // 检查密码长度
            if (
                _shopPwd.Length < passwordMinLength ||
                _shopPwd.Length > passwordMaxLength
                )
            {
                msg = string.Format("店铺管理员登录密码的长度只能在{0}和{1}个字符之间",
                                    passwordMinLength,
                                    passwordMaxLength);
                return false;
            }
            return true;
        }
        private bool ValidateConnectionStrings(out string msg)
        {
            msg = null;

            // 不校验数据库登录密码
            if (
                string.IsNullOrEmpty(_dbServer) ||
                string.IsNullOrEmpty(_dbName) ||
                string.IsNullOrEmpty(_dbLoginName)
                )
            {
                // 数据库地址，数据库名称和数据库用户名是必填项
                msg = "数据库连接信息不完整";
                return false;
            }

            return true;
        }
        private static bool TestFolder(string folderPath, out string errorMsg)
        {
            try
            {
                // 创建测试文件
                System.IO.File.WriteAllText(folderPath, "Hi");

                // 修改测试文件
                System.IO.File.AppendAllText(folderPath, ",This is a test file.");

                // 删除文件
                System.IO.File.Delete(folderPath);

                errorMsg = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }
        public ActionResult Completed()
        {
            return View();
        }
        #region helper
        private string GetConnectionString()
        {
            int port = 0;
            if (int.TryParse(_dbPort, out port))
            {
            }

            //return String.Format(
            //    "provider=Sql.Data.SqlClient;server={0};database={1};user id={2};password={3};persistsecurityinfo=True;" ,
            //    _dbServer , port == 0 ? "" : "," + _dbPort , _dbName , _dbLoginName , _dbPwd );

            return String.Format(
                "server={0};database={1};user id={2};password={3};persistsecurityinfo=True;",
                _dbServer, _dbName, _dbLoginName, _dbPwd);

            //return String.Format(
            //    "Data Source = {0};Initial Catalog = {1};User Id = {2};Password = {3};" ,
            //    _dbServer , _dbName , _dbLoginName , _dbPwd );
        }

        private string GetEFConnectionString()
        {
            int port = 0;
            if (int.TryParse(_dbPort, out port))
            {
            }
            //return String.Format(
            //    "metadata=res://*/Entities.csdl|res://*/Entities.ssdl|res://*/Entities.msl;provider=System.Data.SqlClient;provider connection string=\"data source={0}{1};initial catalog={2};persist security info=True;uid={3};Password={4};MultipleActiveResultSets=True;App=EntityFramework\";" ,
            //    _dbServer , port == 0 ? "" : "," + _dbPort , _dbName , _dbLoginName , _dbPwd );

            return String.Format(
                "metadata=res://*/Entities.csdl|res://*/Entities.ssdl|res://*/Entities.msl;provider=MySql.Data.MySqlClient;provider connection string=\" server={0}{1};user id={2};password={3};persistsecurityinfo=True;database={4}\";",
                _dbServer, port == 0 ? "" : "," + _dbPort, _dbLoginName, _dbPwd, _dbName);
        }

        private string GetSimpleConnectionString()
        {
            int port = 0;
            if (int.TryParse(_dbPort, out port))
            {
            }
            //return String.Format(
            //    "metadata=res://*/Entities.csdl|res://*/Entities.ssdl|res://*/Entities.msl;provider=System.Data.SqlClient;provider connection string=\"data source={0}{1};initial catalog={2};persist security info=True;uid={3};Password={4};MultipleActiveResultSets=True;App=EntityFramework\";" ,
            //    _dbServer , port == 0 ? "" : "," + _dbPort , _dbName , _dbLoginName , _dbPwd );

            return String.Format(
                "server={0}{1};user id={2};password={3};persistsecurityinfo=True;database={4};Charset=utf8",
                _dbServer, port == 0 ? "" : "," + _dbPort, _dbLoginName, _dbPwd, _dbName);
        }

        #endregion

        private bool CreateDtabase(out string msg)
        {
            string connection = String.Format(
                "server={0};user id={1};password={2};persistsecurityinfo=True;",
                _dbServer, _dbLoginName, _dbPwd);

            using (DbConnection con = new MySqlConnection(connection))
            {
                msg = "";
                DbCommand com = con.CreateCommand();
                com.CommandType = CommandType.Text;
                com.CommandText = "CREATE DATABASE " + _dbName;
                if (_dbName.IndexOf('.') >= 0)
                {
                    msg = "数据库名不能含有字符\".\"";
                    return false;
                }
                try
                {
                    con.Open();
                    com.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    msg = "数据库创建失败";
                    return false;
                }
                finally
                {
                    con.Close();
                }
            }
            return true;
        }

        #region 创建数据库架构
        private bool CreateDataSchema(out string errorMsg)
        {
            string filename = Request.MapPath("/SqlScripts/Schema.sql");

            // 如果数据架构文件存在则创建架构
            if (!System.IO.File.Exists(filename))
            {
                errorMsg = "没有找到数据库架构文件-Schema.sql";
                return false;
            }
            try
            {
                return ExecuteScriptFile(filename, out errorMsg);
            }
            catch
            {
                errorMsg = "数据架构创建错误";
                return false;
            }
        }
        #endregion

        #region script helper
        private bool ExecuteScriptFile(string pathToScriptFile, out string errorMsg)
        {
            StreamReader reader = null;
            DbConnection connection = null;

            string strSql;
            string applicationPath = Request.ApplicationPath;

            using (reader = new StreamReader(pathToScriptFile))
            {
                using (connection = new MySqlConnection(GetConnectionString()))
                {
                    DbCommand dbCmd = connection.CreateCommand();
                    dbCmd.Connection = connection;
                    dbCmd.CommandType = CommandType.Text;
                    dbCmd.CommandTimeout = 360;

                    // 考虑到安装脚本可能比较大，将命令超时时间设为6分钟
                    connection.Open();

                    while (!reader.EndOfStream)
                    {
                        try
                        {
                            strSql = NextSqlFromStream(reader);

                            if (!string.IsNullOrEmpty(strSql))
                            {
                                dbCmd.CommandText = strSql.Replace("$VirsualPath$", applicationPath);
                                dbCmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }

                    }

                    connection.Close();
                }

                reader.Close();
            }

            errorMsg = null;
            return true;
        }

        private static string NextSqlFromStream(StreamReader reader)
        {
            StringBuilder sb = new StringBuilder();
            string lineOfText = reader.ReadLine().Trim();

            while (!reader.EndOfStream && string.Compare(lineOfText, "GO", true, CultureInfo.InvariantCulture) != 0)
            {
                sb.Append(lineOfText + Environment.NewLine);
                lineOfText = reader.ReadLine();
            }

            // 如果最后一句不是GO,添加最后一句
            if (string.Compare(lineOfText, "GO", true, CultureInfo.InvariantCulture) != 0)
                sb.Append(lineOfText + Environment.NewLine);

            return sb.ToString();
        }
        #endregion

        #region 添加演示数据
        private bool AddDemoData(out string errorMsg)
        {
            string filename = Request.MapPath("/SqlScripts/SiteDemo.zh-CN.sql");
            if (!System.IO.File.Exists(filename))
            {
                errorMsg = "没有找到演示数据文件-SiteDemo.Sql";
                return false;
            }

            try
            {
                return ExecuteScriptFile(filename, out errorMsg);
            }
            catch
            {
                errorMsg = "演示数据创建错误";
                return false;
            }
        }
        #endregion

        #region 创建超级管理员
        private bool CreateAdministrator(out string errorMsg)
        {
            DbConnection connection = null;
            DbTransaction transaction = null;

            try
            {
                using (connection = new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    DbCommand dbCmd = connection.CreateCommand();
                    transaction = connection.BeginTransaction();

                    dbCmd.Connection = connection;
                    dbCmd.Transaction = transaction;
                    dbCmd.CommandType = CommandType.Text;

                    var sitePwdSalt = Guid.NewGuid().ToString();
                    var sitePwd = GetPasswrodWithTwiceEncode(_sitePwd, sitePwdSalt);

                    var shopPwdSalt = Guid.NewGuid().ToString();
                    var shopPwd = GetPasswrodWithTwiceEncode(_shopPwd, shopPwdSalt);

                    // 自营店id
                    long shopId = 1;
                    //dbCmd.CommandText =
                    //    "SELECT top 1 Id FROM Himall_Shops";
                    dbCmd.CommandText =
                        "SELECT Id FROM Himall_Shops limit 1";
                    var shopIdObj = dbCmd.ExecuteScalar();
                    if (shopIdObj != null)
                    {
                        shopId = (long)shopIdObj;
                    }
                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText =
                        "INSERT INTO Himall_Managers  (shopId, RoleId, UserName, Password, PasswordSalt, CreateDate)" +
                        "VALUES (@shopId, 0, @userName, @Password, @PasswordSalt,@CreateDate )";
                    dbCmd.Parameters.Add(new MySqlParameter("@shopId", shopId));
                    dbCmd.Parameters.Add(new MySqlParameter("@userName", _shopAdminName));
                    dbCmd.Parameters.Add(new MySqlParameter("@Password", shopPwd));
                    dbCmd.Parameters.Add(new MySqlParameter("@PasswordSalt", shopPwdSalt));
                    dbCmd.Parameters.Add(new MySqlParameter("@CreateDate", DateTime.Now));
                    dbCmd.ExecuteNonQuery();



                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText =
                       "INSERT INTO Himall_Managers  (shopId, RoleId, UserName, Password, PasswordSalt, CreateDate)" +
                       "VALUES (0, 0, @userName, @Password, @PasswordSalt,@CreateDate )";
                    dbCmd.Parameters.Add(new MySqlParameter("@userName", _siteAdminName));
                    dbCmd.Parameters.Add(new MySqlParameter("@Password", sitePwd));
                    dbCmd.Parameters.Add(new MySqlParameter("@PasswordSalt", sitePwdSalt));
                    dbCmd.Parameters.Add(new MySqlParameter("@CreateDate", DateTime.Now));
                    dbCmd.ExecuteNonQuery();


                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText = "SET FOREIGN_KEY_CHECKS=0;" +
                       "INSERT INTO Himall_Members  (id,UserName, Password, PasswordSalt,TopRegionId,RegionId,OrderNumber,Disabled,Points,Expenditure,CreateDate,LastLoginDate)" +
                       "VALUES (569,@userName, @Password, @PasswordSalt,0,0,0,0,0,0.00,@CreateDate,@LastLoginDate );"+ "set foreign_key_checks=1;";
                    dbCmd.Parameters.Add(new MySqlParameter("@userName", _shopAdminName));
                    dbCmd.Parameters.Add(new MySqlParameter("@Password", shopPwd));
                    dbCmd.Parameters.Add(new MySqlParameter("@PasswordSalt", shopPwdSalt));
                    dbCmd.Parameters.Add(new MySqlParameter("@CreateDate", DateTime.Now));
                    dbCmd.Parameters.Add(new MySqlParameter("@LastLoginDate", DateTime.Now));
                    dbCmd.ExecuteNonQuery();


                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText =
                       "update Himall_SiteSettings set Value=@SiteName WHERE  `Key`='SiteName'";
                    dbCmd.Parameters.Add(new MySqlParameter("@SiteName", _siteName));
                    dbCmd.ExecuteNonQuery();

                    transaction.Commit();
                    connection.Close();
                }

                errorMsg = null;
                return true;
            }
            catch (SqlException ex)
            {
                errorMsg = ex.Message;

                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        errorMsg = ex2.Message;
                    }
                }

                if (connection != null && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                }

                return false;
            }
        }
        #endregion

        #region 修改图片
        private bool UpdateSliderImage(out string errorMsg)
        {
            DbConnection connection = null;
            DbTransaction transaction = null;

            try
            {
                using (connection = new MySqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    DbCommand dbCmd = connection.CreateCommand();
                    transaction = connection.BeginTransaction();

                    dbCmd.Connection = connection;
                    dbCmd.Transaction = transaction;
                    dbCmd.CommandType = CommandType.Text;

                    var sitePwdSalt = Guid.NewGuid().ToString();
                    var sitePwd = GetPasswrodWithTwiceEncode(_sitePwd, sitePwdSalt);

                    var shopPwdSalt = Guid.NewGuid().ToString();
                    var shopPwd = GetPasswrodWithTwiceEncode(_shopPwd, shopPwdSalt);

                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText =
                       "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`>1 and `Id`<8";
                    dbCmd.Parameters.Add(new MySqlParameter("@ImageUrl", "http://fpoimg.com/226x288"));
                    dbCmd.ExecuteScalar();
                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText =
                       "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`=8";
                    dbCmd.Parameters.Add(new MySqlParameter("@ImageUrl", "http://fpoimg.com/464x288"));
                    dbCmd.ExecuteScalar();

                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText =
                       "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`>8 and `Id`<13";
                    dbCmd.Parameters.Add(new MySqlParameter("@ImageUrl", "http://fpoimg.com/226x288"));
                    dbCmd.ExecuteScalar();

                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText =
                       "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`=1";
                    dbCmd.Parameters.Add(new MySqlParameter("@ImageUrl", "http://fpoimg.com/464x288"));
                    dbCmd.ExecuteScalar();

                    dbCmd.Parameters.Clear();
                    dbCmd.CommandText =
                       "update Himall_ImageAds set ImageUrl=@ImageUrl WHERE  `Id`=13";
                    dbCmd.Parameters.Add(new MySqlParameter("@ImageUrl", "http://fpoimg.com/310x165"));
                    dbCmd.ExecuteScalar();

                    transaction.Commit();
                    connection.Close();
                }

                errorMsg = null;
                return true;
            }
            catch (SqlException ex)
            {
                errorMsg = ex.Message;

                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        errorMsg = ex2.Message;
                    }
                }

                if (connection != null && connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                }

                return false;
            }
        }
        #endregion

        #region 添加初始化数据

        private bool AddInitData(out string errorMsg)
        {
            string imagepath = Request.MapPath("/SqlScripts/Storage");
            string destpath = Request.MapPath("/Storage");
            try
            {
                Directory.Move(imagepath, destpath);
            }
            catch
            {
            }

            string filename = Request.MapPath("/SqlScripts/SiteInitData.zh-CN.Sql");

            if (!System.IO.File.Exists(filename))
            {
                errorMsg = "没有找到初始化数据文件-SiteInitData.Sql";
                return false;
            }

            try
            {
                return ExecuteScriptFile(filename, out errorMsg);
            }
            catch
            {
                errorMsg = "初始化数据创建错误";
                return false;
            }
        }
        #endregion

        #region 保存web.config
        private bool SaveConfig(out string errorMsg)
        {
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);

                // 删除安装标识配置节
                config.AppSettings.Settings["IsInstalled"].Value = "true";

                //写入当前网址
                var curhttp = System.Web.HttpContext.Current;
                config.AppSettings.Settings["CurDomainUrl"].Value = curhttp.Request.Url.Scheme + "://" + curhttp.Request.Url.Authority;

                // 写入数据库连接信息

                config.ConnectionStrings.ConnectionStrings["Entities"].ConnectionString = GetEFConnectionString();
                config.ConnectionStrings.ConnectionStrings["mysql"].ConnectionString = GetSimpleConnectionString();
                config.Save();

                errorMsg = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        #endregion
    }
}