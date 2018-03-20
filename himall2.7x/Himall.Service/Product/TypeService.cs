
using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using Himall.CommonModel;
using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;

namespace Himall.Service
{

    /// <summary>
    /// 属性比较器
    /// </summary>
    public class AttrComparer : IEqualityComparer<AttributeInfo>
    {
        public bool Equals(AttributeInfo x, AttributeInfo y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id && x.Id != 0 && y.Id != 0;
        }
        public int GetHashCode(AttributeInfo attr)
        {
            if (Object.ReferenceEquals(attr, null)) return 0;
            int Id = (int)(attr.Id ^ attr.TypeId);
            return Id;
        }
    }

    public class AttrValueComparer : IEqualityComparer<AttributeValueInfo>
    {
        public bool Equals(AttributeValueInfo x, AttributeValueInfo y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Value == y.Value;
        }
        public int GetHashCode(AttributeValueInfo attr)
        {
            if (Object.ReferenceEquals(attr, null)) return 0;
            int Id = attr.Value.GetHashCode();
            return Id;
        }
    }

    public class SpecValueComparer : IEqualityComparer<SpecificationValueInfo>
    {
        public bool Equals(SpecificationValueInfo x, SpecificationValueInfo y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Value == y.Value;
        }
        public int GetHashCode(SpecificationValueInfo spec)
        {
            if (Object.ReferenceEquals(spec, null)) return 0;
            int Id = spec.Value.GetHashCode();
            return Id;
        }
    }

    public class TypeService : ServiceBase, ITypeService
    {
        public ProductTypeInfo GetType(long id)
        {
			return Context.ProductTypeInfo.FirstOrDefault(p => p.Id == id && p.IsDeleted == false);
        }

        public ProductTypeInfo GetTypeByProductId(long productId)
        {
            return (ProductTypeInfo)Context.ProductTypeInfo.Join(Context.ProductInfo.Where(d => d.Id == productId), x => x.Id, y => y.TypeId, (x, y) => x).ToList().FirstOrDefault();
        }

        /// <summary>
        /// 采取遍历列表更新的策略
        /// </summary>
        /// <param name="model"></param>
        private void ProcessingAttr(ProductTypeInfo model)
        {
            ProcessingAttrDeleteAndAdd(model);
            ProcessingAttrUpdate(model);
        }

