using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Hrms.Models;


namespace Hrms.Controllers
{
    public class CatererController : Controller
    {
        private HRDbContext db = new HRDbContext();

        private List<string> ControllerRoles = new List<string>() { "Кейтерер", "Администратор" };
        static DateTime CurrentDate = DateTime.Now;

        // GET: Caterer
        public ActionResult EditMenu(int? subcategory)
        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");
            if (subcategory == null)
                subcategory = 0;
            else
                subcategory = int.Parse(subcategory.ToString());

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            List<MenuCategory> menuCategories = db.MenuCategories.OrderBy(r => r.Id).ToList();
            ViewBag.menuCategories = menuCategories;
            List<MenuSubcategory> menuSubcategories = db.MenuSubcategories.OrderBy(r => r.CategoryId).ThenBy(r => r.Name)
                                                    .Include(r => r.Category).ToList();
            ViewBag.menuSubcategories = menuSubcategories;
            List<Profile> users = db.Profiles.Include(u => u.Role).ToList();
            List<Dish> dishes = new List<Dish>();
            if (subcategory == 0)
            {
                dishes = db.Dishes.Include(r => r.Category).Include(r => r.Subcategory).OrderBy(r => r.Name)
                                  .Where(r => r.CatererId==userId).ToList();
                ViewBag.CurrentSubcategory = "Все меню";
            }
            else
            {
                dishes = db.Dishes.Include(r => r.Category).Include(r => r.Subcategory).OrderBy(r => r.Name)
                    .Where(r => r.SubcategoryId == subcategory && r.CatererId == userId).ToList();
                MenuSubcategory sc = menuSubcategories.FirstOrDefault(r => r.Id ==subcategory);
                ViewBag.CurrentSubcategory = sc.Category.Name+" "+ sc.Name;
            }
            
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("EditMenu", dishes);

            }
            return RedirectToAction("Index", "Home");

        }

        public FileContentResult GetPhoto(int id)
        {
            Dish dish = db.Dishes.FirstOrDefault(p => p.Id == id);
            if (dish != null)
                if (dish.Photo!=null)
                   return File(dish.Photo, "image/jpeg");
           return null;
        }

        public ActionResult AddDish()
        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");

            List<MenuCategory> menuCategories = db.MenuCategories.OrderBy(r => r.Id).ToList();
            ViewBag.menuCategories = menuCategories;
            int firstCategory = menuCategories[0].Id;
            List<MenuSubcategory> menuSubcategories = db.MenuSubcategories.OrderBy(r => r.CategoryId).ThenBy(r => r.Name)
                                                    .Where(r => r.CategoryId == firstCategory)
                                                    .Include(r => r.Category)
                                                    .ToList();
            ViewBag.menuSubcategories = menuSubcategories;
            List<Profile> users = db.Profiles.Include(u => u.Role).ToList();
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);

            Dish dish = new Dish();
            dish.Id = 0;
            dish.CategoryId = 0;
            dish.SubcategoryId = 0;
            dish.CatererId = userId;            
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("EditDish", dish);
            }
            return RedirectToAction("EditMenu", "Caterer");
        }

        public ActionResult EditDish(int? id)
        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");

            if (id == null)
                return RedirectToAction("EditMenu", "Caterer", new { id = id });
            else
                id = int.Parse(id.ToString());
            List<Profile> users = db.Profiles.Include(u => u.Role).ToList();

            Dish dish = db.Dishes.Find(id);
            if (dish==null)
                return RedirectToAction("EditMenu", "Caterer");
            List<MenuCategory> menuCategories = db.MenuCategories.OrderBy(r => r.Id).ToList();
            ViewBag.menuCategories = menuCategories;
            List<MenuSubcategory> menuSubcategories = db.MenuSubcategories.OrderBy(r => r.CategoryId).ThenBy(r => r.Name)
                                                    .Include(r => r.Category).Where(r => r.CategoryId==dish.CategoryId).ToList();
            ViewBag.menuSubcategories = menuSubcategories;

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("EditDish", dish);
            }
            return RedirectToAction("Index", "Home");
        }


        public ActionResult SaveDish(Dish dish, HttpPostedFileBase image = null)
        {
            if (dish.Id == 0)
            {
                dish.CatererId= int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                if (image != null)
                {
                    dish.Photo = new byte[image.ContentLength];
                    image.InputStream.Read(dish.Photo, 0, image.ContentLength);
                }
                db.Dishes.Add(dish);
            }
            else
            {
                Dish d = db.Dishes.Find(dish.Id);
                if (d != null)
                {
                    d.CatererId = dish.CatererId;
                    d.CategoryId = dish.CategoryId;
                    d.SubcategoryId = dish.SubcategoryId;
                    d.Name = dish.Name;
                    d.Price = dish.Price;
                    d.Description= dish.Description;
                    if (image != null)
                    {
                        d.Photo = new byte[image.ContentLength];
                        image.InputStream.Read(d.Photo, 0, image.ContentLength);
                    }
                }
            }
            db.SaveChanges();
            return RedirectToAction("EditMenu", "Caterer", new { id = dish.CategoryId });
        }

        public JsonResult DeleteDish(int id)
        {
            Dish dish = db.Dishes.Find(id);
            if (dish == null)
                return Json("Ошибка удаления", JsonRequestBehavior.AllowGet);
            db.Dishes.Remove(dish);
            db.SaveChanges();
            return Json("Ok",JsonRequestBehavior.AllowGet);
        }


        public JsonResult getSubcategories(int category)
        {
            List<MenuSubcategory> subcategories = db.MenuSubcategories.Where(r => r.CategoryId == category).ToList();
            List<JSubcategories> result = new List<JSubcategories>();
            foreach (MenuSubcategory s in subcategories)
            {
                result.Add(new JSubcategories() { Id = s.Id, Name = s.Name });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public class JSubcategories
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }


        public ActionResult MyOrders(int? Status, DateTime? Date)
        {
            DateTime CurrentDate;


            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");

            if (Status == null)
            {
                Status = 1;
            }

            if (Date == null)
                CurrentDate = DateTime.Now;
            else
                CurrentDate = DateTime.Parse(Date.ToString());

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);

            var Rows = db.OrderDetails.Include(r => r.Order).Include(r => r.Dish)
                            .Where(r => r.Dish.CatererId == userId &&
                            r.Order.Date.Day == CurrentDate.Day &&
                            r.Order.Date.Month == CurrentDate.Month &&
                            r.Order.Date.Year == CurrentDate.Year &&
                            r.Status == Status)
                            .GroupBy(r => new
                            {
                                DishId = r.DishId,
                                Name = r.Dish.Name,
                                Price = r.Dish.Price
                            })
                            .Select(g => new
                            {
                                DishId = g.Key.DishId,
                                Name = g.Key.Name,
                                Price = g.Key.Price,
                                Quantity = g.Sum(c => c.Quantity)
                            })
                        .ToList();

            List<CaterersOrder> caterersOrder = new List<CaterersOrder>();
            foreach (var r in Rows)
            {
                caterersOrder.Add(new CaterersOrder { Id = r.DishId, Name = r.Name, Price = r.Price, Quantity =r.Quantity });
            }

            ViewBag.CurrentDate = CurrentDate.ToString("yyyy-MM-dd");
            ViewBag.Status = Status;
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("MyOrders",caterersOrder);
            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult ChangeStatus(jData jData)
        {
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            List<OrderDetail> orderDetails = db.OrderDetails.Include(r => r.Order)
                                                .Include(r => r.Dish)
                                                .Where(r => r.Dish.CatererId==userId &&
                                                r.Order.Date.Year == jData.date.Year &&
                                                r.Order.Date.Month == jData.date.Month &&
                                                r.Order.Date.Day == jData.date.Day &&
                                                r.Status == 1).ToList();
            foreach (var od in orderDetails)
            {
                od.Status = 2;
            }
            db.SaveChanges();
            orderDetails = db.OrderDetails.Include(r => r.Order)
                            .Include(r => r.Dish)
                            .Where(r => r.Dish.CatererId == userId &&
                            r.Order.Date.Year == jData.date.Year &&
                            r.Order.Date.Month == jData.date.Month &&
                            r.Order.Date.Day == jData.date.Day &&
                            r.Status == 1).ToList();
            if (orderDetails.Count == 0)
                return Json("Ok");
            else
                return Json("Error");
        }

        public class jData
        {
            public DateTime date { get; set; }
            public int status { get; set; }
        }
    }
}