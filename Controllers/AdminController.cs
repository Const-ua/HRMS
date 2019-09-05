using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Hrms.Models;

namespace Hrms.Controllers
{
    public class AdminController : Controller
    {
        private HRDbContext db = new HRDbContext();
        private List<string> ControllerRoles = new List<string>() { "Администратор" };
        public int PageSize = 6;
        public List<string> SortTypes = new List<string> { "Id_Asc", "Name_Asc", "Role_Asc", "Id_Desc", "Name_Desc", "Role_Desc" };
        static MainFormModel mfm = new MainFormModel();

        // GET: Admin
        public ActionResult Users()
        {
            int pages;
            int page;
            string SortField;
            string flt = "";

            if (HttpContext.Request["flt"] != null)
                flt = HttpContext.Request["flt"];
            ViewBag.flt = flt;
            if (HttpContext.Request["page"] != null)
                page = int.Parse(HttpContext.Request["page"]);
            else
                page = 1;
            mfm.Chat = db.Messages.Include(u => u.Profile).Take(100).OrderBy(m => m.Date).ToList();
            mfm.UserProfile = new Profile();
            if (HttpContext.Request["sort"] != null)
            {
                SortField = HttpContext.Request["sort"];
                if (SortTypes.IndexOf(SortField) == -1)
                    SortField = "Id_Asc";
            }
            else
            {
                SortField = "Id_Asc";
            }
            List<Profile> users = new List<Profile>();

            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                if (flt != "")
                    users = db.Profiles.Include(u => u.Role)
                        .Where(f => f.Email.ToLower().Contains(flt.ToLower()))
                        .ToList();
                else
                    users = db.Profiles.Include(u => u.Role).ToList();

                int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                mfm.UserProfile = db.Profiles.FirstOrDefault(u => u.Id == userId);
                Role role = db.Roles.FirstOrDefault(r => r.Id == mfm.UserProfile.RoleId);

                pages = (int)Math.Ceiling((decimal)users.Count() / PageSize);
                if (page > pages)
                    page = pages;
                switch (SortField)
                {
                    case "Id_Asc":
                        users = users.OrderBy(f => f.Id)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Name_Asc":
                        users = users.OrderBy(f => f.Email)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Role_Asc":
                        users = users.OrderBy(f => f.Role)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Id_Desc":
                        users = users.OrderByDescending(f => f.Id)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Name_Desc":
                        users = users.OrderByDescending(f => f.Email)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Role_Desc":
                        users = users.OrderByDescending(f => f.Role)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    default:
                        users = users.OrderBy(f => f.Id)
                           .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                }

                ViewBag.SortField = SortField;
                ViewBag.CurrentPage = page;
                ViewBag.Pages = pages;

                if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
                {
                    ViewData["LoggedIn"] = 1;
                    ViewData["UserName"] = mfm.UserProfile.FirstName + " " + mfm.UserProfile.LastName;
                    Session["UserId"] = mfm.UserProfile.Id;
                    if (role != null)
                        ViewData["UserRole"] = mfm.UserProfile.Role.Name;
                    else
                        ViewData["UserRole"] = "Роль неустановлена";
                    return View(users);
                }
            }
            return RedirectToAction("Index", "Home");

        }

        public ActionResult EditUser(int id)
        {
            List<Role> roles = db.Roles.ToList();
            ViewBag.Roles = roles;
            List<Profile> users = db.Profiles.Include(u => u.Role).ToList();
            Profile usr = users.FirstOrDefault(u => u.Id == id);
            if (usr != null)
            {
                int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                Profile user = db.Profiles.FirstOrDefault(u => u.Id == userId);
                Role role = db.Roles.FirstOrDefault(r => r.Id == user.RoleId);
                if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
                {
                    ViewData["LoggedIn"] = 1;
                    ViewData["UserName"] = user.FirstName + " " + user.LastName;
                    Session["UserId"] = user.Id;
                    if (role != null)
                        ViewData["UserRole"] = role.Name;
                    return View("EditUser", usr);
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Id = id;
                return PartialView("NotFound");
            }
        }

        [HttpPost]
        public ActionResult SaveUser(Profile user, string UserRole)
        {
            if (user.Id == 0)
            {
                Role role = db.Roles.FirstOrDefault(r => r.Name == UserRole);
                user.RoleId = role.Id;
                user.BirthDay = DateTime.ParseExact("19500101", "yyyyMdd", null);
                user.DepartmentId = 1;
                user.PositionId = 1;
                db.Profiles.Add(user);
            }
            else
            {
                Profile usr = db.Profiles.Find(user.Id);
                usr.Email = user.Email;
                usr.HashPwd = user.HashPwd;
                Role role = db.Roles.FirstOrDefault(r => r.Name == UserRole);
                usr.RoleId = role.Id;
            }
            db.SaveChanges();
            return RedirectToAction("Users", "Admin");
        }

        public ActionResult AddUser()
        {
            List<Role> roles = db.Roles.ToList();
            ViewBag.Roles = roles;
            Profile user = new Profile();
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = mfm.UserProfile.Id;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("EditUser", user);
            }
            return RedirectToAction("Index", "Home");

        }

