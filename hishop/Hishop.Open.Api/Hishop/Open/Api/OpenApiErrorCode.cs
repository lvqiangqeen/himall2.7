namespace Hishop.Open.Api
{
    using System;
    using System.ComponentModel;

    public enum OpenApiErrorCode
    {
        [Description("非法的交易订单（或子订单）ID")]
        Biz_Order_ID_is_Invalid = 0x1fb,
        [Description("物流公司不存在")]
        Company_not_Exists = 0x201,
        [Description("请求被禁止")]
        Forbidden_Request = 0x10,
        [Description("开发者权限不足")]
        Insufficient_ISV_Permissions = 1,
        [Description("用户权限不足")]
        Insufficient_User_Permissions = 2,
        [Description("非法的AppKey参数")]
        Invalid_App_Key = 10,
        [Description("非法的参数")]
        Invalid_Arguments = 15,
        [Description("非法数据格式")]
        Invalid_Format = 6,
        [Description("不存在的方法名")]
        Invalid_Method = 5,
        [Description("非法签名")]
        Invalid_Signature = 8,
        [Description("非法的时间戳参数")]
        Invalid_Timestamp = 13,
        [Description("缺少AppKey参数")]
        Missing_App_Key = 9,
        [Description("缺少方法名参数")]
        Missing_Method = 4,
        [Description("缺少参数")]
        Missing_Parameters = 0x1f5,
        [Description("缺少必选参数")]
        Missing_Required_Arguments = 14,
        [Description("缺少签名参数")]
        Missing_Signature = 7,
        [Description("缺少时间戳参数")]
        Missing_Timestamp = 12,
        [Description("需要绑定用户昵称")]
        Need_Binding_User = 0x1f6,
        [Description("运单号太长")]
        Out_Sid_Too_Long = 0x202,
        [Description("页码条数超出长度限制")]
        Page_Size_Too_Long = 0x1fd,
        [Description("参数错误")]
        Parameter_Error = 0x11,
        [Description("参数格式错误")]
        Parameters_Format_Error = 0x1f7,
        [Description("修改商品状态失败")]
        Product_ApproveStatus_Faild = 0x25c,
        [Description("商品不存在")]
        Product_Not_Exists = 0x259,
        [Description("状态不在指定范围之内")]
        Product_Status_is_Invalid = 0x25a,
        [Description("商品库存不足")]
        Product_Stock_Lack = 0x203,
        [Description("修改商品库存失败")]
        Product_UpdateeQuantity_Faild = 0x25b,
        [Description("远程服务出错")]
        Remote_Service_Error = 3,
        [Description("配送方式不存在")]
        ShippingMode_not_Exists = 0x205,
        [Description("系统错误")]
        System_Error = 0x12,
        [Description("结束时间晚于当前时间")]
        Time_End_Now = 0x1ff,
        [Description("开始时间晚于结束时间")]
        Time_Start_End = 510,
        [Description("开始时间晚于当前时间")]
        Time_Start_Now = 0x200,
        [Description("查询条件(修改时间)跨度不能超过一天")]
        Time_StartModified_AND_EndModified = 520,
        [Description("交易标记值不在指定范围之内")]
        Trade_Flag_Too_Long = 0x206,
        [Description("非法交易")]
        Trade_is_Invalid = 0x1f9,
        [Description("来自门店的订单")]
        Trade_is_Store = 0x209,
        [Description("交易备注超出长度限制")]
        Trade_Memo_Too_Long = 0x1fc,
        [Description("交易不存在")]
        Trade_not_Exists = 0x1f8,
        [Description("订单打印失败")]
        Trade_Print_Faild = 0x20b,
        [Description("状态不在指定范围之内")]
        Trade_Status_is_Invalid = 0x207,
        [Description("订单状态不允许打印")]
        Trade_Status_Print = 0x20a,
        [Description("订单状态不允许进行发货")]
        Trade_Status_Send = 0x204,
        [Description("用户不存在")]
        User_not_Exists = 0x1fa
    }
}

