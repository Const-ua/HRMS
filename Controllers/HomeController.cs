using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Hrms.Models;

namespace Hrms.Controllers
{
    public class HomeController : Controller
    {

        private HRDbContext db = new HRDbContext();
        public int PageSize = 6;
        static MainFormModel mfm = new MainFormModel();


        public ActionResult Index()
        {
            int pages;
            int page;
            string flt = "";

            int userId = 0;
            if (HttpContext.Request["page"] != null)
                page = int.Parse(HttpContext.Request["page"]);
            else
                page = 1;

            if (HttpContext.Request["flt"] != null)
                flt = HttpContext.Request["flt"];
            ViewBag.flt = flt;
            mfm.UserProfile = new Profile();
            List<Profile> users = db.Profiles.Include(u => u.Role).ToList();
            mfm.Chat = db.Messages.Include(u => u.Profile).Take(100).OrderBy(m => m.Date).ToList();


            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                try
                {
                    userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                }
                catch
                {
                    userId = 0;
                }
                finally
                {
                }
                mfm.UserProfile = users.FirstOrDefault(u => u.Id == userId);

                if (mfm.UserProfile == null)
                {
                    ViewData["LoggedIn"] = null;
                    HttpContext.Response.Cookies["UserId"].Expires = DateTime.Now.AddDays(-1);
                    return RedirectToAction("Index");

                }

                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = mfm.UserProfile.FirstName + " " + mfm.UserProfile.LastName;
                ViewData["UserRole"] = mfm.UserProfile.Role.Name;
                Session["UserId"] = mfm.UserProfile.Id;

            }
            if (flt != "")
                mfm.News = db.NewsItems.Where(f => f.Text.ToLower().Contains(flt.ToLower())
                                                || f.Header.ToLower().Contains(flt.ToLower()))
                    .OrderByDescending(f => f.Date).ToList();
            else
                mfm.News = db.NewsItems.OrderByDescending(f => f.Date).ToList();
            pages = (int)Math.Ceiling((decimal)mfm.News.Count() / PageSize);
            if (page > pages)
                page = pages;
            mfm.News = mfm.News.Skip((page - 1) * PageSize).Take(PageSize).ToList();
            ViewBag.CurrentPage = page;
            ViewBag.Pages = pages;
            List<Goal> goals = db.Goals.Where(f => f.ProfileId == userId).OrderBy(f => f.DateTo).ToList();
            ViewBag.GoalsPages = (int)Math.Ceiling((decimal)goals.Count() / PageSize); ;
            goals = goals.Take(PageSize).ToList();

            ViewBag.Goals = goals;
            ViewBag.GoalsCurrentPage = 1;
            return View(mfm);
        }

