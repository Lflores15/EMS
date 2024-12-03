using EMS.Models;
using System.Collections.Generic;

namespace EMS.Models
{
    public class AdminDashboardViewModel
    {
        public List<Event> Events { get; set; }
        public List<User> Users { get; set; }
    }
}
