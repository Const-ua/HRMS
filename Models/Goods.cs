using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Goods
    {
        public int Id { get; set; }
        public string GoodsName { get; set; }
        public List<Active> Actives { get; set; }
    }
}