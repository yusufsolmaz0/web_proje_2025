using System;

namespace FitnessCenterManagement.Models
{
    public class AppointmentListVM
    {
        public int Id { get; set; }
        public string MemberName { get; set; } = "";
        public string TrainerName { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public DateTime StartTime { get; set; }
        public bool IsApproved { get; set; }
    }
}
