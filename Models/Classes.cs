using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class MyOrders 
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Sum { get; set; }

    }
}