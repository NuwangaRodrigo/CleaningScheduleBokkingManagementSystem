using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CleaningScheduleBokkingManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        BookingScheduleManagementSystemEntities2 db = new BookingScheduleManagementSystemEntities2();
        public int userId;
        // GET: Login
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoginUser(USER e)
        {
            var user = db.USERS.Where(x => x.User_Name == e.User_Name && x.Password == e.Password).Count();
            var user_id = db.USERS.FirstOrDefault(x => x.User_Name == e.User_Name && x.Password == e.Password);
            if (user > 0 && user_id != null)
            {
                userId = user_id.Resident_Id;
                Session["Resident_Id"] = userId;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return Json(new { errorMessage = "Invalid username or password." });
            }
        }
        public ActionResult UserDashBoard()
        {
            if (Session["User_Name"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
    }
}