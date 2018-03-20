using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Strategy
{
    public enum IOErrorMsg
    {
        /// <summary>
        /// 文件名称不能为空
        /// </summary>
        [Description("文件名称不能为空")]
        FileEmpty = 1,

        /// <summary>
        /// 文件已存在
        /// </summary>
        [Description("文件已存在")]
        FileExist = 2,

        /// <summary>
        /// 目录已存在
        /// </summary>
        [Description("目录已存在")]
        DirExist = 4,

        /// <summary>
        /// 文件不存在
        /// </summary>
        [Description("文件不存在")]
        FileNotExist = 8,

        /// <summary>
        /// 目录不存在
        /// </summary>
        [Description("目录不存在")]
        DirNotExist = 16,

        /// <summary>
        /// 目录下包含子目录或文件，无法删除
        /// </summary>
        [Description("目录下包含子目录或文件，无法删除")]
        DirDeleleError = 32,

        /// <summary>
        /// 文件为普通文件，不允许操作
        /// </summary>
        [Description("文件为普通文件，不允许操作")]
        NoramlFileNotOperate = 64,

        /// <summary>
        /// 文件为可追加文件，不允许操作
        /// </summary>
        [Description("文件为可追加文件，不允许操作")]
        AppendFileNotOperate = 128,

        /// <summary>
        /// 目标文件为可追加文件，不允许操作
        /// </summary>
        [Description("目标文件为可追加文件，不允许操作")]
        TargetAppendFileNotOperate = 256
    }
}
