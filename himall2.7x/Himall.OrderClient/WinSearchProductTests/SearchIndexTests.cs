using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinSearchProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSearchProduct.Tests
{
    [TestClass()]
    public class SearchIndexTests
    {
        [TestMethod()]
        public void SyncDataTest()
        {
            IProductIndex index = new SearchIndex();
            index.CreateIndex();
        }

        [TestMethod()]
        public void Empty()
        {
            IProductIndex index = new SearchIndex();
            index.EmptyIndex();
        }
    }
}