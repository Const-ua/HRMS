using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class MenuCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<MenuSubcategory> SubCategories { get; set; }

    }
}