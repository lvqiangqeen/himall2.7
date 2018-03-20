using Himall.Core.Strategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Core
{
    /// <summary>
    /// 文件操作接口
    /// </summary>
    public interface IHimallIO : IStrategy
    {
        /// <summary>
        /// 获取文件的绝对路径
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        string GetFilePath(string fileName);

        /// <summary>
        /// 获取图片的路径
        /// </summary>
        /// <param name="imageName">图片文件名称</param>
        /// <param name="styleName">图片的样式</param>
        /// <returns></returns>
        string GetImagePath(string imageName, string styleName = null);

        /// <summary>
        /// 获取文件内容
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        byte[] GetFileContent(string fileName);

        /// <summary>
        /// 创建普通文件（该文件不可追加内容）
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="stream">文件流</param>
        /// <param name="fileCreateType">文件的创建类型 默认为创建新文件</param>
        void CreateFile(string fileName, Stream stream, FileCreateType fileCreateType = FileCreateType.CreateNew);

        /// <summary>
        /// 创建普通文件（该文件不可追加内容）
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="content">文件内容</param>
        /// <param name="fileCreateType">文件的创建类型</param>
        void CreateFile(string fileName, string content, FileCreateType fileCreateType = FileCreateType.CreateNew);

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="dirName">目录名称</param>
        void CreateDir(string dirName);

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        bool ExistFile(string fileName);

        /// <summary>
        /// 判断目录是否存在
        /// </summary>
        /// <param name="dirName">目录名称</param>
        /// <returns></returns>
        bool ExistDir(string dirName);

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="dirName">要移除的目录的名称</param>
        /// <param name="recursive">要移除 path 中的目录、子目录和文件，则为 true；否则为 false</param>
        void DeleteDir(string dirName, bool recursive = false);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName">文件名称</param>
        void DeleteFile(string fileName);

        /// <summary>
        /// 删除多个文件
        /// </summary>
        /// <param name="fileNames">文件名称集合</param>
        void DeleteFiles(List<string> fileNames);

        /// <summary>
        /// 将文件拷贝到新位置
        /// </summary>
        /// <param name="sourceFileName">要拷贝的文件名称</param>
        /// <param name="destFileName">目标文件的名称</param>
        /// <param name="overwrite">是否允许覆盖 默认为false</param>
        void CopyFile(string sourceFileName, string destFileName, bool overwrite = false);

        /// <summary>
        /// 将文件移动到新位置
        /// </summary>
        /// <param name="sourceFileName">要移动的文件名称</param>
        /// <param name="destFileName">目标文件的名称</param>
        /// <param name="overwrite">是否允许覆盖 默认为false</param>
        void MoveFile(string sourceFileName, string destFileName, bool overwrite = false);

        /// <summary>
        /// 列出目录下的文件和子目录
        /// </summary>
        /// <param name="dirName">目录名称</param>
        /// <param name="self">是否包含本身 默认为false</param>
        /// <returns></returns>
        List<string> GetDirAndFiles(string dirName, bool self = false);

        /// <summary>
        /// 列出目录下所有文件
        /// </summary>
        /// <param name="dirName">目录名称</param>
        /// <param name="self">是否包含本身 false</param>
        /// <returns></returns>
        List<string> GetFiles(string dirName, bool self = false);

        /// <summary>
        /// 指定的文件下追加内容（如果文件不存在，则创建可追加文件）
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="stream">文件流</param>
        void AppendFile(string fileName, Stream stream);

        /// <summary>
        /// 指定的文件下追加内容（如果文件不存在，则创建可追加文件）
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="content">追加内容</param>
        void AppendFile(string fileName, string content);

        /// <summary>
        /// 获取目录基本信息
        /// </summary>
        /// <param name="dirName">目录名称</param>
        /// <returns></returns>
        MetaInfo GetDirMetaInfo(string dirName);

        /// <summary>
        /// 获取文件基本信息
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        MetaInfo GetFileMetaInfo(string fileName);
        /// <summary>
        /// 生成图片缩略图
        /// </summary>
        /// <param name="sourceFilename"></param>
        /// <param name="destFilename"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void CreateThumbnail(string sourceFilename, string destFilename, int width, int height);
        /// <summary>
        /// 获取不同尺寸的产品图片
        /// </summary>
        /// <param name="productPath"></param>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        string GetProductSizeImage(string productPath, int index, int width = 0);
    }
}
