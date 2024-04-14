using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CleaningScheduleBokkingManagementSystem.Models
{
    public class UserDetailsViewModel
    {
        public string FullName { get; set; }
        public int ContactNo { get; set; }
        public int RoomNumber { get; set; }
        public string GroupName { get; set; }
    }
}