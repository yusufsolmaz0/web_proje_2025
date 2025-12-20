using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public int DurationMinutes { get; set; }

        public decimal Price { get; set; }

        // ✅ Bu servisi yapabilen trainerlar
        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}
