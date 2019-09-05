using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Hrms.Models;


namespace Hrms.Controllers
{
    public class RecruterController : Controller
    {
        private HRDbContext db = new HRDbContext();
        public int PageSize = 6;
        private List<string> ControllerRoles = new List<string>() { "Администратор", "Рекрутер", "HR-менеджер" };
        public List<string> SortTypes = new List<string> { "Priority_Asc", "Position_Asc", "Department_Asc", "DateFrom_Asc", "DateTo_Asc", "Priority_Desc", "Position_Desc", "Department_Desc", "DateFrom_Desc", "DateTo_Desc" };
        public List<string> AspirantsSortTypes = new List<string> { "Priority_Asc", "Name_Asc", "Department_Asc", "Vacancy_Asc", "Priority_Desc", "Name_Desc", "Department_Desc", "Vacancy_Desc" };
        public List<string> InterviewSortTypes = new List<string> { "Result_Asc", "Name_Asc", "Date_Asc", "Vacancy_Asc", "Result_Desc", "Name_Desc", "Date_Desc", "Vacancy_Desc" };

        static int VacDepFilter;
        static int VacPosFilter;
        static string VacSortField;

        static int AspDepFilter;
        static int AspPosFilter;
        static int AspRecFilter;
        static string AspSortField;

        static int IntDepFilter;
        static int IntPosFilter;
        static string IntSortField;


        static int Status;
        // GET: Recruter
        public ActionResult Vacancies(int? department, int? position, int? id)
        {
            int pages;
            int page;
            if (HttpContext.Request["page"] != null)
                page = int.Parse(HttpContext.Request["page"]);
            else
                page = 1;

            if (HttpContext.Request.Cookies["UserId"] != null)
            {


                if (HttpContext.Request["sort"] != null)
                {
                    VacSortField = HttpContext.Request["sort"];
                    if (SortTypes.IndexOf(VacSortField) == -1)
                        VacSortField = "Id_Asc";
                }
                else
                {
                    VacSortField = "Id_Asc";
                }

                List<Vacancy> vacansies = db.Vacancies.Include(p => p.Position).ToList();
                List<Position> positions = db.Positions.Where(r => r.Id != 1).ToList();
                ViewBag.Positions = positions;
                List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
                ViewBag.Departments = departments;

                ViewBag.DepFilter = 0;
                ViewBag.PosFilter = 0;
                if (department != null && department != 0)
                {
                    vacansies = vacansies.Where(d => d.Department.Id == department).ToList();
                    ViewBag.DepFilter = department;
                }
                if (position != null && position != 0)
                {
                    vacansies = vacansies.Where(p => p.Position.Id == position).ToList();
                    ViewBag.PosFilter = position;
                }

                VacDepFilter = ViewBag.DepFilter;
                VacPosFilter = ViewBag.PosFilter;


                pages = (int)Math.Ceiling((decimal)vacansies.Count() / PageSize);
                if (page > pages)
                    page = pages;


                switch (VacSortField)
                {
                    case "Priority_Asc":
                        vacansies = vacansies.OrderBy(f => f.Priority).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Position_Asc":
                        vacansies = vacansies.OrderBy(f => f.Position).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Department_Asc":
                        vacansies = vacansies.OrderBy(f => f.Department).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "DateFrom_Asc":
                        vacansies = vacansies.OrderBy(f => f.DateFrom).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "DateTo_Asc":
                        vacansies = vacansies.OrderBy(f => f.DateTo).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;

                    case "Priority_Desc":
                        vacansies = vacansies.OrderByDescending(f => f.Priority).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Position_Desc":
                        vacansies = vacansies.OrderByDescending(f => f.Position).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Department_Desc":
                        vacansies = vacansies.OrderByDescending(f => f.Department).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "DateFrom_Desc":
                        vacansies = vacansies.OrderByDescending(f => f.DateFrom).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "DateTo_Desc":
                        vacansies = vacansies.OrderByDescending(f => f.DateTo).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;

                    default:
                        vacansies = vacansies.OrderBy(f => f.Id).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                }
                ViewBag.SortField = VacSortField;
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
                    Vacancy CurrentVacancy = new Vacancy();
                    if (id != null)
                    {
                        CurrentVacancy = db.Vacancies.FirstOrDefault(r => r.Id == id);
                        if (CurrentVacancy == null)
                            CurrentVacancy = db.Vacancies.First();
                    }
                    else
                        CurrentVacancy = db.Vacancies.First();
                    List<Aspirant> aspirants = db.Aspirants.OrderBy(r => r.FirstName + r.LastName)
                            .Where(r => r.VacancyId == CurrentVacancy.Id).ToList();
                    ViewBag.CurrentVacancy = CurrentVacancy;
                    var tuple = new Tuple<List<Vacancy>, List<Aspirant>>(vacansies, aspirants);
                    return View(tuple);


                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddVacancy()
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
                    List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
                    ViewBag.Departments = departments;
                    Department dep = departments.FirstOrDefault();

                    List<Position> positions = new List<Position>();
                    if (dep == null)
                        positions = db.Positions.Where(r => r.Id != 1).ToList();
                    else
                        positions = db.Positions.Where(r => r.Id != 1).Where(r => r.DepId == dep.Id).ToList();
                    ViewBag.Positions = positions;


                    Vacancy vacancy = new Vacancy();
                    vacancy.DepartmentId = 0;
                    vacancy.PositionId = 0;
                    vacancy.DateFrom = DateTime.Now;
                    vacancy.DateTo = DateTime.Now;

                    ViewBag.CurrentDepartment = VacDepFilter;
                    ViewBag.CurrentPosition = VacPosFilter;
                    ViewBag.SortField = VacSortField;

                    return PartialView("EditVacancy", vacancy);
                }
            }
            return RedirectToAction("Index", "Home");


        }

