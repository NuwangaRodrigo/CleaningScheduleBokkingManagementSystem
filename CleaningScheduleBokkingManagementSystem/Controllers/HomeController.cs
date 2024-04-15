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
        BookingScheduleManagementSystemEntities2 dc = new BookingScheduleManagementSystemEntities2();
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
            var events = dc.CleaningSchedules
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
        public JsonResult ChooseScedule(CleaningSchedule e)
        {
            var Status = false;
            // using (BookingScheduleManagementSystemEntities dc = new BookingScheduleManagementSystemEntities())
            //{
            if (e.WeekNumber > 0)
            {
                // Update schedule
                var v = dc.CleaningSchedules.Where(a => (a.WeekNumber == e.WeekNumber && a.SlotNumber == e.SlotNumber)).FirstOrDefault();
                if (v != null)


                {
                    v.Resident_Id = (int?)Session["Resident_Id"];
                    v.Theme_Colour = "#378006";
                    v.Is_FullDay = true;
                }
            }

            else
            {
                dc.CleaningSchedules.Add(e);
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


            var v = dc.CleaningSchedules.Where(a => (a.WeekNumber == weekNumber && a.SlotNumber == slotNumber)).FirstOrDefault();
            if (v != null)
            {
                // dc.CleaningSchedules.Remove(v);
                v.Resident_Id = 0;

                v.Theme_Colour = "#AA336A";
                dc.SaveChanges();
                Status = true;
            }

            return new JsonResult { Data = new { status = Status } };
        }
        public ActionResult Group()
        {

            if (Session["Resident_Id"] != null)
            {
                // Retrieve the Resident_Id from the session
                residentId = (int)Session["Resident_Id"];
                var resident = dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId);
                if (resident == null)
                {
                    return HttpNotFound();
                }

                int groupId = resident.Group_Id;
                //load same group members
                var users = (from r in dc.RESIDENTS
                             join g in dc.GROUPS on r.Group_Id equals g.Group_Id
                             where r.Group_Id == groupId && r.Resident_Id != 0
                             select new UserDetailsViewModel
                             {
                                 FullName = r.Full_Name,
                                 ContactNo = r.Contact_No,
                                 RoomNumber = r.Room_Id,
                                 GroupName = g.Group_Name
                             }).ToList();

                return View(users);
            }
            return RedirectToAction("Login", "Login");
        }
        public JsonResult SchedulePlaning()
        {
            var events = dc.CleaningSchedules
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
                var eventsForDate = dc.CleaningSchedules
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
                var eventsOnSameDay = dc.CleaningSchedules.Count(e => e.Start_Date.HasValue && e.Start_Date.Value.Date == selectedDate);
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
                         join g in dc.GROUPS on r.Group_Id equals g.Group_Id
                         where r.Group_Id == groupId
                         select new UserDetailsViewModel
                         {
                             FullName = r.Full_Name,
                             ContactNo = r.Contact_No,
                             RoomNumber = r.Room_Id,
                             GroupName = g.Group_Name
                         }).ToList();

            return View(users);
        }
        public ActionResult Register()
        {
            if (Session["Resident_Id"] != null)
            {
                // Retrieve the Resident_Id from the session
                residentId = (int)Session["Resident_Id"];
                var resident = dc.RESIDENTS.FirstOrDefault(r => r.Resident_Id == residentId);
                if (resident == null)
                {
                    return HttpNotFound();
                }

                var details = dc.RESIDENTS
     .Join(dc.ROOMS, cs => cs.Room_Id, r => r.Room_Id,
         (cs, r) => new { cs, r })
     .Join(dc.FLOORS, fs => fs.r.Floor_Id, f => f.Floor_Id,
         (joinedData, f) => new { joinedData.cs, joinedData.r, f })
     .Join(dc.BUILDINGS, bs => bs.f.Building_Id, b => b.Building_Id,
         (joinedData, b) => new { joinedData.cs, joinedData.r, joinedData.f, b })
     .Select(joinedData => new
     {
         joinedData.cs.Full_Name,
         joinedData.cs.Contact_No,
         joinedData.cs.Email,
         ResidentName = (joinedData.cs.Resident_Id != 0) ? "No Name to Display" : joinedData.cs.Full_Name
     })
     .ToList();

                return View(details);
            }
            return RedirectToAction("Login", "Login");
        }

        //POST: Account/RegisterUser
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult RegisterUser(RESIDENT RESIDENTS)
        {
            var Status = false;

            using (var db = new BookingScheduleManagementSystemEntities2())
            {
                RESIDENTS.Group_Id = 1;
                RESIDENTS.Is_Admin = false;
                db.RESIDENTS.Add(RESIDENTS);
                db.SaveChanges();

                Status = true;
            }

            return new JsonResult { Data = new { status = Status } };
        }
    }
}
    
