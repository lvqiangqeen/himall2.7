using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using System.IO;
using EntityFramework.Extensions;
using Himall.CommonModel;

namespace Himall.Service
{
	public class BrandService : ServiceBase, IBrandService
	{
		public void AddBrand(Model.BrandInfo model)
		{
			Context.BrandInfo.Add(model);
			Context.SaveChanges();
			//转移图片
			model.Logo = MoveImages(model.Id, model.Logo);
			Context.SaveChanges();
		}

		public void UpdateBrand(Model.BrandInfo model)
		{
			model.Logo = MoveImages(model.Id, model.Logo);
			BrandInfo brand = GetBrand(model.Id);
			brand.Name = model.Name.Trim();
			brand.Description = model.Description;
			brand.Logo = model.Logo;
			brand.Meta_Description = model.Meta_Description;
			brand.Meta_Keywords = model.Meta_Keywords;
			brand.Meta_Title = model.Meta_Title;
			brand.RewriteName = model.RewriteName;
			brand.IsRecommend = model.IsRecommend;

			ShopBrandApplysInfo info = Context.ShopBrandApplysInfo.FirstOrDefault(p => p.BrandId == model.Id);
			if (info != null)
			{
				info.BrandName = model.Name;
				info.Description = model.Description;
				info.Logo = model.Logo;
			}
			Context.SaveChanges();


		}

		public void DeleteBrand(long id)
		{
			//this.Context.BrandInfo.Where(p => p.Id == id).Update(p => new BrandInfo { IsDeleted = true });
			var brand=this.Context.BrandInfo.FirstOrDefault(p => p.Id == id);
			if (brand != null)
			{
				brand.IsDeleted = true;
				this.Context.SaveChanges();
			}
		}

		public QueryPageModel<BrandInfo> GetBrands(string keyWords, int pageNo, int pageSize)
		{
			int total = 0;
			var brands = Context.BrandInfo.Where(p => p.IsDeleted == false);
			if (!string.IsNullOrWhiteSpace(keyWords))
				brands = brands.Where(p => p.Name.Contains(keyWords));
			brands = brands.GetPage(out total, pageNo, pageSize, p => p.OrderByDescending(pp => pp.Id));

			var pageModel = new QueryPageModel<BrandInfo>()
			{
				Models = brands.ToList(),
				Total = total
			};
			return pageModel;
		}

		public void AuditBrand(long id, Himall.Model.ShopBrandApplysInfo.BrandAuditStatus status)
		{
			var m = Context.ShopBrandApplysInfo.FindById(id);
			m.AuditStatus = (int)status;
			if (status == Himall.Model.ShopBrandApplysInfo.BrandAuditStatus.Audited) //审核通过
			{
				if (m.ApplyMode == (int)Himall.Model.ShopBrandApplysInfo.BrandApplyMode.New) //申请的是新品牌
				{
                    var model = Context.BrandInfo.Where(r => r.Name.ToLower() == m.BrandName.ToLower() && r.IsDeleted == false).FirstOrDefault();
					if (model == null) //是否已存在该品牌
					{
						//向品牌表里加入一条数据
						BrandInfo brand = new BrandInfo()
						{
							Name = m.BrandName.Trim(),
							Logo = m.Logo,
							Description = m.Description
						};

						Context.BrandInfo.Add(brand);
						Context.SaveChanges();

						//关联申请表与品牌表的联系
						m.BrandId = brand.Id;

						BrandInfo b = GetBrand(brand.Id);
						b.Logo = MoveImages(b.Id, b.Logo, 1);

						//向商家品牌表加入一条数据 
						ShopBrandsInfo info = new ShopBrandsInfo();
						info.BrandId = b.Id;
						info.ShopId = m.ShopId;

						Context.ShopBrandsInfo.Add(info);
						Context.SaveChanges();
					}
					else
					{
						//向商家品牌表加入一条数据 
						ShopBrandsInfo info = new ShopBrandsInfo();
						info.BrandId = model.Id;
						info.ShopId = m.ShopId;

						Context.ShopBrandsInfo.Add(info);
						Context.SaveChanges();
					}
				}
				else
				{
					//向商家品牌表加入一条数据 
					ShopBrandsInfo info = new ShopBrandsInfo();
					info.BrandId = (long)m.BrandId;
					info.ShopId = m.ShopId;

					Context.ShopBrandsInfo.Add(info);
					Context.SaveChanges();
				}
			}
			Context.SaveChanges();
		}

		public List<BrandInfo> GetBrands(string keyWords)
		{
			var result = this.Context.BrandInfo.Where(p => p.IsDeleted == false);
			if (!string.IsNullOrWhiteSpace(keyWords))
				result = result.Where(p => p.Name.Contains(keyWords) || p.RewriteName.Contains(keyWords));
			return result.ToList();
		}

		/// <summary>
		/// 根据品牌id获取品牌
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public List<BrandInfo> GetBrandsByIds(IEnumerable<long> ids)
		{
			return this.Context.BrandInfo.Where(p => ids.Contains(p.Id) && p.IsDeleted == false).ToList();
		}

