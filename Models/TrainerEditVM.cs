using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class TrainerEditVM
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = "";

        public List<int> SelectedServiceIds { get; set; } = new List<int>();

        public List<ServiceCheckboxVM> AllServices { get; set; } = new List<ServiceCheckboxVM>();
    }

    public class ServiceCheckboxVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsSelected { get; set; }
    }
}
