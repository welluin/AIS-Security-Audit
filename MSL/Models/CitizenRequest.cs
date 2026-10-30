namespace MSL.Models
{
    public class CitizenRequest
    {
        public int Id { get; set; }

        public string CitizenFio { get; set; } = string.Empty;

        public string RequestText { get; set; } = string.Empty;

        public int? AssignedEmployeeId { get; set; }

        public Employee? AssignedEmployee { get; set; }

        public string Status { get; set; } = "В обработке";
    }
}