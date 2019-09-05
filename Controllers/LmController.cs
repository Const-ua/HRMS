using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Hrms.Models;

namespace Hrms.Controllers
{
    public class LmController : Controller
    {
        private HRDbContext db = new HRDbContext();
        private List<string> ControllerRoles = new List<string>() { "Администратор", "Линейный менеджер" };
        public int PageSize = 6;
        public static string DepFilter = "Все";
        public static string PosFilter = "Все";
        public List<string> SortTypes = new List<string> { "Status_Asc", "Name_Asc", "Profile_Asc", "DateTo_Asc","Status_Desc", "Name_Desc", "Profile_Desc" ,"DateTo_Desc"};
        public static string SortField;

        // GET: Lm
        public ActionResult Goals()
        {
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
                    SortField = "DateTo_Asc";
            }
            else
            {
                SortField = "DateTo_Asc";
            }
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
                Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);

                List<Goal> goals = db.Goals
                    .Include(p => p.Profile).Where(r => r.Profile.DepartmentId==usr.DepartmentId).ToList();

                pages = (int)Math.Ceiling((decimal)goals.Count() / PageSize);
                if (page > pages)
                    page = pages;

                switch (SortField)
                {
                    case "Status_Asc":
                        goals = goals.OrderBy(f => f.Status).ThenBy(f => f.DateTo)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Name_Asc":
                        goals = goals.OrderBy(f => f.Name)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Profile_Asc":
                        goals = goals.OrderBy(f => f.Profile.LastName).ThenBy(f => f.Profile.FirstName)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "DateTo_Asc":
                        goals = goals.OrderBy(f => f.DateTo)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;

                    case "Status_Desc":
                        goals = goals.OrderByDescending(f => f.Status).ThenByDescending(f => f.DateTo)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Name_Desc":
                        goals = goals.OrderByDescending(f => f.Name)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Profile_Desc":
                        goals = goals.OrderByDescending(f => f.Profile.LastName).ThenByDescending(f => f.Profile.FirstName)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "DateTo_Desc":
                        goals = goals.OrderByDescending(f => f.DateTo)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    default:
                        goals = goals.OrderByDescending(f => f.DateTo)
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
                    ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                    Session["UserId"] = userId;
                    if (role != null)
                        ViewData["UserRole"] = role.Name;
                    return View(goals);
                }
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
            List<Profile> profiles = db.Profiles.Where(f => f.DepartmentId == usr.DepartmentId)
                                        .OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ToList();
            ViewBag.Profiles = profiles;
            Goal goal = new Goal();
            goal.Id = 0;
            goal.DateFrom = DateTime.Today;
            goal.DateTo = DateTime.Today.AddMonths(1);
            goal.LmId = userId;
            ViewBag.Lm = usr.FirstName + " " + usr.LastName;
            ViewBag.SortField = SortField;
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("EditGoal", goal);
            }
            return RedirectToAction("Goals", "Lm", new { sort = SortField });

        }

        public ActionResult EditGoal(int id)
        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");

            Goal goal = db.Goals.Find(id);
            if (goal == null)
                return RedirectToAction("Goals", "Lm", new { sort = SortField });
            Profile LmProfile = db.Profiles.Find(goal.LmId);

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            List<Profile> profiles = db.Profiles.Where(f => f.DepartmentId == usr.DepartmentId)
                                        .OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ToList();
            ViewBag.Profiles = profiles;
            ViewBag.Lm = LmProfile.FirstName + " " + LmProfile.LastName;
            ViewBag.SortField = SortField;
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("EditGoal", goal);
            }
            return RedirectToAction("Index", "Home");

        }

        public ActionResult SaveGoal(Goal goal)
        {
            if (goal.Id == 0)
            {
                int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                goal.LmId = userId;
                goal.DateFrom = DateTime.Today;
                db.Goals.Add(goal);
                
            }
            else
            {
                Goal g = db.Goals.Find(goal.Id);
                g.DateTo = goal.DateTo;
                g.Status = goal.Status;
                g.Name = goal.Name;
                g.Description = goal.Description;
                g.Comments = goal.Comments;

            }
            db.SaveChanges();
            return RedirectToAction("Goals", "Lm", new { sort = SortField });
        } 
    }

}