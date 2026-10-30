namespace MSL.Models
{
    public class ClassifiedDocument
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public SecurityLevel SecretLevel { get; set; }

        public int AuthorId { get; set; }

        public Employee? Author { get; set; }

        public int DepartmentId { get; set; }

        public Department? Department { get; set; }
    }
}