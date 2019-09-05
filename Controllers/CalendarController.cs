using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Hrms.Models;

namespace Hrms.Controllers
{
    public class CalendarController : Controller
    {
        private HRDbContext db = new HRDbContext();

        private List<string> ControllerRoles = new List<string>() { "Администратор", "Рекрутер", "HR-менеджер" };
        static DateTime CurrentDate = DateTime.Now;

        public ActionResult Index()

        {
            if (HttpContext.Request.Cookies["UserId"] == null)
                return RedirectToAction("Index", "Home");

            List<Interview> interviews = db.Interviews.Include(p => p.Aspirant).Include(p => p.Result)
                                    .Include(r => r.Aspirant.Vacancy)
                                    .Include(r => r.Aspirant.Vacancy.Position)
                                    .Include(r => r.Aspirant.Vacancy.Department)
                                    .Where(r => r.DateFrom.Year == CurrentDate.Year &&
                                                r.DateFrom.Month == CurrentDate.Month &&
                                                r.DateFrom.Day == CurrentDate.Day)
                                     .OrderBy(r => r.DateFrom).ThenBy(r=> r.DateTo).ToList();
            List<Calendar> Calendar = new List<Calendar>();

            DateTime DateTo = new DateTime();
            DateTime DateFrom = new DateTime();
            string Time = "";
            for (int i = 9; i <= 20; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    DateFrom = new DateTime(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day, i, j * 30, 0);
                    if (j == 0)
                        DateTo = new DateTime(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day, i, 30, 0);
                    else
                        DateTo = new DateTime(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day, i + 1, 0, 0);
                    List<Interview> interview = interviews.Where(r => (r.DateFrom >= DateFrom && r.DateFrom < DateTo)).ToList();
                     
                    Time = (i < 10 ? "0" + i.ToString() : i.ToString()) + ":" + (j * 30 < 30 ? "00" : (j * 30).ToString());
                    if (interview.Count ==0)
                        Calendar.Add(new Calendar {Time = Time, Interview = null});
                    else
                        foreach (var _interview in interview)
                        {
                            Calendar.Add(new Calendar { Time = Time, Interview = _interview });
                        }

                }
            }
            ViewBag.CurrentDate = CurrentDate;
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
                return View("Calendar", Calendar);

            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult ChangeDate(Jdate newDate)
        {
            DateTime DateTo = new DateTime();
            DateTime DateFrom = new DateTime();
            DateTime NewDate = newDate.date;
            string Time = "";
            List<Interview> interviews = db.Interviews.Include(p => p.Aspirant).Include(p => p.Result)
                        .Include(r => r.Aspirant.Vacancy)
                        .Include(r => r.Aspirant.Vacancy.Position)
                        .Include(r => r.Aspirant.Vacancy.Department)
                        .Where(r => r.DateFrom.Year == newDate.date.Year &&
                                    r.DateFrom.Month == newDate.date.Month &&
                                    r.DateFrom.Day == newDate.date.Day)
                         .OrderBy(r => r.DateFrom).ThenBy(r => r.DateTo).ToList();
            List<Jcalendar> Calendar = new List<Jcalendar>();

            for (int i = 9; i <= 20; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    DateFrom = new DateTime(NewDate.Year, NewDate.Month, NewDate.Day, i, j * 30, 0);
                    if (j == 0)
                        DateTo = new DateTime(NewDate.Year, NewDate.Month, NewDate.Day, i, 30, 0);
                    else
                        DateTo = new DateTime(NewDate.Year, NewDate.Month, NewDate.Day, i + 1, 0, 0);
                    List<Interview> interview = interviews.Where(r => (r.DateFrom >= DateFrom && r.DateFrom < DateTo)).ToList();

                    Time = (i < 10 ? "0" + i.ToString() : i.ToString()) + ":" + (j * 30 < 30 ? "00" : (j * 30).ToString());
                    if (interview.Count == 0)
                        Calendar.Add(new Jcalendar { timeFrom = Time, timeTo="", aspirantName="",aspirantPosition="",aspirantPhone="" });
                    else
                        foreach (var _interview in interview)
                        {
                            Calendar.Add(new Jcalendar { timeFrom = Time, timeTo = _interview.DateTo.ToString("HH:mm"),
                                                         aspirantName = _interview.Aspirant.LastName+" "+_interview.Aspirant.FirstName,
                                                         aspirantPosition = _interview.Aspirant.Vacancy.Position.Name,
                                                         aspirantPhone = _interview.Aspirant.Phone});
                        }
                }
            }

            return Json(Calendar,JsonRequestBehavior.AllowGet);
        }

        public class Jdate
        {
            public DateTime date { get; set; }
        }

        public class Jcalendar
        {
            public string timeFrom { get; set; }
            public string timeTo { get; set; }
            public string aspirantName { get; set; }
            public string aspirantPosition { get; set; }
            public string aspirantPhone { get; set; }
        }
    }
}