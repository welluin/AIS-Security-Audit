namespace MSL.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string Login { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string PasswordSalt { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        public EmployeePersonalData? PersonalData { get; set; }

        public int ClearanceLevel { get; set; }
    }
}