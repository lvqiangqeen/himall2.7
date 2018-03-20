using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.IServices;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Model;


namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class CustomerServiceController : BaseSellerController
    {
        private ICustomerService _iCustomerService;
        public CustomerServiceController(ICustomerService iCustomerService)
        {
            _iCustomerService = iCustomerService;
        }

        // GET: SellerAdmin/CustomerService
        public ActionResult Management()
        {
            var customerServices = _iCustomerService.GetCustomerService(CurrentSellerManager.ShopId).OrderByDescending(item=>item.Id).ToArray();
			var model = new CustomerServiceManagementViewModel();
			model.CustomerServices = customerServices.Select(
				item => new CustomerServiceModel()
				{
					Id = item.Id,
					Account = item.AccountCode,
					Name = item.Name,
					Tool = item.Tool,
					Type = item.Type
				}).ToList();

            var mobileService=_iCustomerService.GetCustomerServiceForMobile(CurrentSellerManager.ShopId);

            var hasMobileService = mobileService != null ? true : false;
			model.HasMobileService = hasMobileService;
			model.MobileService = mobileService;
			return View(model);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Delete(long id)
        {
            _iCustomerService.DeleteCustomerService(CurrentSellerManager.ShopId, id);
            return Json(new { success = true });
        }

        public ActionResult Add(long? id)
        {
            var service = _iCustomerService;
            CustomerServiceInfo customerServiceInfo;
            if (id.HasValue && id > 0)
                customerServiceInfo = service.GetCustomerService(CurrentSellerManager.ShopId, id.Value);
            else
                customerServiceInfo = new CustomerServiceInfo();

            var customerServiceModels = new CustomerServiceModel()
               {
                   Id = customerServiceInfo.Id,
                   Account = customerServiceInfo.AccountCode,
                   Name = customerServiceInfo.Name,
                   Tool = customerServiceInfo.Tool,
                   Type = customerServiceInfo.Type
               };
            return View(customerServiceModels);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Add(CustomerServiceModel customerServiceModel)
        {
            var service = _iCustomerService;
            CustomerServiceInfo customerServiceInfo = new CustomerServiceInfo()
            {
                Id = customerServiceModel.Id,
                Type = customerServiceModel.Type.GetValueOrDefault(CustomerServiceInfo.ServiceType.PreSale),
                Tool = customerServiceModel.Tool,
                Name = customerServiceModel.Name,
                AccountCode = customerServiceModel.Account,
                ShopId = CurrentSellerManager.ShopId,
                TerminalType = Himall.Model.CustomerServiceInfo.ServiceTerminalType.PC,
                ServerStatus = Himall.Model.CustomerServiceInfo.ServiceStatusType.Open
            };

            if (customerServiceInfo.Id > 0)
                service.UpdateCustomerService(customerServiceInfo);
            else
                service.AddCustomerService(customerServiceInfo);

            return Json(new { success = true });

        }

        public ActionResult addMobile()
        {
            var service = _iCustomerService;
            CustomerServiceInfo customerServiceInfo;
            customerServiceInfo = service.GetCustomerServiceForMobile(CurrentSellerManager.ShopId);
            if (customerServiceInfo == null)
                customerServiceInfo = new CustomerServiceInfo();
            var customerServiceModels = new CustomerServiceModel()
            {
                Id = customerServiceInfo.Id,
                Account = customerServiceInfo.AccountCode,
                Name = customerServiceInfo.Name,
                Tool = customerServiceInfo.Tool,
                Type = customerServiceInfo.Type
            };
            return View(customerServiceModels);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult addMobile(CustomerServiceModel customerServiceModel)
        {
            var service = _iCustomerService;
            CustomerServiceInfo customerServiceInfo = new CustomerServiceInfo()
            {
                Id = customerServiceModel.Id,
                Type = customerServiceModel.Type.GetValueOrDefault(CustomerServiceInfo.ServiceType.PreSale),
                Tool = CustomerServiceInfo.ServiceTool.QQ,
                Name = customerServiceModel.Name,
                AccountCode = customerServiceModel.Account,
                ShopId = CurrentSellerManager.ShopId,
                TerminalType = Himall.Model.CustomerServiceInfo.ServiceTerminalType.Mobile,
                ServerStatus = Himall.Model.CustomerServiceInfo.ServiceStatusType.Open
            };

            if (customerServiceInfo.Id > 0)
                service.UpdateCustomerService(customerServiceInfo);
            else
                service.AddCustomerService(customerServiceInfo);

            return Json(new { success = true });
        }

		public ActionResult AddMeiQia(long? id)
		{
			return Add(id);
		}

        [HttpPost]
        [UnAuthorize]
        public JsonResult deleteMobile()
        {
            _iCustomerService.DeleteCustomerServiceForMobile(CurrentSellerManager.ShopId);
            return Json(new { success = true });
        }
    }
}