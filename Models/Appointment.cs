using System;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public bool IsApproved { get; set; } = false;
    }
}
