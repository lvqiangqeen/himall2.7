using Himall.IServices;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Himall.Web.App_Code.UEditor
{
    public class UploadHandle:IUEditorHandle
    {
        UploadConfig UploadConfig { get;  set; }
        UploadResult Result { get; set; }

        public UploadHandle(UploadConfig config)
        {
            this.UploadConfig = config;
            this.Result = new UploadResult() { State = UploadState.Unknown };
        }

        public object Process()
        {
            byte[] uploadFileBytes = null;
            string uploadFileName = null;
            int fileLength = 0;
           // Stream stream = null;

            if (UploadConfig.Base64)
            {
                uploadFileName = UploadConfig.Base64Filename;
                uploadFileBytes = Convert.FromBase64String(HttpContext.Current.Request[UploadConfig.UploadFieldName]);
            }
            else
            {
                var file = HttpContext.Current.Request.Files[UploadConfig.UploadFieldName];
                uploadFileName = file.FileName;

                if (!CheckFileType(uploadFileName))
                {
                    Result.State = UploadState.TypeNotAllow;
                    return WriteResult();
                }
                if (!CheckFileSize(file.ContentLength))
                {
                    Result.State = UploadState.SizeLimitExceed;
                    return WriteResult();
                }
                fileLength = file.ContentLength;
                uploadFileBytes = new byte[file.ContentLength];
                try
                {
                    
                    file.InputStream.Read(uploadFileBytes, 0, file.ContentLength);
                    //stream = file.InputStream;
                }
                catch (Exception)
                {
                    Result.State = UploadState.NetworkError;
                    WriteResult();
                }
            }

            Result.OriginFileName = uploadFileName;

            var savePath = PathFormatter.Format(uploadFileName, UploadConfig.PathFormat);
            var localPath = HttpContext.Current.Server.MapPath(savePath);
            try
            {   //YZY改写为文件处理策略
                //if (!Directory.Exists(Path.GetDirectoryName(localPath)))
                //{
                //    Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                //}
                ServiceHelper.Create<IPhotoSpaceService>().AddPhote(
                    Convert.ToInt32(HttpContext.Current.Request.Form["cate_id"]), 
                    Result.OriginFileName,
                    savePath, 
                    fileLength,
                    UploadConfig.ShopId
                    );


                Core.HimallIO.CreateFile(savePath,BytesToStream(uploadFileBytes),Core.FileCreateType.Create);
                //File.WriteAllBytes(localPath, uploadFileBytes);
                Result.Url =Core.HimallIO.GetImagePath(savePath);
                Result.State = UploadState.Success;
            }
            catch (Exception e)
            {
                Result.State = UploadState.FileAccessError;
                Result.ErrorMessage = e.Message;
            }
            return WriteResult();
        }

        private Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
        private object WriteResult()
        {
            return (new
            {
                state = GetStateMessage(Result.State),
                url = Result.Url,
                title = Result.OriginFileName,
                original = Result.OriginFileName,
                error = Result.ErrorMessage
            });
        }

        private string GetStateMessage(UploadState state)
        {
            switch (state)
            {
                case UploadState.Success:
                    return "SUCCESS";
                case UploadState.FileAccessError:
                    return "文件访问出错，请检查写入权限";
                case UploadState.SizeLimitExceed:
                    return "文件大小超出服务器限制";
                case UploadState.TypeNotAllow:
                    return "不允许的文件格式";
                case UploadState.NetworkError:
                    return "网络错误";
            }
            return "未知错误";
        }

        private bool CheckFileType(string filename)
        {
            var fileExtension = Path.GetExtension(filename).ToLower();
            return UploadConfig.AllowExtensions.Select(x => x.ToLower()).Contains(fileExtension);
        }

        private bool CheckFileSize(int size)
        {
            return size < UploadConfig.SizeLimit;
        }
    }

    public class UploadConfig
    {
        /// <summary>
        /// 店铺ID
        /// </summary>
        public long ShopId { get; set; }
        /// <summary>
        /// 文件命名规则
        /// </summary>
        public string PathFormat { get; set; }

        /// <summary>
        /// 上传表单域名称
        /// </summary>
        public string UploadFieldName { get; set; }

        /// <summary>
        /// 上传大小限制
        /// </summary>
        public int SizeLimit { get; set; }

        /// <summary>
        /// 上传允许的文件格式
        /// </summary>
        public string[] AllowExtensions { get; set; }

        /// <summary>
        /// 文件是否以 Base64 的形式上传
        /// </summary>
        public bool Base64 { get; set; }

        /// <summary>
        /// Base64 字符串所表示的文件名
        /// </summary>
        public string Base64Filename { get; set; }
    }

    public class UploadResult
    {
        public UploadState State { get; set; }
        public string Url { get; set; }
        public string OriginFileName { get; set; }

        public string ErrorMessage { get; set; }
    }

    public enum UploadState
    {
        Success = 0,
        SizeLimitExceed = -1,
        TypeNotAllow = -2,
        FileAccessError = -3,
        NetworkError = -4,
        Unknown = 1,
    }
}