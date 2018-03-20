using Himall.IServices;
using AutoMapper;
using Himall.DTO;
using Himall.Core;

/**
 * 主题设置实现
 * 2016-05-16
 * **/
namespace Himall.Application
{
    public  class ThemeApplication 
    {

        private static IThemeService _iThemeService= ObjectContainer.Current.Resolve<IThemeService>();

        /// <summary>
        /// 商城主题设置设置,如果有数据即修改
        /// </summary>
        /// <param name="mTheme">主题实体类</param>
        public static void SetTheme(Himall.DTO.Theme mVTheme)
        {
            new Himall.Model.UserMemberInfo();

            Mapper.CreateMap<Himall.DTO.Theme, Himall.Model.ThemeInfo>();
            var mTheme = Mapper.Map<Himall.DTO.Theme, Himall.Model.ThemeInfo>(mVTheme);

            if (mVTheme.ThemeId <= 0)
            {

                _iThemeService.AddTheme(mTheme);
            }
            else
            {
                _iThemeService.UpdateTheme(mTheme);
            }

        }

        /// <summary>
        /// 获取商城主题
        /// </summary>
        /// <returns></returns>
        public static Himall.DTO.Theme getTheme()
        {
            Himall.Model.ThemeInfo mTheme = _iThemeService.getTheme();
            Himall.DTO.Theme mVTheme = new Theme();
            if (mTheme != null)
            {

                Mapper.CreateMap<Himall.Model.ThemeInfo, Himall.DTO.Theme>();
                mVTheme = Mapper.Map<Himall.Model.ThemeInfo, Himall.DTO.Theme>(mTheme);
            }
            else
            {
                mVTheme.TypeId = Himall.CommonModel.ThemeType.Defaults;
            }

            return mVTheme;
        }


    }
}