		public BrandInfo GetBrand(long id)
		{
			return Context.BrandInfo.FirstOrDefault(p => p.Id == id && p.IsDeleted == false);
		}

		string MoveImages(long brandId, string image, int type = 0)
		{
			if (string.IsNullOrEmpty(image))
			{
				return "";
			}
			var ext = image.Substring(image.LastIndexOf("."));
			string relativeDir = "/Storage/Plat/Brand/";
			string fileName = "logo_" + brandId + ext;
			if (image.Contains("/temp/"))
			{
				string path = image.Substring(image.LastIndexOf("/temp"));
				Core.HimallIO.CopyFile(path, relativeDir + fileName, true);
			}
			else if (type == 1)
			{
				string path = image;
				Core.HimallIO.CopyFile(path, relativeDir + fileName, true);
			}
			return relativeDir + fileName;
		}

		string MoveImages(int id, long shopId, string image, string name, int index = 1)
		{
			if (string.IsNullOrEmpty(image))
			{
				return "";
			}
			var ext = ".png";
			string ImageDir = string.Empty;

			var path = "/Storage/Shop/" + shopId + "/Brand/";
			var fileName = name + "_" + id + "_" + index + ext;
			if (image.Contains("/temp/"))
			{
				string temp = image.Substring(image.LastIndexOf("/temp"));
				Core.HimallIO.CopyFile(temp, path + fileName, true);
			}
			return path + fileName;
		}

		public IEnumerable<BrandInfo> GetBrandsByCategoryIds(params long[] categoryIds)
		{
			var typeIds = this.Context.CategoryInfo.Where(p => categoryIds.Contains(p.Id) && p.IsDeleted == false && p.ProductTypeInfo.IsDeleted == false).Select(p => p.TypeId);
			var brandIds = this.Context.TypeBrandInfo.Where(p => typeIds.Contains(p.TypeId)).Select(p => p.BrandId);
			var select=this.Context.BrandInfo.Where(p => brandIds.Contains(p.Id) && p.IsDeleted == false);
			var sql = select.ToString();
			var result= select.ToList();

			return result;
		}

		public IEnumerable<BrandInfo> GetBrandsByCategoryIds(long shopId, params long[] categoryIds)
		{
			var isSelfShop = Context.ShopInfo.Exist(p => p.Id == shopId && p.IsSelf);

			var typeIds = this.Context.CategoryInfo.Where(p => categoryIds.Contains(p.Id) && p.IsDeleted == false && p.ProductTypeInfo.IsDeleted == false).Select(p => p.TypeId);
			var typeBrands = this.Context.TypeBrandInfo.Where(p => typeIds.Contains(p.TypeId));

			if (!isSelfShop)//平台店查询所有的
				typeBrands = typeBrands.Where(p => this.Context.ShopBrandsInfo.Any(sb => sb.ShopId == shopId && sb.BrandId == p.BrandId));

			var brandIds = typeBrands.Select(p => p.BrandId);
			var select = this.Context.BrandInfo.Where(p => brandIds.Contains(p.Id) && p.IsDeleted == false);
			var sql = select.ToString();
			var result = select.ToList();

			return result;
		}

		public void ApplyBrand(ShopBrandApplysInfo model)
		{
			Context.ShopBrandApplysInfo.Add(model);
			Context.SaveChanges();

			//移动品牌Logo
			if (model.ApplyMode == 2)
			{
				model.Logo = MoveImages(model.Id, model.ShopId, model.Logo, "logo", 1);
			}
			//移动品牌授权证书
			var pics = model.AuthCertificate;
			string newpics = string.Empty;
			if (!string.IsNullOrEmpty(pics))
			{
				var arr = pics.Split(',');
				var index = 0;
				foreach (var image in arr)
				{
					index++;
					newpics += MoveImages(model.Id, model.ShopId, image, "auth", index) + ",";
				}
			}
			if (!string.IsNullOrEmpty(newpics))
			{
				model.AuthCertificate = newpics.TrimEnd(',');
			}
			Context.SaveChanges();
		}

		public bool IsExistApply(long shopId, string brandName)
		{
			var apply = Context.ShopBrandApplysInfo.Where(item => item.ShopId == shopId && item.BrandName.ToLower().Trim() == brandName.ToLower().Trim() && item.AuditStatus != (int)ShopBrandApplysInfo.BrandAuditStatus.Refused).FirstOrDefault();
			if (apply != null)
				return true;
			else
				return false;
		}

		public bool IsExistBrand(string brandName)
		{
            var brand = Context.BrandInfo.Where(item => item.Name.ToLower().Trim() == brandName.ToLower().Trim() && item.IsDeleted == false).FirstOrDefault();
			if (brand != null)
				return true;
			else
				return false;
		}

