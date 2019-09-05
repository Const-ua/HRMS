using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Hrms.Models;



namespace Hrms.Controllers
{
    public class EmployeeController : Controller
    {

        private HRDbContext db = new HRDbContext();
        private List<string> ControllerRoles = new List<string>() { "Администратор", "Офис-менеджер", "HR-менеджер", "Рекрутер", "Сотрудник", "Линейный менеджер" };

        // GET: Employee
        public ActionResult Lunch(int? subcategory)
        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");
            if (subcategory == null)
                subcategory = 0;
            else
                subcategory = int.Parse(subcategory.ToString());
            List<MenuCategory> menuCategories = db.MenuCategories.OrderBy(r => r.Id).ToList();
            ViewBag.menuCategories = menuCategories;
            List<MenuSubcategory> menuSubcategories = db.MenuSubcategories.OrderBy(r => r.CategoryId).ThenBy(r => r.Name)
                                                    .Include(r => r.Category).ToList();
            ViewBag.menuSubcategories = menuSubcategories;
            List<Profile> users = db.Profiles.Include(u => u.Role).ToList();
            List<Dish> dishes = new List<Dish>();
            if (subcategory == 0)
            {
                dishes = db.Dishes.Include(r => r.Category).Include(r => r.Subcategory).OrderBy(r => r.Name).ToList();
                ViewBag.CurrentSubcategory = "Все меню";
            }
            else
            {
                dishes = db.Dishes.Include(r => r.Category).Include(r => r.Subcategory).OrderBy(r => r.Name)
                    .Where(r => r.SubcategoryId == subcategory).ToList();
                MenuSubcategory sc = menuSubcategories.FirstOrDefault(r => r.Id == subcategory);
                ViewBag.CurrentSubcategory = sc.Category.Name + " " + sc.Name;
            }
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            List<OrderDetail> OD = db.OrderDetails.Include(r => r.Order).Where(r => r.Order.ProfileId == userId && r.Order.Status == 0).ToList();
            int count =OD.Sum(r => r.Quantity);
            ViewBag.DishCount = count;
            ViewBag.CurrentEmployee = userId;
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("Lunch", dishes);

            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult AddDishToOrder(int id)
        {
            int dishId = id;
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            if (userId == 0)
                Json(-1);
            Order order = db.Orders.FirstOrDefault(r => r.ProfileId == userId && r.Status == 0);
            if (order == null) //новый заказ
            {
                order = new Order() { ProfileId = userId, Status = 0, Date = DateTime.Now, Sum = 0 };
                db.Orders.Add(order);
                db.SaveChanges();
                order = db.Orders.FirstOrDefault(r => r.ProfileId == userId && r.Status == 0);
                OrderDetail orderDetail = new OrderDetail() { OrderId = order.Id, DishId = dishId, Quantity = 1, Status = 0 };
                db.OrderDetails.Add(orderDetail);
                db.SaveChanges();
                return Json(1);
            }
            else
            {
                OrderDetail orderDetail = db.OrderDetails.FirstOrDefault(r => r.OrderId == order.Id && r.DishId == dishId);
                if (orderDetail == null) //новый товар в корзине
                {
                    orderDetail = new OrderDetail() { OrderId = order.Id, DishId = dishId, Quantity = 1 };
                    db.OrderDetails.Add(orderDetail);
                }
                else //добавить количество
                {
                    orderDetail.Quantity++;
                }
                db.SaveChanges();
                int count = db.OrderDetails.Include(r => r.Order).Where(r => r.OrderId == order.Id && r.Order.Status == 0).Sum(r => r.Quantity);
                return Json(count);
            }
        }

        public ActionResult Basket(int? id)
        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");
            if (id == null)
                id = 0;
            else
                id = int.Parse(id.ToString());

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);

