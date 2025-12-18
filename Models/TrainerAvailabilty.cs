using System;

namespace FitnessCenterManagement.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }   // Primary Key

        public int TrainerId { get; set; }

        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableTo { get; set; }
    }
}