        /// <summary>
        /// 更新属性，不会删除属性，只会删除和添加属性值
        /// </summary>
        /// <param name="model"></param>
        private void ProcessingAttrUpdate(ProductTypeInfo model)
        {
            var actualModel = Context.ProductTypeInfo.FindById(model.Id);
            foreach (var item in model.AttributeInfo.ToList())
            {
                if (actualModel.AttributeInfo.Any(a => a.Id.Equals(item.Id)&&item.Id!=0))
                {
                    var actualAttr = actualModel.AttributeInfo.FirstOrDefault(a => a.Id.Equals(item.Id));
                    actualAttr.Name = item.Name;
                    actualAttr.IsMulti = item.IsMulti;

                    //将model中删除的属性值
                    var deleteV = actualAttr.AttributeValueInfo.Except(item.AttributeValueInfo, new AttrValueComparer());
                    if (null != deleteV && 0 < deleteV.Count())
                    {
                        foreach (var attrV in deleteV.ToList())
                        {
                            Context.AttributeValueInfo.Remove(attrV);
                        }
                    }

                    //将model中的新增的属性值添加到DB
                    var addV = item.AttributeValueInfo.Except(actualAttr.AttributeValueInfo, new AttrValueComparer());
                    if (null != addV && 0 < addV.Count())
                    {
                        foreach (var attrV in addV.ToList())
                        {
                            attrV.AttributeId = item.Id;
                            Context.AttributeValueInfo.Add(attrV);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 处理属性的添加、删除操作
        /// </summary>
        /// <param name="model"></param>
        private void ProcessingAttrDeleteAndAdd(ProductTypeInfo model)
        {
            var actualModel = Context.ProductTypeInfo.FindById(model.Id);
            //需要删除的属性
            var deleteAttr = actualModel.AttributeInfo.Except(model.AttributeInfo, new AttrComparer()).ToList();
            foreach (var attr in deleteAttr)
            {
                var attrValues = Context.AttributeValueInfo.FindBy(a => a.AttributeId.Equals(attr.Id));
                foreach (var attrV in attrValues.ToList())
                {
                    Context.AttributeValueInfo.Remove(attrV);
                }
                Context.AttributeInfo.Remove(attr);
            }


            //从前台添加的新的属性
            //需要添加的属性
            var addNewAttr = model.AttributeInfo.Except(actualModel.AttributeInfo, new AttrComparer()).ToList();
            if (null != addNewAttr && addNewAttr.Count() > 0)
            {
                foreach (var item in addNewAttr)
                {
                    Context.AttributeInfo.Add(item);
                }
            }
        }

        /// <summary>
        /// 采取全部删除，再添加的策略
        /// </summary>
        /// <param name="model"></param>
        private void ProcessingBrand(ProductTypeInfo model)
        {
            var actualModel = Context.ProductTypeInfo.FindById(model.Id);
            //删除之前关联的品牌
            foreach (var brand in actualModel.TypeBrandInfo.ToList())
            {
                Context.TypeBrandInfo.Remove(brand);
            }
            foreach (var item in model.TypeBrandInfo)
            {
                Context.TypeBrandInfo.Add(item);
            }
        }

        private void UpdateSpecificationValues(ProductTypeInfo model, SpecificationType specEnum)
        {
            var newSpec = model.SpecificationValueInfo.Where(s => s.Specification == specEnum);
            var actual = Context.SpecificationValueInfo
                                .Where(s => s.TypeId.Equals(model.Id) && s.Specification == specEnum).AsEnumerable();

            var deleteSpec = actual.Except(newSpec, new SpecValueComparer());
            foreach (var specV in deleteSpec.ToList())
            {
                Context.SpecificationValueInfo.Remove(specV);
            }


            var addSpec = newSpec.Except(actual, new SpecValueComparer());
            foreach (var specV in addSpec.ToList())
            {
                Context.SpecificationValueInfo.Add(specV);
            }


        }

        /// <summary>
        /// 采取更新的策略
        /// </summary>
        /// <param name="model"></param>
        private void ProcessingSpecificationValues(ProductTypeInfo model)
        {
            /*规定Specification的值（0：颜色,  1：尺寸,  2：版本）*/

            var _model = Context.ProductTypeInfo.FindById(model.Id);
            //颜色
            if (model.IsSupportColor)
            {
                UpdateSpecificationValues(model, SpecificationType.Color);
                _model.ColorAlias = model.ColorAlias;
            }

            //尺寸
            if (model.IsSupportSize)
            {
                UpdateSpecificationValues(model, SpecificationType.Size);
                _model.SizeAlias = model.SizeAlias;
            }

            //版本
            if (model.IsSupportVersion)
            {
                UpdateSpecificationValues(model, SpecificationType.Version);
                _model.VersionAlias = model.VersionAlias;
            }

            _model.IsSupportVersion = model.IsSupportVersion;
            _model.IsSupportColor = model.IsSupportColor;
            _model.IsSupportSize = model.IsSupportSize;
        }

        private void ProcessingCommon(ProductTypeInfo model)
        {
            var actual = Context.ProductTypeInfo.FindById(model.Id);
            actual.Name = model.Name;
        }

        public void UpdateType(ProductTypeInfo model)
        {

            //更新品牌
            ProcessingBrand(model);

            //更新的属性、属性值
            ProcessingAttr(model);

            //更新规格
            ProcessingSpecificationValues(model);

            //更新常用属性
            ProcessingCommon(model);

            //提交保存
            Context.SaveChanges();

            Cache.Remove(CacheKeyCollection.CACHE_ATTRIBUTE_LIST);
            Cache.Remove(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST);
        }

        public void DeleteType(long id)
        {
			var existCategory=this.Context.CategoryInfo.Exist(p => p.TypeId == id && p.IsDeleted == false);

			if (existCategory)
				throw new HimallException("该类型已经有分类关联，不能删除.");

			//this.Context.ProductTypeInfo.Where(p => p.Id == id).Update(p => new ProductTypeInfo { IsDeleted = true });
			var sql = "UPDATE Himall_Types SET IsDeleted=@p1 WHERE Id=@p0";
			this.Context.Database.ExecuteSqlCommand(sql, id, true);
        }

        public void AddType(ProductTypeInfo model)
        {
            Context.ProductTypeInfo.Add(model);
            Context.SaveChanges();
        }

        public IQueryable<ProductTypeInfo> GetTypes()
        {
			return Context.ProductTypeInfo.Where(p => p.IsDeleted == false);
        }

        public QueryPageModel<ProductTypeInfo> GetTypes(string search, int pageNo, int pageSize)
        {
            int count = 0;

			var list = Context.ProductTypeInfo.Where(p => p.IsDeleted == false);
			if (!string.IsNullOrWhiteSpace(search))
				list = list.Where(p => p.Name.Contains(search));
			list=list.GetPage(out count, p => p.OrderByDescending(pp => pp.Id), pageNo, pageSize);

			var result = new QueryPageModel<ProductTypeInfo>()
			{
				Total = count,
				Models = list.ToList()
			};

            return result;

        }
    }
}
