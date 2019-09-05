using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Hrms.Models
{
    public class HRDbContext : DbContext
    {
        public DbSet<Active> Actives { get; set; }
        public DbSet<Aspirant> Aspirants { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Role> Roles { get; set; }
   
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Goods> Goods { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<Result> Results { get; set; }

        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<MenuSubcategory> MenuSubcategories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Goal> Goals { get; set; }

    }
}