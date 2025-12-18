namespace FitnessCenterManagement.Models
{
    public class Member
    {
        public int Id { get; set; }   // Primary Key

        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
