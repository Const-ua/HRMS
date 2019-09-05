using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Hrms.Models;

namespace Hrms.Controllers
{
    public class OmanagerController : Controller
    {
        private HRDbContext db = new HRDbContext();
        private List<string> ControllerRoles = new List<string>() { "Администратор", "Офис-менеджер" };
        public int PageSize = 6;
        public List<string> SortTypes = new List<string> { "Id_Asc", "Name_Asc", "Position_Asc", "Id_Desc", "Name_Desc", "Position_Desc" };
        static string DepFilter;
        static string PosFilter;
        // GET: Omanager

        public ActionResult News()
        {
            string SortField;

            int pages;
            int page;
            if (HttpContext.Request["page"] != null)
                page = int.Parse(HttpContext.Request["page"]);
            else
                page = 1;

            if (HttpContext.Request["sort"] != null)
            {
                SortField = HttpContext.Request["sort"];
                if (SortTypes.IndexOf(SortField) == -1)
                    SortField = "Date_Desc";
            }
            else
            {
                SortField = "Date_Desc";
            }
            if (HttpContext.Request.Cookies["UserId"] != null)
            {

                List<NewsItem> news = db.NewsItems.ToList();
                pages = (int)Math.Ceiling((decimal)news.Count() / PageSize);
                if (page > pages)
                    page = pages;

                switch (SortField)
                {
                    case "Id_Asc":
                        news = news.OrderBy(f => f.Id)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Header_Asc":
                        news = news.OrderBy(f => f.Header)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Date_Asc":
                        news = news.OrderBy(f => f.Date)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Id_Desc":
                        news = news.OrderByDescending(f => f.Id)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Header_Desc":
                        news = news.OrderByDescending(f => f.Header)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Date_Desc":
                        news = news.OrderByDescending(f => f.Date)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    default:
                        news = news.OrderBy(f => f.Date)
                           .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                }

                ViewBag.SortField = SortField;
                ViewBag.CurrentPage = page;
                ViewBag.Pages = pages;
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
                    return View(news);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddNews()
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
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

                    NewsItem news = new NewsItem();
                    news.Date = DateTime.Today;
                    news.Priority = "Ordinar";
                    return View("Editnews", news);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult EditNews(int id)
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
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

                    List<NewsItem> newsItems = db.NewsItems.ToList();
                    NewsItem news = newsItems.FirstOrDefault(n => n.Id == id);
                    if (news != null)
                        return View(news);
                    else
                    {
                        ViewBag.Id = id;
                        return PartialView("NotFound");
                    }
                }
            }
            return RedirectToAction("Index", "Home");

        }

        [ValidateInput(false)]
        public ActionResult SaveNews(NewsItem news, HttpPostedFileBase image = null)
        {
            if (news.Id == 0)
            {
                news.Text = news.Text.Trim();
                if (image != null)
                {
                    news.Photo = new byte[image.ContentLength];
                    image.InputStream.Read(news.Photo, 0, image.ContentLength);
                }

                db.NewsItems.Add(news);

            }
            else
            {
                NewsItem newsItem = db.NewsItems.Find(news.Id);
                newsItem.Date = news.Date;
                newsItem.Header = news.Header;
                newsItem.Text = news.Text;
                newsItem.Priority = news.Priority;
                news.Text = news.Text.Trim();
                if (image != null)
                {
                    newsItem.Photo = new byte[image.ContentLength];
                    image.InputStream.Read(newsItem.Photo, 0, image.ContentLength);
                }
            }
            db.SaveChanges();
            return RedirectToAction("News", "Omanager");
        }


        public JsonResult DeleteNews(int id)
        {
            NewsItem news = db.NewsItems.FirstOrDefault(u => u.Id == id);
            if (news != null)
            {
                db.NewsItems.Remove(news);
                db.SaveChanges();
                return Json("Ok");
            }
            else
                return Json("Ошибка удаления: новость не найдена!");
        }


