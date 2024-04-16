using CleaningScheduleBokkingManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CleaningScheduleBokkingManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        BookingScheduleManagementDBEntities dc = new BookingScheduleManagementDBEntities();
        public int residentId;
        // GET: Home
        public ActionResult Index()
        {
            if (Session["Resident_Id"] != null)
            {
               
                residentId = (int)Session["Resident_Id"];

            }
            return View();
        }
        public JsonResult GetInitialSchedules()
        {
            var events = dc.CLEANINGSCHEDULEs
    .Join(dc.RESIDENTS, cs => cs.Resident_Id, r => r.Resident_Id,
        (cs, r) => new { cs, r })
    .Select(joinedData => new
    {
        joinedData.cs.WeekNumber,
        joinedData.cs.SlotNumber,
        joinedData.cs.Start_Date,
        joinedData.cs.End_Date,
        joinedData.cs.Is_Cleaned,
        joinedData.cs.Is_Verified,
        joinedData.cs.Theme_Colour,
        joinedData.cs.Is_FullDay,
        ResidentName = (joinedData.cs.Start_Date != null && joinedData.cs.Resident_Id == null) ? "Choose a slot" : joinedData.r.Full_Name
    })
    .ToList();

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };


        }
        [HttpPost]
        public JsonResult ChooseScedule(CLEANINGSCHEDULE e)
        {
            var Status = false;
            // using (BookingScheduleManagementSystemEntities dc = new BookingScheduleManagementSystemEntities())
            //{
            if (e.WeekNumber > 0)
            {
                // Update schedule
                var v = dc.CLEANINGSCHEDULEs.Where(a => (a.WeekNumber == e.WeekNumber && a.SlotNumber == e.SlotNumber)).FirstOrDefault();
                if (v != null)


                {
                    v.Resident_Id = (int?)Session["Resident_Id"];
                    v.Theme_Colour = "#378006";
                    v.Is_FullDay = true;
                }
            }

            else
            {
                dc.CLEANINGSCHEDULEs.Add(e);
            }
            dc.SaveChanges();
            Status = true;
            //}
            return new JsonResult { Data = new { status = Status } };
        }
        [HttpPost]
        public JsonResult DeleteSchedule(int weekNumber, int slotNumber)
        {
            var Status = false;
            var currentUser = getCurrentUser(); // Get the current user (you need to implement this method)
            
            var v = dc.CLEANINGSCHEDULEs.FirstOrDefault(a => a.WeekNumber == weekNumber && a.SlotNumber == slotNumber);

            if (v != null)
            {
                if ( v.Resident_Id == currentUser.Resident_Id)
                {
                    v.Resident_Id = 0;
                    v.Theme_Colour = "#AA336A";
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    
                    Status = false;
                }
            }
            else
            {
                
                Status = false;
            }

            return Json(new { status = Status });
        }
        public RESIDENT getCurrentUser()
        {
            
            if (User.Identity.IsAuthenticated)
            {
                
                string Email = User.Identity.Name;

                // Assuming you have a method to retrieve user information from your database based on the username
                var currentUser = dc.RESIDENTS.FirstOrDefault(u => u.Email == Email);

                return currentUser;
            }
            else
            {
                // User is not authenticated, return null or handle appropriately
                return null;
            }
        }

        
        public JsonResult SchedulePlaning()
        {
            var events = dc.CLEANINGSCHEDULEs
    .Join(dc.RESIDENTS, cs => cs.Resident_Id, r => r.Resident_Id,
        (cs, r) => new { cs, r })
    .Select(joinedData => new
    {
        joinedData.cs.WeekNumber,
        joinedData.cs.SlotNumber,
        joinedData.cs.Start_Date,
        joinedData.cs.End_Date,
        joinedData.cs.Is_Cleaned,
        joinedData.cs.Is_Verified,
        joinedData.cs.Theme_Colour,
        joinedData.cs.Is_FullDay,
        ResidentName = (joinedData.cs.Start_Date != null && joinedData.cs.Resident_Id == null) ? "Choose a slot" : joinedData.r.Full_Name
    })
    .ToList();

            return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public ActionResult GetEventsForDate(string date)
        {
            DateTime selectedDate;
            if (DateTime.TryParse(date, out selectedDate))
            {
                var eventsForDate = dc.CLEANINGSCHEDULEs
                    .Where(e => e.Start_Date.HasValue && e.Start_Date.Value.Date == selectedDate.Date)
                    .ToList();


                var eventsData = eventsForDate.Select(e => new
                {
                    residentId = 0,
                    start = e.Start_Date.HasValue ? e.Start_Date.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "", // Conditional formatting
                    weekNumber = GetWeekNumber(e.Start_Date),
                    slotNumber = GetSlotNumber(e.Start_Date, selectedDate.Date),
                });

                return Json(eventsData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Invalid date format" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method to get the week number for a given date
        private int GetWeekNumber(DateTime? date)
        {
            if (date.HasValue)
            {
                return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.Value, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            }
            return 0; // Return 0 if date is null
        }

        // Helper method to get the slot number for a given event date and selected date
        private int GetSlotNumber(DateTime? eventDate, DateTime selectedDate)
        {
            if (eventDate.HasValue && eventDate.Value.Date == selectedDate)
            {
                // Assuming slot number is determined by the number of events on the same day
                var eventsOnSameDay = dc.CLEANINGSCHEDULEs.Count(e => e.Start_Date.HasValue && e.Start_Date.Value.Date == selectedDate);
                return eventsOnSameDay + 1;
            }
            return 0; // Return 0 if eventDate is null or different from selectedDate
        }

        public ActionResult GroupDetails()
        {
            var resident = dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == (int)Session["Resident_Id"]);
            if (resident == null)
            {
                return HttpNotFound();
            }

            int groupId = resident.Group_Id;
            //load same group members
            var users = (from r in dc.RESIDENTS
                         join g in dc.GROUPs on r.Group_Id equals g.Group_Id
                         where r.Group_Id == groupId
                         select new UserDetailsViewModel
                         {
                             FullName = r.Full_Name,
                             ContactNo = r.Contact_No,
                             GroupName = g.Group_Name
                         }).ToList();

            return View(users);
        }
        public ActionResult Register()
        {
            
                return View();
            
            
        }
        public ActionResult RegisterUser(RESIDENT RESIDENTS)
        {
            var Status = false;

            using (var db = new BookingScheduleManagementDBEntities())
            {
                RESIDENTS.Group_Id = 1;
                RESIDENTS.Is_Admin = false;
                db.RESIDENTS.Add(RESIDENTS);
                db.SaveChanges();

                Status = true;
            }

            
            return RedirectToAction("Login", "Login");
        }
        public ActionResult Group()
        {
            if (Session["Resident_Id"] != null)
            {

                residentId = (int)Session["Resident_Id"];
                ViewBag.ResidentId = residentId;

            }

            
            return View();

        }
        // GET: Home/GetResidentId
        [HttpPost]
        public ActionResult GetResidentId()
        {
            try
            {
                // Retrieve the resident ID from the session
                int residentId = (int)Session["Resident_Id"];
                return Json(new { success = true, residentId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to get resident ID: " + ex.Message });
            }
        }
        // POST: Group/Create
        [HttpPost]
        public ActionResult Create(GROUP group)
        {
            if (ModelState.IsValid)
            {
                dc.GROUPs.Add(group);
                dc.SaveChanges();

                // Update Is_Admin column in RESIDENTS table
                int residentId = (int)Session["Resident_Id"];
                var resident = dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId);
                if (resident != null)
                {
                    resident.Is_Admin = true;
                    resident.Group_Id = group.Group_Id;
                    dc.SaveChanges(); // Save the changes to the RESIDENTS table
                }

                return RedirectToAction("Index", "Home");
            }
            return View(group);
        }


        // POST: Group/Search
        [HttpPost]
        public ActionResult Search(string email)
        {
            using (var db = new BookingScheduleManagementDBEntities())
            {
                // Query residents based on email
                var residents = db.RESIDENTS
                                   .Where(r => r.Email.Contains(email))
                                   .Select(r => new
                                   {
                                       r.Resident_Id,
                                       r.Full_Name,
                                       r.Contact_No,
                                       r.Email
                                   })
                                   .ToList();

                return Json(residents);
            }
        }
        [HttpPost]
        public ActionResult AddUserToGroup(int residentId, int groupId)
        {
            try
            {
                // Find the resident and group
                var resident = dc.RESIDENTS.Find(residentId);
                var group = dc.GROUPs.Find(groupId);

                if (resident != null && group != null)
                {
                    // Update the resident's group id
                    resident.Group_Id = groupId;

                    // Save changes to the database
                    dc.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Resident or group not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult GetGroupData(int residentId)
        {
            try
            {
                // Check if the resident is an admin
                var resident = dc.RESIDENTS.Find(residentId);
                if (resident != null && resident.Is_Admin)
                {
                    // Get group data if group ID is not equal to 1
                    var groupData = dc.GROUPs.Where(g => g.Group_Id != 1).ToList();
                    return Json(new { success = true, groupData });
                }
                else
                {
                    return Json(new { success = false, message = "Only admins can view group data." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // POST: Group/AddMember
        [HttpPost]
        public ActionResult AddMember(int groupId, int residentId)
        {
            var resident = dc.RESIDENTS.Find(residentId);
            if (resident != null)
            {
                resident.Group_Id = groupId;
                dc.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
    
