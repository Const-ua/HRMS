using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Dish
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int SubcategoryId { get; set; }
        public string Name { get; set; }
        public int CatererId { get; set; }
        public string Description{ get; set; }
        public byte[] Photo { get; set; }
        public int Price { get; set; }

        public MenuCategory Category { get; set; }
        public MenuSubcategory Subcategory { get; set; }
        public Profile Caterer { get; set; }

     }
}