        public ActionResult Actives(string department, string position, int? id)
        {
            string SortField;
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
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                List<Position> positions = db.Positions.Where(r => r.Id != 1).ToList();
                ViewBag.Positions = positions;
                List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
                ViewBag.Departments = departments;

                List<Profile> profiles = db.Profiles
                    .Include(p => p.Position).Include(d => d.Department).ToList();
                ViewBag.DepFilter = "Все";
                ViewBag.PosFilter = "Все";
                if (department != null && department != "Все")
                {
                    profiles = profiles.Where(d => d.Department.Name == department).ToList();
                    ViewBag.DepFilter = department;
                }
                if (position != null && position != "Все")
                {
                    profiles = profiles.Where(p => p.Position.Name == position).ToList();
                    ViewBag.PosFilter = position;
                }

                switch (SortField)
                {
                    case "Id_Asc":
                        profiles = profiles.OrderBy(f => f.Id).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Name_Asc":
                        profiles = profiles.OrderBy(f => f.LastName + f.FirstName).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Position_Asc":
                        profiles = profiles.OrderBy(f => f.Position).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Id_Desc":
                        profiles = profiles.OrderByDescending(f => f.Id).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Name_Desc":
                        profiles = profiles.OrderByDescending(f => f.LastName + f.FirstName).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Position_Desc":
                        profiles = profiles.OrderByDescending(f => f.Position).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    default:
                        profiles = profiles.OrderBy(f => f.Id).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                }

                ViewBag.SortField = SortField;
                DepFilter = ViewBag.DepFilter;
                PosFilter = ViewBag.PosFilter;
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
                    Profile CurrentEmployee = new Profile();
                    if (id != null)
                    {
                        CurrentEmployee = db.Profiles.FirstOrDefault(r => r.Id == id);
                        if (CurrentEmployee == null)
                            CurrentEmployee = db.Profiles.First();
                    }
                    else
                        CurrentEmployee = db.Profiles.First();
                    List<Active> actives = db.Actives.OrderBy(r => r.Goods.GoodsName + r.Name)
                            .Where(r => r.ProfileId == CurrentEmployee.Id).ToList();
                    List<Goods> goods = db.Goods.OrderBy(r => r.GoodsName).ToList();
                    ViewBag.CurrentEmployee = CurrentEmployee;
                    var tuple = new Tuple<List<Profile>, List<Active>>(profiles, actives);
                    return View(tuple);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult EditActive(int id)
        {
            List<Goods> goods = db.Goods.ToList();
            ViewBag.Goods = goods;
            List<Active> actives = db.Actives.Include(u => u.Goods).ToList();
            Active act = actives.FirstOrDefault(u => u.Id == id);

            if (act != null)
                return PartialView(act);
            else
            {
                ViewBag.Id = id;
                return PartialView("NotFound");
            }
        }

        [HttpPost]
        public ActionResult SaveActive(Active active, string GoodsName)
        {
            if (active.Id == 0)
            {
                Goods goods = db.Goods.FirstOrDefault(r => r.GoodsName == GoodsName);
                active.GoodsId = goods.Id;
                active.ProfileId = int.Parse(Session["UserId"].ToString());
                db.Actives.Add(active);
                db.SaveChanges();
                return RedirectToAction("Goods", "Omanager", new { Id = active.GoodsId });

            }
            else
            {
                Active act = db.Actives.Find(active.Id);
                act.Name = active.Name;
                act.InvNumber = active.InvNumber;
                act.Price = active.Price;
                act.PurchaseDate = active.PurchaseDate;
                act.SerNo = active.SerNo;
                act.Comments = active.Comments;
                Goods goods = db.Goods.FirstOrDefault(r => r.GoodsName == GoodsName);
                act.Id = active.Id;
                act.ProfileId = active.ProfileId;
                db.SaveChanges();
                return RedirectToAction("Goods", "Omanager", new { Id = act.GoodsId });

            }
        }


        public JsonResult DeleteActive(int id)
        {
            Active active = db.Actives.FirstOrDefault(r => r.Id == id);

            if (active != null)
            {
                db.Actives.Remove(active);
                db.SaveChanges();
                return Json("Ok", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("Ошибка удаления! \n Актив не найден!", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Goods(int? id)
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                List<Goods> goods = db.Goods.ToList();

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
                    Goods CurrentGoodsType = new Goods();
                    if (id != null)
                    {
                        CurrentGoodsType = db.Goods.FirstOrDefault(r => r.Id == id);
                        if (CurrentGoodsType == null)
                            CurrentGoodsType = db.Goods.First();
                    }
                    else
                        CurrentGoodsType = db.Goods.FirstOrDefault();
                    List<Active> actives = new List<Active>();
                    if (CurrentGoodsType != null)
                    {
                        actives = db.Actives.OrderBy(r => r.Name)
                           .Where(r => r.GoodsId == CurrentGoodsType.Id).ToList();
                    }

                    ViewBag.CurrentGoodsType = CurrentGoodsType;
                    var tuple = new Tuple<List<Goods>, List<Active>>(goods, actives);
                    return View(tuple);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddGoods()
        {
            Goods goods = new Goods();
            return PartialView("EditGoods", goods);
        }

        public ActionResult SaveGoods(Goods goods)
        {
            if (goods.Id == 0)
            {
                db.Goods.Add(goods);
            }
            else
            {
                Goods Good = db.Goods.Find(goods.Id);
                Good.GoodsName = goods.GoodsName;
            }
            db.SaveChanges();
            return RedirectToAction("Goods", "Omanager");
        }

        public ActionResult EditGoods(int id)
        {
            Goods goods = db.Goods.FirstOrDefault(r => r.Id == id);
            if (goods == null)
                return PartialView("NotFound");
            else
                return PartialView("EditGoods", goods);
        }


        public JsonResult DeleteGoods(int? id)
        {
            if (id == null)
                return Json("Ошибка удаления! \n Тип не найден!", JsonRequestBehavior.AllowGet);
            Goods goods = db.Goods.FirstOrDefault(r => r.Id == id);
            if (goods == null)
                return Json("Ошибка удаления! \n Тип не найден!", JsonRequestBehavior.AllowGet);
            Active act = db.Actives.FirstOrDefault(r => r.GoodsId == goods.Id);
            if (act != null)
                return Json("Ошибка удаления! \n Есть подчиненные активы!", JsonRequestBehavior.AllowGet);
            db.Goods.Remove(goods);
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddAct(int id)
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
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
                    Active act = new Active();
                    List<Goods> goods = db.Goods.ToList();
                    ViewBag.Goods = goods;
                    act.GoodsId = 0;
                    act.ProfileId = 0;
                    act.PurchaseDate = DateTime.Now;
                    Goods CurrentGoods = db.Goods.FirstOrDefault(r => r.Id == id);
                    if (CurrentGoods == null)
                        ViewBag.CurrentGoods = "";
                    else
                        ViewBag.CurrentGoods = CurrentGoods.GoodsName;
                    ViewBag.CurrentGoodsId = id;
                    return View("EditActive", act);
                }
            }
            return RedirectToAction("Index", "Home");

        }

        public ActionResult EditAct(int id)
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
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
                    Active act = db.Actives.FirstOrDefault(r => r.Id == id);
                    if (act == null)
                    {
                        return PartialView("NotFound");
                    }
                    List<Goods> goods = db.Goods.ToList();
                    ViewBag.Goods = goods;
                    Goods CurrentGoods = db.Goods.FirstOrDefault(r => r.Id == act.GoodsId);
                    if (CurrentGoods == null)
                        ViewBag.CurrentGoods = "";
                    else
                        ViewBag.CurrentGoods = CurrentGoods.GoodsName;
                    ViewBag.CurrentGoodsId = act.GoodsId;
                    return View("EditActive", act);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddActive(int? id)
        {
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

                if (id == null)
                {
                    ViewBag.ErrorMessage = "Владелец неопределен!";
                    return PartialView("NotFound");
                }
                Profile OwnerProfile = db.Profiles.FirstOrDefault(r => r.Id == id);
                if (OwnerProfile == null)
                {
                    ViewBag.ErrorMessage = "Владелец не найден!";
                    return PartialView("NotFound");
                }
                ViewBag.Profile = OwnerProfile;
                List<Goods> goods = db.Goods.OrderBy(f => f.GoodsName).ToList();
                ViewBag.Goods = goods;
                List<Active> actives = db.Actives.OrderBy(f => f.Name).ToList();
                Active CurrentActive = actives.FirstOrDefault();
                ViewBag.CurrentActive = CurrentActive;
                ViewBag.CurrentDepartment = DepFilter;
                ViewBag.CurrentPosition = PosFilter;
                return View("TakeActive", actives);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult TakeActive(int CurrentId, int Actives)
        {
            Active act = db.Actives.FirstOrDefault(r => r.Id == Actives);
            if (act != null)
            {
                act.ProfileId = CurrentId;
                db.SaveChanges();
            }
            return RedirectToAction("Actives", "Omanager", new { id = CurrentId, department = DepFilter, position = PosFilter });
        }

        [HttpPost]
        public JsonResult GetActives(string goodsName)
        {

            Goods goods = db.Goods.FirstOrDefault(r => r.GoodsName == goodsName);
            List<JActive> actives = new List<JActive>();
            if (goods == null)
                return Json(actives, JsonRequestBehavior.AllowGet);
            List<Active> ActList = db.Actives.Where(r => r.GoodsId == goods.Id).OrderBy(f => f.Name).ToList();
            foreach (Active a in ActList)
            {
                actives.Add(new JActive() { id = a.Id, name = a.Name });
            }
            return Json(actives, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetActiveProperties(int activeId)
        {

            Active act = db.Actives.FirstOrDefault(r => r.Id == activeId);
            return Json(act, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveOwner(int activeId)
        {
            Active act = db.Actives.FirstOrDefault(r => r.Id == activeId);
            Profile profile = db.Profiles.FirstOrDefault(r => r.Id == act.ProfileId);
            if (profile == null)
                return Json("Владелец не найден!", JsonRequestBehavior.AllowGet);
            return Json(profile.FirstName + " " + profile.LastName, JsonRequestBehavior.AllowGet);
        }


        public class JActive
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public FileContentResult GetNewsPhoto(int id)
        {
            NewsItem newsItem = db.NewsItems.FirstOrDefault(p => p.Id == id);
            if (newsItem != null)
                if (newsItem.Photo!=null)
                  return File(newsItem.Photo, "image/jpeg");
            return null;
        }

    }

}