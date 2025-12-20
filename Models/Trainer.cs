using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = "";

        // ✅ Trainer'ın yapabildiği servisler
        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
