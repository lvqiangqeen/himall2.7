using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Admin.Models.Product
{
    public class TypeInfoModel
    {
        public long Id { get; set; }
        public string ColorValue { get; set; }
        public bool IsSupportColor { get; set; }
        public bool IsSupportSize { get; set; }
        public bool IsSupportVersion { get; set; }

        public string ColorAlias { get; set; }
        public string SizeAlias { get; set; }
        public string VersionAlias { get; set; }
        public string SizeValue { get; set; }

        public string VersionValue { get; set; }

        [Required(ErrorMessage = "类型名称必填")]
        public string Name { get; set; }
        public List<TypeAttribute> Attributes { get; set; }
        public List<BrandInfoModel> Brands { get; set; }

        public static implicit operator ProductTypeInfo(TypeInfoModel m)
        {
            ProductTypeInfo info = new ProductTypeInfo()
            {
                Id = m.Id,
                Name = m.Name,
                ColorValue = m.ColorValue,
                SizeValue = m.SizeValue,
                VersionValue = m.VersionValue,
                ColorAlias = m.ColorAlias,
                SizeAlias = m.SizeAlias,
                VersionAlias = m.VersionAlias,
                SpecificationValueInfo = new List<SpecificationValueInfo>(),
                AttributeInfo = new List<AttributeInfo>(),
                TypeBrandInfo = new List<TypeBrandInfo>()
            };

            #region 规格
            if (m.IsSupportColor && (!string.IsNullOrWhiteSpace(m.ColorValue)))
            {
                info.IsSupportColor = m.IsSupportColor;
                m.ColorValue = m.ColorValue.Replace("，", ",");
                var colorArray = m.ColorValue.Split(',');
                foreach (var item in colorArray)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    info.SpecificationValueInfo.Add(new SpecificationValueInfo
                    {
                        Specification = SpecificationType.Color,
                        Value = item,
                        TypeId = m.Id
                    });
                }

            }
            if (m.IsSupportSize && (!string.IsNullOrWhiteSpace(m.SizeValue)))
            {
                info.IsSupportSize = m.IsSupportSize;
                m.SizeValue = m.SizeValue.Replace("，",",");
                var sizeArray = m.SizeValue.Split(',');
                foreach (var item in sizeArray)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    info.SpecificationValueInfo.Add(new SpecificationValueInfo
                    {
                        Specification = SpecificationType.Size,
                        Value = item,
                        TypeId = m.Id
                    });
                }
            }
            if (m.IsSupportVersion && (!string.IsNullOrWhiteSpace(m.VersionValue)))
            {
                info.IsSupportVersion = m.IsSupportVersion;
                m.VersionValue = m.VersionValue.Replace("，", ",");
                var versionArray = m.VersionValue.Split(',');
                foreach (var item in versionArray)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    info.SpecificationValueInfo.Add(new SpecificationValueInfo
                    {
                        Specification = SpecificationType.Version,
                        Value = item,
                        TypeId = m.Id
                    });
                }
            }
            #endregion

            #region 品牌

            if (null != m.Brands && m.Brands.Count() > 0)
            {
                foreach (var item in m.Brands)
                {
                    info.TypeBrandInfo.Add(new TypeBrandInfo()
                    {
                        BrandId = item.Id,
                        TypeId = m.Id
                    });
                }
            }

            #endregion

            #region 属性
            if (null != m.Attributes && m.Attributes.Count() > 0)
            {
                foreach (var item in m.Attributes)
                {
                    var attr = new AttributeInfo
                    {
                        IsMulti = item.IsMulti,
                        Name = item.Name,
                        AttributeValueInfo = new List<AttributeValueInfo>(),
                        TypeId = m.Id,
                        Id = item.Id
                    };
                    item.Value = string.IsNullOrWhiteSpace(item.Value)?"":item.Value.Replace("，", ",");
                    var values = item.Value.Split(',');
                    foreach (var v in values)
                    {
                        attr.AttributeValueInfo.Add(new AttributeValueInfo { Value = v, DisplaySequence = 1 });
                    }
                    info.AttributeInfo.Add(attr);
                }
            }
            #endregion

            return info;
        }

    }

    public class BrandInfoModel
    {
        public long Id { get; set; }
    }
    public class TypeAttribute
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsMulti { get; set; }
        public long TypeId { get; set; }
    }
}