            List<OrderDetail> orderDetails = db.OrderDetails.Include(r => r.Order).Include(r => r.Dish)
                                               .Where(r => r.Order.ProfileId == userId && r.Order.Status == 0)
                                               .OrderBy(r => r.Dish.Name).ToList();
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("Basket", orderDetails);
            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult ChangeQuantity(jData jData)
        {
            int Sum = 0;
            int Total = 0;


            OrderDetail Row = db.OrderDetails.Include(r => r.Dish).First(r => r.Id == jData.id);
            if (Row == null)
                Json("");
            Row.Quantity = jData.quantity;
            Sum = Row.Quantity * Row.Dish.Price;
            db.SaveChanges();
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Total = db.OrderDetails.Include(r => r.Dish).Include(r => r.Order)
                      .Where(r => r.Order.ProfileId == userId && r.Status == 0)
                      .Sum(r => r.Dish.Price * r.Quantity);
            jResponse jResponse = new jResponse() { Sum = Sum, Total = Total };
            return Json(jResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteDish(int id)
        {
            OrderDetail Row = db.OrderDetails.Find(id);
            if (Row == null)
                return Json("Ошибка удаления", JsonRequestBehavior.AllowGet);
            db.OrderDetails.Remove(Row);
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }


        public ActionResult SaveOrder()
        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Order order = db.Orders.FirstOrDefault(r => r.ProfileId == userId && r.Status == 0);
            List<OrderDetail> orderDetail = db.OrderDetails.Include(r => r.Dish).Include(r => r.Order).Where(r => r.Order.ProfileId == userId && r.Status == 0).ToList();
            if (orderDetail.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            foreach(var row in orderDetail)
            {
                row.Status = 1;
            }
            order.Sum = orderDetail.Sum(r => r.Dish.Price * r.Quantity);
            order.Status = 1;
            db.SaveChanges();

            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("Thanks");
            }
            return RedirectToAction("Index", "Home");
        }   

        public class jData
        {
            public int id { get; set; }
            public int quantity { get; set; }
        }

        public class jResponse
        {
            public int Sum { get; set; }
            public int Total { get; set; }
        }

        public ActionResult MakeOrder(int id)
        {

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Order order = db.Orders.Find(id);
            List<OrderDetail> orderDetails = db.OrderDetails.Where(r => r.OrderId == order.Id).ToList();
            order = db.Orders.FirstOrDefault(r => r.ProfileId==userId && r.Status==0);
            if (order == null) { 
                order = new Order() { ProfileId = userId, Status = 0, Date = DateTime.Now, Sum = 0 };
                db.Orders.Add(order);
                db.SaveChanges();
                order = db.Orders.FirstOrDefault(r => r.ProfileId == userId && r.Status == 0);
            }
            foreach(var row in orderDetails)
            {
                db.OrderDetails.Add(new OrderDetail() { OrderId = order.Id, DishId = row.DishId, Quantity = row.Quantity, Status = 0 });
            }
            db.SaveChanges();
            return RedirectToAction("Basket", "Employee");
        }

        public ActionResult MyActives()
        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            List<Active> actives = db.Actives.Include(r => r.Goods).Where(r => r.ProfileId == userId).OrderBy(r => r.Name).ToList();
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);

            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("MyActives", actives);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddGoal()
        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            Goal goal = new Goal();
            goal.Id = 0;
            goal.DateFrom = DateTime.Today;
            goal.DateTo = DateTime.Today.AddMonths(1);
            goal.LmId = userId;
            goal.ProfileId = userId;
            ViewBag.Lm = usr.FirstName + " " + usr.LastName;
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("MyGoal", goal);
            }
            return RedirectToAction("Index", "Home");

        }

        public ActionResult MyGoal(int? id)
        {
            if (id==null)
                return RedirectToAction("Index", "Home");
            id = int.Parse(id.ToString());

            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");

            Goal goal = db.Goals.Find(id);
            if (goal == null)
                return RedirectToAction("Index", "Home");
            Profile LmProfile = db.Profiles.Find(goal.LmId);

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            List<Profile> profiles = db.Profiles.Where(f => f.DepartmentId == usr.DepartmentId)
                                        .OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ToList();
            ViewBag.Profiles = profiles;
            ViewBag.Lm = LmProfile.FirstName + " " + LmProfile.LastName;
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("MyGoal", goal);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult SaveGoal(Goal goal)
        {
            if (goal.Id == 0)
            {
                int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                goal.LmId = userId;
                goal.ProfileId = userId;
                goal.DateFrom = DateTime.Today;
                db.Goals.Add(goal);

            }
            else
            {
                Goal g = db.Goals.Find(goal.Id);
                g.Status = goal.Status;
                g.Comments = goal.Comments;

            }
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

    }
}