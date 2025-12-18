using System;

namespace FitnessCenterManagement.Models
{
    public class ExerciseSuggestion
    {
        public int Id { get; set; }   // Primary Key

        public int? MemberId { get; set; }

        public string Prompt { get; set; } = "";
        public string Response { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
