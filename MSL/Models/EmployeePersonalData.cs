namespace MSL.Models
{
    public class EmployeePersonalData
    {
        public int EmployeeId { get; set; }

        public string PassportData { get; set; } = string.Empty;

        public Employee? Employee { get; set; }
    }
}