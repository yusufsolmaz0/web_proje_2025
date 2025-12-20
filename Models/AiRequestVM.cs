using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class AiRequestVM
    {
        [Required]
        [Range(120, 220)]
        public int HeightCm { get; set; }

        [Required]
        [Range(30, 250)]
        public int WeightKg { get; set; }

        [Required]
        public string Goal { get; set; } = "Kilo Verme"; // Kilo Verme / Kas Yapma / Fit Kalma

        public string? ExtraInfo { get; set; } // opsiyonel
    }
}
