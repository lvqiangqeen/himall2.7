using Himall.Model;
using System;

namespace Himall.IServices.QueryModel
{
    public partial class SensitiveWordQuery : QueryBase
    {
        public int Id { get; set; }

        public string SensitiveWord { get; set; }

        public string CategoryName { get; set; }
    }
}
