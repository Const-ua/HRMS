using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class MenuSubcategory
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public MenuCategory Category { get; set; }
        public List<Dish> Dishes { get; set; }
    }
}