        public ActionResult EditVacancy(int? id)
        {
            if (id == null)
                return RedirectToAction("Index", "Home");
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
                    List<Vacancy> vacancies = db.Vacancies.Include(u => u.Position).Include(r => r.Department).ToList();
                    List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
                    ViewBag.Departments = departments;

                    Vacancy vacancy = vacancies.FirstOrDefault(u => u.Id == id);
                    if (vacancy == null)
                        return RedirectToAction("Index", "Home");

                    Department dep = departments.FirstOrDefault(r => r.Id == vacancy.DepartmentId);
                    List<Position> positions = new List<Position>();
                    if (dep != null)
                        positions = db.Positions.Where(r => r.DepId == dep.Id).ToList();
                    else
                        positions = db.Positions.Where(r => r.DepId == dep.Id).ToList();

                    ViewBag.Positions = positions;
                    ViewBag.CurrentDepartment = VacDepFilter;
                    ViewBag.CurrentPosition = VacPosFilter;
                    ViewBag.SortField = VacSortField;
                    return PartialView("EditVacancy", vacancy);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult SaveVacancy(Vacancy vacancy, int Dep, int Pos)
        {
            if (vacancy.Id == 0)
            {
                Department department = db.Departments.FirstOrDefault(r => r.Id == Dep);
                vacancy.DepartmentId = department.Id;
                Position position = db.Positions.FirstOrDefault(r => r.Id == Pos);
                vacancy.PositionId = position.Id;
                db.Vacancies.Add(vacancy);
            }
            else
            {
                Vacancy vac = db.Vacancies.Find(vacancy.Id);
                vac.Name = vacancy.Name;
                vac.DateFrom = vacancy.DateFrom;
                vac.DateTo = vacancy.DateTo;
                vac.Priority = vacancy.Priority;
                vac.Salary = vacancy.Salary;
                vac.Skills = vacancy.Skills;
                vac.Experience = vacancy.Experience;
                vac.Comments = vacancy.Comments;
                Department department = db.Departments.FirstOrDefault(r => r.Id == Dep);
                vac.DepartmentId = department.Id;
                Position position = db.Positions.FirstOrDefault(r => r.Id == Pos);
                vac.PositionId = position.Id;
            }
            db.SaveChanges();
            return RedirectToAction("Vacancies", "Recruter", new { department = VacDepFilter, position = VacPosFilter, sort = VacSortField });
        }


        public JsonResult DeleteVacancy(int id)
        {
            Vacancy vacancy = db.Vacancies.FirstOrDefault(u => u.Id == id);
            if (vacancy != null)
            {
                db.Vacancies.Remove(vacancy);
                db.SaveChanges();
                return Json("Ok");
            }
            else
                return Json("Ошибка удаления: новость не найдена!");
        }

        public ActionResult Aspirants(int? id, int? recruter, int? department, int? position)
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {

                int pages;
                int page;
                string flt = "";

                Status = (id == null ? 1 : id.Value);
                if (HttpContext.Request["page"] != null)
                    page = int.Parse(HttpContext.Request["page"]);
                else
                    page = 1;
                if (HttpContext.Request["sort"] != null)
                {
                    AspSortField = HttpContext.Request["sort"];
                    if (AspirantsSortTypes.IndexOf(AspSortField) == -1)
                        AspSortField = "Id_Asc";
                }
                else
                {
                    AspSortField = "Id_Asc";
                }
                if (HttpContext.Request["flt"] != null)
                    flt = HttpContext.Request["flt"];
                ViewBag.flt = flt;


                if (Status < 1 | Status > 4)
                    Status = 1;
                ViewBag.Status = Status;
                List<Profile> Recruters = db.Profiles.Include(r => r.Role).Where(r => r.Role.Name == "Рекрутер")
                    .Where(r => r.LastName != null && r.FirstName != null).ToList();

                List<Aspirant> aspirants = new List<Aspirant>();
                if (recruter == 0 || recruter == null)
                    aspirants = db.Aspirants.Where(r => r.Status == Status)
                        .Include(r => r.Vacancy).Include(r => r.Vacancy.Position).Include(r => r.Vacancy.Department)
                        .OrderBy(r => r.Date).ToList();
                else
                    aspirants = db.Aspirants.Where(r => r.Status == Status && r.ProfileId == recruter)
                        .Include(r => r.Vacancy).Include(r => r.Vacancy.Position).Include(r => r.Vacancy.Department)
                        .OrderBy(r => r.Date).ToList();

                if (department != null && department != 0)
                {
                    aspirants = aspirants.Where(d => d.Vacancy.DepartmentId == department).ToList();
                    ViewBag.DepFilter = department;
                }
                if (position != null && position != 0)
                {
                    aspirants = aspirants.Where(p => p.Vacancy.PositionId == position).ToList();
                    ViewBag.PosFilter = position;
                }
                if (recruter != null && recruter != 0)
                {
                    aspirants = aspirants.Where(d => d.Profile.Id == recruter).ToList();
                    ViewBag.RecFilter = recruter;
                    AspRecFilter = ViewBag.RecFilter;
                }
                aspirants = aspirants.Where(f => f.LastName.ToLower().Contains(flt.ToLower()) ||
                                                 f.FirstName.ToLower().Contains(flt.ToLower())).ToList();
                pages = (int)Math.Ceiling((decimal)aspirants.Count() / PageSize);
                if (page > pages)
                    page = pages;

                switch (AspSortField)
                {
                    case "Priority_Asc":
                        aspirants = aspirants.OrderBy(f => f.Vacancy.Priority)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Name_Asc":
                        aspirants = aspirants.OrderBy(f => f.LastName + f.FirstName)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;

                    case "Vacancy_Asc":
                        aspirants = aspirants.OrderBy(f => f.Vacancy.Position.Name)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Department_Asc":
                        aspirants = aspirants.OrderBy(f => f.Vacancy.Department.Name)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;

                    case "Priority_Desc":
                        aspirants = aspirants.OrderByDescending(f => f.Vacancy.Priority)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Name_Desc":
                        aspirants = aspirants.OrderByDescending(f => f.LastName + f.FirstName)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;

                    case "Vacancy_Desc":
                        aspirants = aspirants.OrderByDescending(f => f.Vacancy.Position.Name)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Department_Desc":
                        aspirants = aspirants.OrderByDescending(f => f.Vacancy.Department.Name)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    default:
                        aspirants = aspirants.OrderBy(f => f.LastName + f.FirstName)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                }
                //users = users.Skip((page - 1) * PageSize).Take(PageSize).ToList();




                ViewBag.SortField = AspSortField;
                ViewBag.CurrentPage = page;
                ViewBag.Pages = pages;

                AspDepFilter = ViewBag.DepFilter == null ? 0 : ViewBag.DepFilter;
                AspPosFilter = ViewBag.PosFilter == null ? 0 : ViewBag.PosFilter;
                AspRecFilter = ViewBag.RecFilter == null ? 0 : ViewBag.RecFilter;


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
                    List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
                    List<Position> positions = db.Positions.Where(r => r.Id != 1).ToList();
                    ViewBag.Recruters = Recruters;
                    ViewBag.Departments = departments;
                    ViewBag.Positions = positions;
                    return View(aspirants);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddAspirant()
        {
            HttpCookie CookieReq = HttpContext.Request.Cookies["UserId"];
            if (CookieReq == null)
                return RedirectToAction("Index", "Home");
            List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
            ViewBag.Departments = departments;

            Department dep = departments.FirstOrDefault();
            List<Vacancy> vacancies = new List<Vacancy>();
            if (dep != null)
                vacancies = db.Vacancies.Where(r => r.DepartmentId == dep.Id).Include(r => r.Position).ToList();
            else
                vacancies = db.Vacancies.Where(r => r.DepartmentId == dep.Id).Include(r => r.Position).ToList();
            ViewBag.Vacancies = vacancies;

            List<Interview> interviews = new List<Interview>();
            ViewBag.Interviews = interviews;

            Aspirant aspirant = new Aspirant();
            aspirant.Vacancy = new Vacancy();
            aspirant.Vacancy.DepartmentId = db.Departments.FirstOrDefault().Id;
            aspirant.Vacancy.Department = db.Departments.FirstOrDefault();
            aspirant.Vacancy.PositionId = 0;
            aspirant.BirthDay = DateTime.ParseExact("19500101", "yyyyMdd", null);
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
            aspirant.Status = 1;
            aspirant.ProfileId = usr.Id;
            aspirant.Profile = usr;
            ViewBag.PosFilter = AspPosFilter;
            ViewBag.DepFilter = AspDepFilter;
            ViewBag.RecFilter = AspRecFilter;
            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("EditAspirant", aspirant);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult EditAspirant(int? id)
        {
            if (id == null)
                return RedirectToAction("Index", "Home");
            HttpCookie CookieReq = HttpContext.Request.Cookies["UserId"];
            if (CookieReq == null)
                return RedirectToAction("Index", "Home");

            Aspirant aspirant = db.Aspirants.Include(r => r.Profile).Include(r => r.Vacancy).FirstOrDefault(r => r.Id == id);
            if (aspirant == null)
                return RedirectToAction("Aspirants", "Recruter", new { department = AspDepFilter, position = AspPosFilter, recruter = AspRecFilter });
            aspirant.Vacancy.Position = db.Positions.FirstOrDefault(r => r.Id == aspirant.Vacancy.PositionId);
            List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
            ViewBag.Departments = departments;

            Department dep = departments.FirstOrDefault();
            List<Vacancy> vacancies = db.Vacancies.Where(r => r.DepartmentId == aspirant.Vacancy.DepartmentId)
                .Include(r => r.Position).ToList();
            ViewBag.Vacancies = vacancies;

            List<Interview> interviews = db.Interviews.Where(r => r.AspirantId == aspirant.Id)
                .Include(r => r.Result).OrderBy(r => r.DateFrom).ToList();
            ViewBag.Interviews = interviews;

            List<Result> results = db.Results.ToList();
            ViewBag.Results = results;


            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);

            ViewBag.PosFilter = AspPosFilter;
            ViewBag.DepFilter = AspDepFilter;
            ViewBag.RecFilter = AspRecFilter;

            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("EditAspirant", aspirant);
            }
            return RedirectToAction("Index", "Home");
        }


        public ActionResult SaveAspirant(Aspirant aspirant, HttpPostedFileBase image = null) //int Dep, int Pos,
        {
            if (aspirant.Id == 0)
            {

                aspirant.Vacancy = new Vacancy();
                aspirant.Vacancy = db.Vacancies.FirstOrDefault(r => r.Id == aspirant.VacancyId);
                aspirant.Date = DateTime.Now;
                aspirant.ProfileId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                aspirant.Status = 1;
                if (image != null)
                {
                    aspirant.Photo = new byte[image.ContentLength];
                    image.InputStream.Read(aspirant.Photo, 0, image.ContentLength);
                }

                db.Aspirants.Add(aspirant);
            }
            else
            {
                Aspirant asp = db.Aspirants.FirstOrDefault(r => r.Id == aspirant.Id);

                asp.FirstName = aspirant.FirstName;
                asp.MiddleName = aspirant.MiddleName;
                asp.LastName = aspirant.LastName;
                asp.Address = aspirant.Address;
                asp.Phone = aspirant.Phone;
                asp.Email = aspirant.Email;
                asp.Skype = aspirant.Skype;
                asp.Salary = aspirant.Salary;
                asp.Education = aspirant.Education;
                asp.Sex = aspirant.Sex;
                asp.BirthDay = aspirant.BirthDay;
                asp.VacancyId = aspirant.VacancyId;
                asp.Experience = aspirant.Experience;
                asp.CvText = aspirant.CvText;
                asp.Skills = aspirant.Skills;
                asp.Status = aspirant.Status;
                if (image != null)
                {
                    asp.Photo = new byte[image.ContentLength];
                    image.InputStream.Read(asp.Photo, 0, image.ContentLength);
                }
            }
            db.SaveChanges();
            return RedirectToAction("Aspirants", "Recruter", new { department = AspDepFilter, position = AspPosFilter, recruter = AspRecFilter });
        }

        public ActionResult ViewAspirant(int id)
        {
            if (id == 0)
                return RedirectToAction("Index", "Home");

            HttpCookie CookieReq = HttpContext.Request.Cookies["UserId"];
            if (CookieReq == null)
                return RedirectToAction("Index", "Home");

            Aspirant aspirant = db.Aspirants.Include(r => r.Profile).Include(r => r.Vacancy).FirstOrDefault(r => r.Id == id);
            if (aspirant == null)
                return RedirectToAction("Aspirants", "Recruter", new { department = AspDepFilter, position = AspPosFilter, recruter = AspRecFilter });
            aspirant.Vacancy.Position = db.Positions.FirstOrDefault(r => r.Id == aspirant.Vacancy.PositionId);
            List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
            ViewBag.Departments = departments;

            Department dep = departments.FirstOrDefault();
            List<Vacancy> vacancies = db.Vacancies.Where(r => r.DepartmentId == aspirant.Vacancy.DepartmentId)
                .Include(r => r.Position).ToList();
            ViewBag.Vacancies = vacancies;

            List<Interview> interviews = db.Interviews.Where(r => r.AspirantId == aspirant.Id)
                           .Include(r => r.Result).OrderBy(r => r.DateFrom).ToList();
            ViewBag.Interviews = interviews;

            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
            Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);

            ViewBag.CurrentDepartment = VacDepFilter;
            ViewBag.CurrentPosition = VacPosFilter;

            if (ControllerRoles.FirstOrDefault(r => r == role.Name) != null)
            {
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View("ViewAspirant", aspirant);
            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult DeleteAspirant(int id)
        {
            Aspirant aspirant = db.Aspirants.FirstOrDefault(u => u.Id == id);
            if (aspirant != null)
            {
                db.Aspirants.Remove(aspirant);
                db.SaveChanges();
                return Json("Ok");
            }
            else
                return Json("Ошибка удаления: новость не найдена!");
        }




        public FileContentResult GetPhoto(int id)
        {
            Aspirant aspirant = db.Aspirants.FirstOrDefault(p => p.Id == id);
            if (aspirant != null)
                return File(aspirant.Photo, "image/jpeg");
            else
                return null;
        }

        public JsonResult getDepartmentVacancies(int Dep)
        {
            List<DepartmenVacancy> positions = new List<DepartmenVacancy>();
            Department Department = db.Departments.FirstOrDefault(r => r.Id == Dep);
            if (Department == null)
            {
                positions.Add(new DepartmenVacancy() { Id = 0, Name = "" });
            }
            else
            {
                int dep = Department.Id;

                List<Vacancy> vacancies = db.Vacancies.Where(r => r.DepartmentId == dep).Include(r => r.Position).ToList();
                foreach (Vacancy v in vacancies)
                {
                    positions.Add(new DepartmenVacancy() { Id = v.Id, Name = v.Name });
                }
            }
            return Json(positions, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddInterview(InterviewDate interviewDate)
        {
            Aspirant asp = db.Aspirants.FirstOrDefault(r => r.Id == interviewDate.id);
            if (asp == null)
            {
                return Json("Ошибка: Кандидат не найден!", JsonRequestBehavior.AllowGet);
            }
            Interview interview = new Interview();
            interview.AspirantId = interviewDate.id;
            interview.DateFrom = interviewDate.dateFrom;
            interview.DateTo = interviewDate.dateTo;
            interview.ResultId = 1;
            db.Interviews.Add(interview);
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }


        public class InterviewDate
        {
            public DateTime dateFrom { get; set; }
            public DateTime dateTo { get; set; }
            public int id { get; set; }
        }

        public class DepartmenVacancy
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public ActionResult Interviews(int? department, int? position, int? id)
        {
            int pages;
            int page;
            if (HttpContext.Request["page"] != null)
                page = int.Parse(HttpContext.Request["page"]);
            else
                page = 1;

            if (HttpContext.Request.Cookies["UserId"] != null)
            {


                if (HttpContext.Request["sort"] != null)
                {
                    IntSortField = HttpContext.Request["sort"];
                    if (InterviewSortTypes.IndexOf(IntSortField) == -1)
                        IntSortField = "Id_Asc";
                }
                else
                {
                    IntSortField = "Id_Asc";
                }

                List<Interview> interviews = db.Interviews.Include(p => p.Aspirant).Include(p => p.Result)
                    .Include(r => r.Aspirant.Vacancy)
                    .Include(r => r.Aspirant.Vacancy.Position)
                    .Include(r => r.Aspirant.Vacancy.Department).ToList();
                List<Position> positions = db.Positions.Where(r => r.Id != 1).ToList();
                ViewBag.Positions = positions;
                List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
                ViewBag.Departments = departments;

                ViewBag.DepFilter = 0;
                ViewBag.PosFilter = 0;
                if (department != null && department != 0)
                {
                    interviews = interviews.Where(d => d.Aspirant.Vacancy.DepartmentId == department).ToList();
                    ViewBag.DepFilter = department;
                }
                if (position != null && position != 0)
                {
                    interviews = interviews.Where(p => p.Aspirant.Vacancy.PositionId == position).ToList();
                    ViewBag.PosFilter = position;
                }

                IntDepFilter = ViewBag.DepFilter;
                IntPosFilter = ViewBag.PosFilter;

                pages = (int)Math.Ceiling((decimal)interviews.Count() / PageSize);
                if (page > pages)
                    page = pages;


                switch (IntSortField)
                {
                    case "Result_Asc":
                        interviews = interviews.OrderBy(f => f.Result.Name).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Name_Asc":
                        interviews = interviews.OrderBy(f => f.Aspirant.LastName + " " + f.Aspirant.FirstName).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Date_Asc":
                        interviews = interviews.OrderBy(f => f.DateFrom).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Vacancy_Asc":
                        interviews = interviews.OrderBy(f => f.Aspirant.Vacancy.Position).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Result_Desc":
                        interviews = interviews.OrderByDescending(f => f.Result.Name).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Name_Desc":
                        interviews = interviews.OrderByDescending(f => f.Aspirant.LastName + " " + f.Aspirant.FirstName).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Date_Desc":
                        interviews = interviews.OrderByDescending(f => f.DateFrom).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Vacancy_Desc":
                        interviews = interviews.OrderByDescending(f => f.Aspirant.Vacancy.Position).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;

                    default:
                        interviews = interviews.OrderBy(f => f.DateFrom).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                }
                interviews = interviews.Skip((page - 1) * PageSize).Take(PageSize).ToList();
                ViewBag.SortField = IntSortField;
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
                    Vacancy CurrentVacancy = new Vacancy();
                    if (id != null)
                    {
                        CurrentVacancy = db.Vacancies.FirstOrDefault(r => r.Id == id);
                        if (CurrentVacancy == null)
                            CurrentVacancy = db.Vacancies.First();
                    }
                    else
                        CurrentVacancy = db.Vacancies.First();
                    return View(interviews);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult NewInterview()
        {
            HttpCookie CookieReq = HttpContext.Request.Cookies["UserId"];
            if (CookieReq == null)
                return RedirectToAction("Index", "Home");

            Interview interview = new Interview();
            interview.Id = 0;
            interview.DateFrom = DateTime.Now;
            interview.DateTo = DateTime.Now.AddMinutes(30);

            interview.Aspirant = db.Aspirants.Include(r => r.Vacancy).FirstOrDefault();
            if (interview.Aspirant == null)
                return RedirectToAction("Index", "Home");
            interview.Result = new Result();
            interview.ResultId = 1;
            List<Aspirant> aspirants = db.Aspirants.OrderBy(r => r.LastName + r.FirstName).ToList();
            ViewBag.Aspirants = aspirants;
            List<Vacancy> vacancies = db.Vacancies.Include(r => r.Position).Include(r => r.Position).Include(r => r.Department)
                 .OrderBy(r => r.Position.Name).ToList();
            ViewBag.Vacancies = vacancies;
            List<Result> results = db.Results.OrderBy(r => r.Id).ToList();
            ViewBag.Results = results;
            ViewBag.TimeFrom = "09:00";
            ViewBag.TimeTo = "09:30";
            ViewBag.PosFilter = IntPosFilter;
            ViewBag.DepFilter = IntDepFilter;

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
                return View("EditInterview", interview);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult EditInterview(int? id)
        {
            if (id == null)
                return RedirectToAction("Index", "Home");

            HttpCookie CookieReq = HttpContext.Request.Cookies["UserId"];
            if (CookieReq == null)
                return RedirectToAction("Index", "Home");

            List<Interview> interviews = db.Interviews.Include(p => p.Aspirant).Include(p => p.Result)
                                            .Include(r => r.Aspirant.Vacancy)
                                            .Include(r => r.Aspirant.Vacancy.Position)
                                            .Include(r => r.Aspirant.Vacancy.Department).ToList();
            Interview interview = interviews.FirstOrDefault(r => r.Id == id);
            if (interview == null)
                return RedirectToAction("Interviews", "Recruter");
            List<Aspirant> aspirants = db.Aspirants.OrderBy(r => r.LastName + r.FirstName).ToList();
            ViewBag.Aspirants = aspirants;
            List<Vacancy> vacancies = db.Vacancies.Include(r => r.Position).Include(r => r.Position).Include(r => r.Department)
                .OrderBy(r => r.Position.Name).ToList();
            ViewBag.Vacancies = vacancies;
            List<Result> results = db.Results.OrderBy(r => r.Id).ToList();
            ViewBag.Results = results;

            ViewBag.TimeFrom = interview.DateFrom.ToString("HH:mm");
            ViewBag.TimeTo = interview.DateTo.ToString("HH:mm");
            ViewBag.PosFilter = IntPosFilter;
            ViewBag.DepFilter = IntDepFilter;
            ViewBag.SortField = IntSortField;

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
                return View("EditInterview", interview);
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult SaveInterview(Interview interview, string TimeFrom, string TimeTo, int? Asp, int? Pos, int Result)
        {
            if (interview.Id == 0)
            {
                interview.AspirantId = Asp.GetValueOrDefault();
                interview.DateFrom = new DateTime(interview.DateFrom.Year, interview.DateFrom.Month, interview.DateFrom.Day,
                       int.Parse(TimeFrom.Substring(0, 2)), int.Parse(TimeFrom.Substring(3, 2)), 0);
                interview.DateTo = interview.DateFrom = new DateTime(interview.DateFrom.Year, interview.DateFrom.Month, interview.DateFrom.Day,
                       int.Parse(TimeTo.Substring(0, 2)), int.Parse(TimeTo.Substring(3, 2)), 0);
                interview.ResultId = Result;
                db.Interviews.Add(interview);
            }
            else
            {
                Interview Interview = db.Interviews.Find(interview.Id);
                if (Interview == null)
                    return RedirectToAction("Interviews", "Recruter", new { department = IntDepFilter, position = IntPosFilter, sort = IntSortField });
                Interview.DateFrom = new DateTime(interview.DateFrom.Year, interview.DateFrom.Month, interview.DateFrom.Day,
                       int.Parse(TimeFrom.Substring(0, 2)), int.Parse(TimeFrom.Substring(3, 2)), 0);
                Interview.DateTo = interview.DateFrom = new DateTime(interview.DateFrom.Year, interview.DateFrom.Month, interview.DateFrom.Day,
                       int.Parse(TimeTo.Substring(0, 2)), int.Parse(TimeTo.Substring(3, 2)), 0);
                Interview.Comments = interview.Comments;

                Interview.ResultId = Result;
            }
            db.SaveChanges();
            return RedirectToAction("Interviews", "Recruter", new { department = IntDepFilter, position = IntPosFilter, sort = IntSortField });
        }

        public JsonResult DeleteInterview(int id)
        {
            Interview interview = db.Interviews.Find(id);
            if (interview != null)
            {
                db.Interviews.Remove(interview);
                db.SaveChanges();
                return Json("Ok");
            }
            else
            {
                return Json("Ошибка удаления: интервью не найдено!");
            }
        }

        public JsonResult getAspirantVacancy(JsonString id)
        {
            if (id != null)
            {
                Aspirant asp = db.Aspirants.Find(int.Parse(id.str));
                return Json(asp.VacancyId);
            }
            else
                return Json("");
        }

        public class JsonString
        {
            public string str { get; set; }
        }

        public class JsonStringAndInt
        {
            public string str { get; set; }
            public int id { get; set; }
        }

        public JsonResult AspirantSearch(string term)
        {
            if (term == null)
                term = "";
            var AspirantName = db.Aspirants.Where(r => (r.LastName.Trim() + " " + r.FirstName.Trim()).Contains(term))
                                 .Select(f => new { label = (f.LastName.Trim() + " " + f.FirstName.Trim()), value = (f.LastName.Trim() + " " + f.FirstName.Trim()) }).Distinct().ToList();
            return Json(AspirantName, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPhotoByName(JsonString name)
        {
            Aspirant asp = db.Aspirants.FirstOrDefault(r => (r.LastName.Trim() + " " + r.FirstName.Trim()) == name.str);
            if (asp != null)
            {
                string jReturn = asp.Id.ToString();
                return Json(jReturn);
            }
            else
                return Json("-1");
        }

        public JsonResult AddAspirantToVacancy(JsonStringAndInt data)
        {
            Aspirant asp = db.Aspirants.FirstOrDefault(r => (r.LastName.Trim() + " " + r.FirstName.Trim()) == data.str);
            if (asp != null)
            {
                asp.VacancyId = data.id;
                db.SaveChanges();
                var jReturn = new { id = asp.Id, name = asp.FirstName.Trim() + " " + asp.LastName.Trim(), Experience = asp.Experience };
                return Json(jReturn);
            }
            return Json("-1");

        }
    }

}