		public void UpdateSellerBrand(ShopBrandApplysInfo model)
		{
			var m = Context.ShopBrandApplysInfo.FindBy(a => a.Id == model.Id && a.ShopId != 0 && a.AuditStatus == (int)ShopBrandApplysInfo.BrandAuditStatus.UnAudit).FirstOrDefault();
			if (m == null)
				throw new Himall.Core.HimallException("该品牌已被审核或删除，不能修改！");
			m.Logo = MoveImages(model.Id, model.Logo);
			m.BrandName = model.BrandName;
			m.Description = model.Description;
			Context.SaveChanges();
		}

		public QueryPageModel<BrandInfo> GetShopBrands(long shopId, int pageNo, int pageSize)
		{
			int total = 0;
			var brands = Context.BrandInfo.FindBy(item =>
				Context.ShopBrandsInfo.Exist(r => r.ShopId == shopId && r.Himall_Brands.IsDeleted == false && r.BrandId == item.Id) && item.IsDeleted == false, pageNo, pageSize, out total, a => a.Id, false);

			var pageModel = new QueryPageModel<BrandInfo>()
			{
				Models = brands.ToList(),
				Total = total
			};
			return pageModel;
		}

		public IQueryable<BrandInfo> GetShopBrands(long shopId)
		{
			IQueryable<BrandInfo> brands = Context.BrandInfo
				.Where(item => Context.ShopBrandsInfo.Exist(r => r.ShopId == shopId && r.BrandId == item.Id) && item.IsDeleted == false);

			return brands;
		}

		public ObsoletePageModel<ShopBrandApplysInfo> GetShopBrandApplys(long? shopId, int? auditStatus, int pageNo, int pageSize, string keyWords)
		{
			int total = 0;
			IQueryable<ShopBrandApplysInfo> brands = Context.ShopBrandApplysInfo.FindAll();

			if (auditStatus.HasValue)
			{
				brands = brands.Where(item => item.AuditStatus == (int)auditStatus);
			}
            if (shopId.HasValue)
            {
                brands = brands.Where(item => item.ShopId == shopId && item.Himall_Brands.IsDeleted == false);
            }
            if (!string.IsNullOrEmpty(keyWords))
            {
                brands = brands.Where(item => item.Himall_Shops.ShopName.Contains(keyWords));
            }
            brands = brands.FindBy(item => true, pageNo, pageSize, out total, a => a.Id, false);
			//if (auditStatus != null)
			//{
			//    brands = brands.FindBy(item => item.AuditStatus == (int)auditStatus, pageNo, pageSize, out total, a => a.Id, false);
			//}
			//if (shopId != null)ww
			//{
			//    if (!string.IsNullOrEmpty(keyWords))
			//    {
			//        brands = brands.FindBy(item => item.ShopId == shopId && item.Himall_Shops.ShopName.Contains(keyWords), pageNo, pageSize, out total, a => a.Id, false);
			//    }
			//    else
			//    {
			//        brands = brands.FindBy(item => item.ShopId == shopId, pageNo, pageSize, out total, a => a.Id, false);
			//    }
			//}
			//else
			//{
			//    if (!string.IsNullOrEmpty(keyWords))
			//    {
			//        brands = brands.FindBy(item => item.Himall_Shops.ShopName.Contains(keyWords), pageNo, pageSize, out total, a => a.Id, false);
			//    }
			//}

			ObsoletePageModel<ShopBrandApplysInfo> pageModel = new ObsoletePageModel<ShopBrandApplysInfo>() { Models = brands, Total = total };
			return pageModel;
		}

		public IQueryable<ShopBrandApplysInfo> GetShopBrandApplys(long shopId)
		{
			IQueryable<ShopBrandApplysInfo> brands = Context.ShopBrandApplysInfo.FindBy(item => item.ShopId == shopId);
			return brands;
		}

		public ShopBrandApplysInfo GetBrandApply(long id)
		{
			var model = Context.ShopBrandApplysInfo.FindById(id);
			if (model.ApplyMode == (int)ShopBrandApplysInfo.BrandApplyMode.Exist)
			{
				model.Description = model.Himall_Brands.Description;
			}
			return model;
		}

		public void DeleteApply(int id)
		{
			var model = Context.ShopBrandApplysInfo.FindById(id);
			Context.ShopBrandApplysInfo.Remove(model);
			//删除对应图片
			if (model.ApplyMode == (int)ShopBrandApplysInfo.BrandApplyMode.New)
			{
				string logosrc = model.Logo;
				if (Core.HimallIO.ExistFile(logosrc))
					Core.HimallIO.DeleteFile(logosrc);
			}
			string imgsrc = model.AuthCertificate;
			if (!string.IsNullOrWhiteSpace(imgsrc))
			{
				try
				{
					if (Core.HimallIO.ExistFile(imgsrc))
						Core.HimallIO.DeleteFile(imgsrc);
				}
				catch (Exception ex)
				{
					Log.Info("删除图片失败" + ex.Message);
				}
			}
			Context.SaveChanges();
		}

		public bool BrandInUse(long id)
		{
			bool flag = Context.ProductInfo.Any(item => item.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && item.IsDeleted == false && item.BrandId.Equals(id));
			return flag;
		}
	}
}