        [HttpPost]
        public ActionResult Login(string Email, string Pwd)
        {
            Email = Email.Trim();
            List<Profile> users = db.Profiles.Include(u => u.Role).ToList();
            Profile usr = db.Profiles.FirstOrDefault(u => u.Email == Email);
            HttpContext.Request.Cookies.Remove("UserId");
            if (usr == null)
            {
                ViewData["LoggedIn"] = -1;
            }
            else
            {
                if (usr.HashPwd == Pwd)
                {
                    Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
                    ViewData["LoggedIn"] = 1;
                    ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                    HttpCookie cookie = new HttpCookie("UserId");
                    Session["UserId"] = usr.Id;
                    cookie.Value = usr.Id.ToString();
                    cookie.Expires = DateTime.Now.AddHours(100);
                    HttpContext.Response.Cookies.Add(cookie);
                    ViewData["UserRole"] = usr.Role?.Name;
                    mfm.UserProfile.GetProperties(usr);

                }
                else
                {
                    ViewData["LoggedIn"] = 0;
                    ViewData["UserName"] = usr.Email;
                    Session["UserId"] = null;
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Logout()
        {
            List<Profile> users = db.Profiles.ToList();
            ViewData["LoggedIn"] = null;
            HttpContext.Response.Cookies["UserId"].Expires = DateTime.Now.AddDays(-1);
            mfm.UserProfile = null;
            return RedirectToAction("Index"); 
        }

        public ActionResult Cabinet()
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
                var orders = db.OrderDetails.Include(r => r.Dish).Include(r => r.Order)
                                        .Where(r => r.Order.ProfileId==userId && r.Status>0)
                                        .GroupBy(r => new
                                        {
                                            Id =r.OrderId,
                                            Date = r.Order.Date

                                        })
                                        .Select(g => new
                                        {
                                            Id =g.Key.Id,
                                            Date = g.Key.Date,
                                            Sum = g.Sum(c => c.Quantity * c.Dish.Price)
                                        })
                                        .OrderByDescending(r => r.Date).ToList();
                List<OrderDetail> orderDetails = db.OrderDetails.Include(r => r.Dish).Include(r => r.Order)
                                                        .Where(r => r.Order.ProfileId== userId).ToList();
                List<MyOrders> myOrders = new List<MyOrders>();
                foreach (var o in orders)
                {
                    myOrders.Add(new MyOrders() { Id = o.Id, Date = o.Date, Sum = o.Sum });
                }
                ViewBag.Pages = (int)Math.Ceiling((decimal)orders.Count() / PageSize);
                ViewBag.CurrentPage = 1;
                ViewBag.Orders = myOrders;
                ViewBag.OrderDetails = orderDetails;
                
                Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
                Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
               
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View(usr);

            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public JsonResult ChangePwd(PwdData pwdData)
        {
            if (pwdData.Pwd != pwdData.RepeatPwd)
            {
                return Json("Пароли не совпадают!");
            }

            if (String.IsNullOrWhiteSpace(pwdData.Pwd) | String.IsNullOrWhiteSpace(pwdData.RepeatPwd))
            {
                return Json("Ошибка: Пароль не может быть пустым!");
            }

            Profile usr = db.Profiles.FirstOrDefault(u => u.Id == pwdData.Id);
            if (usr != null)
            {
                if (usr.HashPwd != pwdData.OldPwd)
                {
                    return Json("Старый пароль неверен!");
                }
                usr.HashPwd = pwdData.Pwd;
                db.SaveChanges();
                return Json("Пароль успешно изменен!");
            }
            else
                return Json("Внутренняя ошибка. Пароль не изменен!");
        }

        [HttpPost]
        public JsonResult AddChatMessage(MessageDate msg)
        {
            if (msg.text == null)
                return Json("");
            Message message = new Message();
            message.Mesg = msg.text;
            message.Date = DateTime.Now;
            message.ProfileId = mfm.UserProfile.Id;
            message.ProfileId = mfm.UserProfile.Id;
            db.Configuration.AutoDetectChangesEnabled = false;
            mfm.Chat.Add(message);
            db.Messages.Add(message);
            db.SaveChanges();
            message.Profile = mfm.UserProfile;
            string Html = "<div>";
            Html += "<p style='font-size:x-small; color:red'><strong>" + message.Date + " " + message.Profile.FirstName + " " + message.Profile.LastName + "</strong><br />";
            Html += "<span style='font-size:medium; color:black'>" + message.Mesg + "</span></p>";
            Html += "</div>";
            return Json(Html);
        }

        public class PwdData
        {
            public int Id { get; set; }
            public string Pwd { get; set; }
            public string RepeatPwd { get; set; }
            public string OldPwd { get; set; }
        }

        public class MessageDate
        {
            public string text { get; set; }
        }

        public class jPage
        {
            public int page { get; set; }
        } 

        public JsonResult RefreshChat()
        {
            string Html = "";
            mfm.Chat = db.Messages.Include(u => u.Profile).Take(100).OrderBy(m => m.Date).ToList();
            foreach (Message mess in mfm.Chat)
            {
                Html += "<div>";
                Html += "<p style='font-size:x-small;  color:red'><strong>" + mess.Date + " " + mess.Profile.FirstName + " " + mess.Profile.LastName + "</strong><br />";
                Html += "<span style='font-size:medium; color:black'>" + mess.Mesg + "</span></p>";
                Html += "</div>";
            }
            return Json(Html);
        }

        public ActionResult News(int? id)
        {
            if (HttpContext.Request.Cookies["UserId"] != null)
            {
                int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);

                if (id == null)
                    return RedirectToAction("Index");
                NewsItem newsItem = db.NewsItems.FirstOrDefault(r => r.Id == id);
                if (newsItem == null)
                    return RedirectToAction("Index");
                Profile usr = db.Profiles.FirstOrDefault(u => u.Id == userId);
                Role role = db.Roles.FirstOrDefault(r => r.Id == usr.RoleId);
                ViewData["LoggedIn"] = 1;
                ViewData["UserName"] = usr.FirstName + " " + usr.LastName;
                Session["UserId"] = userId;
                if (role != null)
                    ViewData["UserRole"] = role.Name;
                return View(newsItem);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult CheckPwd(string UserName, string Pwd)
        {
            UserName = UserName.Trim();
            Profile usr = db.Profiles.FirstOrDefault(u => u.Email == UserName);
            if (usr == null)
            {
                return Json("Не ведено имя пользователя!");
            }
            else
            {
                if (usr.HashPwd == Pwd)
                {
                    return Json("Ok");
                }
                else
                {
                    return Json("Пароль неверен!");
                }
            }

        }

        public JsonResult GetPage(jPage jPage)
        {
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            var orders = db.OrderDetails.Include(r => r.Dish).Include(r => r.Order)
                                    .Where(r => r.Order.ProfileId == userId && r.Status > 0)
                                    .GroupBy(r => new
                                    {
                                        Id = r.OrderId,
                                        Date = r.Order.Date

                                    })
                                    .Select(g => new
                                    {
                                        Id = g.Key.Id,
                                        Date = g.Key.Date,
                                        Sum = g.Sum(c => c.Quantity * c.Dish.Price)
                                    })
                                    .OrderByDescending(r => r.Date).ToList();
            orders = orders.Skip((jPage.page - 1) * PageSize).Take(PageSize).ToList();
            List<OrderDetail> orderDetails = db.OrderDetails.Include(r => r.Dish).Include(r => r.Order)
                                                    .Where(r => r.Order.ProfileId == userId).ToList();
            int Sum = 0;
            List<MessageDate> tRow =new List<MessageDate>();
            foreach (var row in orders)
            {

                tRow.Add(new MessageDate()
                {
                    text = "<tr><td colspan='2'><strong>" + row.Date.ToString("dd-MM-yyyy") + "</strong></td>" +
                            "<td colspan = '3' align = 'right'><a href = '/Employee/MakeOrder/" + row.Id + "' class='btn btn-sm btn-info'>Повторить заказ</a>" +
                            "</td></tr>"
                });
                Sum = 0;
                foreach(var _row in orderDetails)
                {
                    if (_row.OrderId == row.Id)
                    {
                        tRow.Add(new MessageDate()
                        { text = "<tr><td></td ><td align = 'left'>" + _row.Dish.Name +
                                "</td><td align = 'right'>" + _row.Quantity.ToString() +
                                "</td><td align = 'right'>" + _row.Dish.Price.ToString() +
                                "</td><td align = 'right'>" + (_row.Quantity * _row.Dish.Price).ToString() +
                                "</td></tr>"
                        });
                
                    }
                }
                tRow.Add(new MessageDate()
                    { text ="<tr><td colspan = '3'></td><td align = 'left'>Итого:</td><td align = 'right'>"+Sum+"</td></tr>"});
            }

            return Json(tRow, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetGoalsPage(jPage jPage)
        {
            int userId = int.Parse(HttpContext.Request.Cookies["UserId"].Value);
            List<Goal> goals = db.Goals.Where(f => f.ProfileId == userId).OrderBy(f => f.DateTo).ToList();

            goals = goals.Skip((jPage.page - 1) * PageSize).Take(PageSize).ToList();
            DateTime Today = DateTime.Today;
            List<MessageDate> tRow = new List<MessageDate>();
            foreach (Goal goal in goals)
            {

                tRow.Add(new MessageDate()
                {
                    text ="<div class='card mb-3'><div class='row no-gutters'><div class='col-md-8'><div class='card-body'>"+
                           (goal.Status == 1 ? "<img src='/Images/check-circle.svg'>" :
                           ((goal.DateTo - Today).Days <= 7 ? "<img src='/Images/bell.svg'>" :
                           ((goal.DateTo < Today) ? "<img src='/Images/alert-circle.svg'>" : "<img src='/Images/coffee.svg'>")))+
                           "<p><strong>"+goal.DateTo.ToShortDateString()+"</strong></p>"+
                           "<a href = '/Employee/MyGoal/"+goal.Id+"' >"+
                            goal.Name+"</a></div></div></div></div>"

                });
            }

            return Json(tRow, JsonRequestBehavior.AllowGet);
        }

    }
}