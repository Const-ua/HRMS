using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int ProfileId { get; set; }
        public int Sum { get; set; }
        public int Status { get; set; }

        public Profile Profile { get; set; }
        public List<OrderDetail> GetOrderDetails { get; set; }
    }
}