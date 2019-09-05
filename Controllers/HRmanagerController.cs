using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Hrms.Models;
using System.IO;

namespace Hrms.Controllers
{
    public class HRmanagerController : Controller
    {
        private HRDbContext db = new HRDbContext();
        private List<string> ControllerRoles = new List<string>() { "Администратор", "HR-менеджер" };
        public int PageSize = 6;
        public static string DepFilter = "Все";
        public static string PosFilter = "Все";

        public List<string> SortTypes = new List<string> { "Id_Asc", "Name_Asc", "Position_Asc", "Id_Desc", "Name_Desc", "Position_Desc" };

        public ActionResult Staff(string department, string position)
        {
            string SortField;

            int pages;
            int page;
            string flt = "";

            if (HttpContext.Request["page"] != null)
                page = int.Parse(HttpContext.Request["page"]);
            else
                page = 1;
            if (HttpContext.Request["flt"] != null)
                flt = HttpContext.Request["flt"];
            ViewBag.flt = flt;

            if (department != null)
                DepFilter = department;
            if (position != null)
                PosFilter = position;
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

                List<Profile> profiles = new List<Profile>();
                if (flt != "")
                     profiles= db.Profiles
                    .Include(p => p.Position).Where(r => r.PositionId != 1 &&
                    (r.LastName.ToLower().Contains(flt)|| r.FirstName.ToLower().Contains(flt)))
                    .ToList();
                else
                    profiles = db.Profiles
               .Include(p => p.Position).Where(r => r.PositionId != 1).ToList();

                ViewBag.DepFilter = "Все";
                ViewBag.PosFilter = "Все";
                if (DepFilter != null && DepFilter != "Все")
                {
                    profiles = profiles.Where(d => d.Department.Name == DepFilter).ToList();
                    ViewBag.DepFilter = DepFilter;
                }
                if (PosFilter != null && PosFilter != "Все")
                {
                    profiles = profiles.Where(p => p.Position.Name == PosFilter).ToList();
                    ViewBag.PosFilter = PosFilter;
                }

                pages = (int)Math.Ceiling((decimal)profiles.Count() / PageSize);
                if (page > pages)
                    page = pages;

                switch (SortField)
                {
                    case "Id_Asc":
                        profiles = profiles.OrderBy(f => f.Id)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Name_Asc":
                        profiles = profiles.OrderBy(f => f.LastName).ThenBy(f=>f.FirstName)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Position_Asc":
                        profiles = profiles.OrderBy(f => f.Position)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Desc";
                        break;
                    case "Id_Desc":
                        profiles = profiles.OrderByDescending(f => f.Id)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Name_Desc":
                        profiles = profiles.OrderByDescending(f => f.LastName).ThenByDescending(f => f.FirstName)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    case "Position_Desc":
                        profiles = profiles.OrderByDescending(f => f.Position)
                            .Skip((page - 1) * PageSize).Take(PageSize).ToList();
                        ViewBag.SortDirection = "Asc";
                        break;
                    default:
                        profiles = profiles.OrderBy(f => f.Id)
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
                    return View(profiles);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult EditEmployee(int id)
        {
            List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
            ViewBag.Departments = departments;
            List<Profile> profiles = db.Profiles.Include(u => u.Position).Include(r => r.Department).ToList();
            Profile employee = profiles.FirstOrDefault(u => u.Id == id);
            List<Position> positions = db.Positions.Where(r => r.DepId == employee.DepartmentId).ToList();
            ViewBag.Positions = positions;

            if (employee != null)
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
                    return View("EditEmployee", employee);
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
        public ActionResult SaveEmployee(Profile employee, int Dep, int Pos, HttpPostedFileBase image = null)
        {
            Profile empl = new Profile();
            if (employee.Id == 0)
            {
                empl = db.Profiles.FirstOrDefault( r => r.Email== employee.Email);
            }
            else
            {
                empl = db.Profiles.Find(employee.Id);
            }
            empl.FirstName = employee.FirstName;
            empl.LastName = employee.LastName;
            empl.MiddleName = employee.MiddleName;
            empl.Address = employee.Address;
            empl.Passport = employee.Passport;
            empl.BirthDay = employee.BirthDay;
            empl.Skype = employee.Skype;
            empl.Phone = employee.Phone;
            empl.TaxCode = employee.TaxCode;
            if (image != null)
            {
                empl.Photo = new byte[image.ContentLength];
                image.InputStream.Read(empl.Photo, 0, image.ContentLength);
            }

            Department department = db.Departments.FirstOrDefault(r => r.Id == Dep);
            empl.DepartmentId = department.Id;
            Position position = db.Positions.FirstOrDefault(r => r.Id == Pos);
            empl.PositionId = position.Id;

            db.SaveChanges();
            return RedirectToAction("Staff", "HRmanager");
        }

        public ActionResult AddEmployee()
        {
            List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();
            ViewBag.Departments = departments;

            Department dep = departments.FirstOrDefault();
            List<Position> positions = new List<Position>();
            if (dep != null)
                positions = db.Positions.Where(r => r.DepId == dep.Id).ToList();
            else
                positions = db.Positions.Where(r => r.DepId == dep.Id).ToList();
            ViewBag.Positions = positions;

            List<Profile> profiles = db.Profiles.Include(u => u.Position).Include(r => r.Department).ToList();
            Profile employee = new Profile();
            employee.DepartmentId = 0;
            employee.PositionId = 0;
            employee.BirthDay = DateTime.ParseExact("19500101", "yyyyMdd", null);
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
                return View("EditEmployee", employee);
            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult DeleteEmployee(int id)
        {
            Profile empl = db.Profiles.FirstOrDefault(u => u.Id == id);
            if (empl != null)
            {
                db.Profiles.Remove(empl);
                db.SaveChanges();
                return Json("Ok");
            }
            else
                return Json("Ошибка удаления: пользователь не найден!");
        }

        public JsonResult CheckEmployee(EmployeeData employeeData)
        {
            return Json("Ok");
        }

        public JsonResult CheckEmail(EmployeeData employeeData)
        {
           
            Profile usr = db.Profiles.FirstOrDefault(u => u.Email == employeeData.Email);
            if (usr == null)
                return Json("Ошибка! Пользователь с таким E-mail не найден!");
               else
                return Json("Ok");

        }

        public class EmployeeData
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public string Passport { get; set; }
            public DateTime BirthDay { get; set; }
            public string TaxCode { get; set; }
        }


        public ActionResult Positions(int? id)
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                List<Department> departments = db.Departments.Where(r => r.Id != 1).ToList();

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
                    Department CurrentDepartment = new Department();
                    if (id != null)
                    {
                        CurrentDepartment = db.Departments.FirstOrDefault(r => r.Id == id);
                        if (CurrentDepartment == null)
                            CurrentDepartment = db.Departments.FirstOrDefault(r => r.Id != 1);
                    }
                    else
                        CurrentDepartment = db.Departments.FirstOrDefault(r => r.Id != 1);
                    List<Position> positions = db.Positions.OrderBy(r => r.Name)
                            .Where(r => r.DepId == CurrentDepartment.Id).ToList();
                  
                    ViewBag.CurrentDepartment = CurrentDepartment;
                    var tuple = new Tuple<List<Department>, List<Position>>(departments, positions);
                    return View(tuple);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AddPosition(int id)
        {
            Position position = new Position();
            position.DepId = id;
            position.FullTime = "Постоянная";
            return PartialView("EditPosition", position);
        }


        public ActionResult EditPosition(int id)
        {
            List<Position> positions = db.Positions.ToList();
            Position position = positions.FirstOrDefault(r => r.Id == id);

            if (position != null)
                return PartialView(position);
            else
            {
                ViewBag.Id = id;
                return PartialView("NotFound");
            }
        }

        [ValidateInput(false)]
        public ActionResult SavePosition(Position pos)
        {
            if (pos.Id == 0)
            {
                pos.Name = pos.Name.Trim();
                db.Positions.Add(pos);
            }
            else
            {
                Position position = db.Positions.Find(pos.Id);
                position.Name = pos.Name.Trim();
                position.SalaryFrom = pos.SalaryFrom;
                position.SalaryTo = pos.SalaryTo;
                position.Experience = pos.Experience;
                position.DepId = pos.DepId;
                position.FullTime = pos.FullTime;
            }
            db.SaveChanges();
            return RedirectToAction("Positions", "HRmanager", new { id = pos.DepId });
        }

        public JsonResult DeletePosition(int id)
        {

            Position position = db.Positions.FirstOrDefault(r => r.Id == id);

            if (position != null)
            {
                List<Profile> profiles = db.Profiles.Where(r => r.PositionId == position.Id).ToList();
                if (profiles.Count != 0)
                {
                    return Json("Не могу удалить позицию: \n На этой позиции есть сотрудники!", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    db.Positions.Remove(position);
                    db.SaveChanges();
                    return Json("Ok", JsonRequestBehavior.AllowGet);
                }
            }
            else
                return Json("Ошибка удаления! \n Позиция не найдена!", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddDepartment()
        {
            Department department = new Department();
            return PartialView("EditDepartment", department);
        }

        public ActionResult EditDepartment(int id)
        {
            Department department = db.Departments.FirstOrDefault(r => r.Id == id);
            if (department != null)
                return PartialView(department);
            else
            {
                ViewBag.Id = id;
                return PartialView("NotFound");
            }
        }

        [ValidateInput(false)]
        public ActionResult SaveDepartment(Department dep)
        {
            if (dep.Id == 0)
            {
                dep.Name = dep.Name.Trim();
                db.Departments.Add(dep);
            }
            else
            {
                Department department = db.Departments.Find(dep.Id);
                department.Name = dep.Name.Trim();
                department.Budget = dep.Budget;
            }
            db.SaveChanges();
            return RedirectToAction("Positions", "HRmanager", new { id = dep.Id });
        }

        public JsonResult DeleteDepartment(int id)
        {

            Department department = db.Departments.FirstOrDefault(r => r.Id == id);

            if (department != null)
            {
                List<Position> positions = db.Positions.Where(r => r.DepId == department.Id).ToList();
                if (positions.Count != 0)
                {
                    return Json("Не могу удалить отдел: \n В нем есть неудаленные позиции!", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    db.Departments.Remove(department);
                    db.SaveChanges();
                    return Json("Ok", JsonRequestBehavior.AllowGet);
                }
            }
            else
                return Json("Ошибка удаления! \n Отдел не найден!", JsonRequestBehavior.AllowGet);
        }

        public FileContentResult GetPhoto(int id)
        {
            Profile profile = db.Profiles.FirstOrDefault(p => p.Id == id);

            if (profile != null)
                if (profile.Photo !=null)
                  return File(profile.Photo, "image/jpeg");
            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
            using (FileStream fstream = new FileStream(startupPath+"/Images/NoPhoto.jpg", FileMode.Open))
            {

                byte[] array = new byte[fstream.Length];
                fstream.Read(array, 0, array.Length);
                return File(array, "image/jpeg");
            }
            
            
            
        }

        [HttpPost]
        public JsonResult GetPositions(int Dep)
        {
            List<PositionsList> positions = new List<PositionsList>();
            Department Department = db.Departments.FirstOrDefault(r => r.Id == Dep);
            if (Department == null)
            {
                positions.Add(new PositionsList() { Id = 0, Name = "" });
            }
            else
            {
                int dep = Department.Id;

                List<Position> Positions = db.Positions.Where(r => r.DepId == dep).ToList();
                foreach (Position p in Positions)
                {
                    positions.Add(new PositionsList() { Id = p.Id, Name = p.Name });
                }
            }
            return Json(positions, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EmailSearch(string term)
        {
            if (term == null)
                term = "";
            var email = db.Profiles.Where(r => r.Email.Contains(term)).Select(f => new { label = f.Email, value = f.Email }).Distinct().ToList();
            return Json(email, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEmployeeRole(string Email)
        {
            AutosearchResult jdata = new AutosearchResult { label = "", value = "" };
            Profile profile = db.Profiles.FirstOrDefault(r => r.Email == Email);
            if (profile != null)
            {
                Role r = db.Roles.FirstOrDefault(f => f.Id == profile.RoleId);
                if (r != null)
                {
                    jdata.label = r.Name;
                    if (profile.LastName != null)
                    {
                        jdata.value = "Этот логин принадлежит сотруднику " + profile.LastName + " " + profile.FirstName + "!";
                    }
                }
            }

            return Json(jdata, JsonRequestBehavior.AllowGet);
        }
        public class AutosearchResult
        {
            public string label { get; set; }
            public string value { get; set; }
        }
        public class PositionsList
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }


    }




}