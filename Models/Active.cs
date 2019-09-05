using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Active
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public string Name { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string InvNumber { get; set; }
        public string SerNo { get; set; }
        public string Comments { get; set; }
        public int GoodsId { get; set; }
        public decimal Price { get; set; }

        public Goods Goods { get; set; }
        public Profile Profile { get; set; }
    }
}