using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class ServiceEditVM
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public int DurationMinutes { get; set; }

        public decimal Price { get; set; }

        // ✅ Bu servisi yapabilen trainerların id'leri
        public List<int> SelectedTrainerIds { get; set; } = new List<int>();

        // ✅ Sayfada checkbox basmak için
        public List<TrainerCheckboxVM> AllTrainers { get; set; } = new List<TrainerCheckboxVM>();
    }

    public class TrainerCheckboxVM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public bool IsSelected { get; set; }
    }
}
