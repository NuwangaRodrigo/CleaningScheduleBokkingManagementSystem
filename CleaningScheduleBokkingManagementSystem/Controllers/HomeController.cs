using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CleaningScheduleBokkingManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetInitialSchedules()
        {
            using (ScheduleBookingDBEntities dc = new ScheduleBookingDBEntities())
            {
                var events = dc.CleaningSchedules.ToList();
                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
        [HttpPost]
        public JsonResult SaveScedule(CleaningSchedule e)
        {
            var Status = false;
            using (ScheduleBookingDBEntities dc = new ScheduleBookingDBEntities())
            {
                if (e.WeekNumber > 0)
                {
                    // Update schedule
                    var v = dc.CleaningSchedules.Where(a => (a.WeekNumber == e.WeekNumber && a.SlotNumber == e.SlotNumber)).FirstOrDefault();
                    if (v!=null)
                    {
                        v.AssignedPersonId = e.AssignedPersonId;
                        v.AssignedPersonName = e.AssignedPersonName;
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
            }
                return new JsonResult { Data = new { status = Status } };
        }
        [HttpPost]
        public JsonResult DeleteSchedule(int weekNumber, int slotNumber)
        {
            var Status = false;
            using (ScheduleBookingDBEntities dc = new ScheduleBookingDBEntities())
            {
                var v= dc.CleaningSchedules.Where(a => (a.WeekNumber == weekNumber && a.SlotNumber == slotNumber)).FirstOrDefault();
                if(v!= null)
                {
                    dc.CleaningSchedules.Remove(v);
                    dc.SaveChanges();
                    Status = true;
                }
            }
            return new JsonResult { Data = new { status = Status } };
        }
    }
}