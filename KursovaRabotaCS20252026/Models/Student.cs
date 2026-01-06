using System;

namespace KursovaRabotaCS20252026.Models
{
    public enum Role { Admin, Student }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FacultyNumber { get; set; } = string.Empty;
        public Role UserRole { get; set; } = Role.Student;
        public string Password { get; set; } = string.Empty;
    }
}
