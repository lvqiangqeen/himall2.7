using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins.Message;
using Himall.Entity;
using Himall.IServices;
using Himall.IServices.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class SensitiveWordService : ServiceBase, ISensitiveWordService
    {
        public ObsoletePageModel<SensitiveWordsInfo> GetSensitiveWords(SensitiveWordQuery query)
        {
            int total = 0;
            IQueryable<SensitiveWordsInfo> Infos = Context.SensitiveWordsInfo.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.SensitiveWord))
            {
                Infos = Infos.Where(item => item.SensitiveWord.Contains(query.SensitiveWord));
            }
            if (!string.IsNullOrWhiteSpace(query.CategoryName))
            {
                Infos = Infos.Where(item => item.CategoryName.Contains(query.CategoryName));
            }
            Infos = Infos.GetPage(out total, query.PageNo, query.PageSize);

            ObsoletePageModel<SensitiveWordsInfo> pageModel = new ObsoletePageModel<SensitiveWordsInfo>() { Models = Infos, Total = total };
            return pageModel;
        }

        public IEnumerable<string> GetCategories()
        {
            var categories = Context.SensitiveWordsInfo.Select(item => item.CategoryName).Distinct();
            return categories;
        }

        public SensitiveWordsInfo GetSensitiveWord(int id)
        {
            return Context.SensitiveWordsInfo.FindById(id);
        }

        public void AddSensitiveWord(SensitiveWordsInfo model)
        {
            Context.SensitiveWordsInfo.Add(model);
            Context.SaveChanges();
        }

        public void UpdateSensitiveWord(SensitiveWordsInfo model)
        {
            SensitiveWordsInfo item = GetSensitiveWord(model.Id);
            item.SensitiveWord = model.SensitiveWord;
            item.CategoryName = model.CategoryName;
            Context.SaveChanges();
        }

        public void DeleteSensitiveWord(int id)
        {
            var model = Context.SensitiveWordsInfo.FindById(id);
            Context.SensitiveWordsInfo.Remove(model);
            Context.SaveChanges();
        }

        public void BatchDeleteSensitiveWord(int[] ids)
        {
            var model = Context.SensitiveWordsInfo.FindBy(item => ids.Contains(item.Id));
            Context.SensitiveWordsInfo.RemoveRange(model);
            Context.SaveChanges();
        }

        public bool ExistSensitiveWord(string word)
        {
            var data = Context.SensitiveWordsInfo.Where(item => item.SensitiveWord.Trim().ToLower() == word.Trim().ToLower());
            if (data.Count() > 0)
                return true;
            else
                return false;
        }
    }
}