        public JsonResult DeleteUser(int id)
        {
            if (id == int.Parse(HttpContext.Request.Cookies["UserId"].Value))
            {
                return Json("Ошибка: Нельзя удалять активного пользователя!");
            }
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == id);
            if (usr != null)
            {
                db.Profiles.Remove(usr);
                db.SaveChanges();
                return Json("Ok");
            }
            else
                return Json("Ошибка удаления: пользователь не найден!");

        }

        public JsonResult CheckEmail(UserData userData)
        {
            Profile usr = db.Profiles.FirstOrDefault(u => u.Email == userData.Email);
            if (usr == null)
                return Json("Ok");
            else
                return Json("Ошибка! E-mail адрес уже существует!");
        }

        public class UserData
        {
            public int Id { get; set; }
            public string Email { get; set; }
        }


        public ActionResult MenuCategories(int? id)
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                List<MenuCategory> categories = db.MenuCategories.ToList();

                int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
                Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
                if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
                {
                    ViewData["LoggedIn"] = 1;
                    ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                    Session["UserId"] = usr.Id;
                    if (role != null)
                        ViewData["UserRole"] = role.Name;
                    MenuCategory CurrentCategory = new MenuCategory();
                    if (id != null)
                    {
                        CurrentCategory = db.MenuCategories.FirstOrDefault(r => r.Id == id);
                        if (CurrentCategory == null)
                            CurrentCategory = db.MenuCategories.First();
                    }
                    else
                        CurrentCategory = db.MenuCategories.FirstOrDefault();
                    List<MenuSubcategory> subcategories = new List<MenuSubcategory>();
                    if (CurrentCategory != null)
                    {
                        subcategories = db.MenuSubcategories.OrderBy(r => r.Name)
                           .Where(r => r.CategoryId == CurrentCategory.Id).ToList();
                    }

                    ViewBag.CurrentCategory = CurrentCategory;
                    var tuple = new Tuple<List<MenuCategory>, List<MenuSubcategory>>(categories, subcategories);
                    return View(tuple);
                }
            }
            return RedirectToAction("Index", "Home");

        }

        public ActionResult AddMenuCategory()
        {
            MenuCategory category = new MenuCategory();
            return PartialView("EditMenuCategory", category);
        }

        public ActionResult SaveMenuCategory(MenuCategory category)
        {
            if (category.Id == 0)
            {
                db.MenuCategories.Add(category);
            }
            else
            {
                MenuCategory MenuCategory = db.MenuCategories.Find(category.Id);
                if (MenuCategory == null)
                    return RedirectToAction("MenuCategories", "Admin");
                MenuCategory.Name = category.Name;
            }
            db.SaveChanges();
            return RedirectToAction("MenuCategories", "Admin");
        }

        public ActionResult EditMenuCategory(int id)
        {
            MenuCategory category = db.MenuCategories.FirstOrDefault(r => r.Id == id);
            if (category == null)
                return PartialView("NotFound");
            else
                return PartialView("EditMenuCategory", category);
        }


        public JsonResult DeleteMenuCategory(int? id)
        {
            if (id == null)
                return Json("Ошибка удаления! \n Категория не найдена!", JsonRequestBehavior.AllowGet);
            MenuCategory category = db.MenuCategories.FirstOrDefault(r => r.Id == id);
            if (category == null)
                return Json("Ошибка удаления! \n Категория не найдена!", JsonRequestBehavior.AllowGet);
            MenuSubcategory subcategory = db.MenuSubcategories.FirstOrDefault(r => r.CategoryId == category.Id);
            if (subcategory != null)
                return Json("Ошибка удаления! \n Есть подчиненные подкатегории!", JsonRequestBehavior.AllowGet);
            db.MenuCategories.Remove(category);
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddMenuSubcategory(int? id)
        {
            if (id == null)
                return RedirectToAction("MenuCategories", "Admin");
            MenuSubcategory category = new MenuSubcategory();
            category.CategoryId = int.Parse(id.ToString());
            return PartialView("EditMenuSubcategory", category);
        }

        public ActionResult EditMenuSubcategory(int id)
        {
            MenuSubcategory category = db.MenuSubcategories.FirstOrDefault(r => r.Id == id);
            if (category == null)
                return PartialView("NotFound");
            else
                return PartialView("EditMenuSubcategory", category);
        }

        public JsonResult DeleteMenuSubcategory(int? id)
        {
            if (id == null)
                return Json("Ошибка удаления! \n Категория не найдена!", JsonRequestBehavior.AllowGet);
            MenuSubcategory category = db.MenuSubcategories.FirstOrDefault(r => r.Id == id);
            if (category == null)
                return Json("Ошибка удаления! \n Подкатегория не найдена!", JsonRequestBehavior.AllowGet);
            Dish dish = db.Dishes.FirstOrDefault(r => r.SubcategoryId == category.Id);
            if (dish != null)
                return Json("Ошибка удаления! \n Есть подчиненные блюда!", JsonRequestBehavior.AllowGet);
            db.MenuSubcategories.Remove(category);
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveMenuSubcategory(MenuSubcategory category)
        {
            if (category.Id == 0)
            {
                db.MenuSubcategories.Add(category);
            }
            else
            {
                MenuSubcategory MenuSubCategory = db.MenuSubcategories.Find(category.Id);
                if (MenuSubCategory == null)
                    return RedirectToAction("MenuCategories", "Admin");
                MenuSubCategory.Name = category.Name;
            }
            db.SaveChanges();
            return RedirectToAction("MenuCategories", "Admin", new { id = category.CategoryId });
        }


    }
}