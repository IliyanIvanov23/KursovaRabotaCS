using KursovaRabotaCS20252026.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaRabotaCS20252026.Services
{
	public class StudentService
	{
		private readonly List<Student> students;
		private readonly Func<Task>? _saveCallback;

		public StudentService(List<Student> students, Func<Task>? saveCallback = null)
		{
			this.students = students;
			_saveCallback = saveCallback;
		}

		public IEnumerable<Student> GetAll() => students.OrderBy(s => s.Id);

		public void Add(Student student)
		{
			if (student == null) throw new ArgumentNullException(nameof(student));
			if (string.IsNullOrWhiteSpace(student.Name)) throw new ArgumentException("Name is required.");
			if (string.IsNullOrWhiteSpace(student.FacultyNumber)) throw new ArgumentException("Faculty number is required.");

			student.Id = students.Any() ? students.Max(s => s.Id) + 1 : 1;
			students.Add(student);
			_ = _saveCallback?.Invoke();
		}

		public Student? FindByFacultyNumber(string fn)
		{
			if (string.IsNullOrWhiteSpace(fn)) return null;
			return students.FirstOrDefault(s => s.FacultyNumber.Equals(fn, StringComparison.OrdinalIgnoreCase));
		}

		public Student? GetById(int id) => students.FirstOrDefault(s => s.Id == id);

		public void Remove(int id)
		{
			students.RemoveAll(s => s.Id == id);
			_ = _saveCallback?.Invoke();
		}

		public Student? Authenticate(string facultyNumber, string password)
		{
			if (string.IsNullOrWhiteSpace(facultyNumber) || password == null) return null;
			return students.FirstOrDefault(s => s.FacultyNumber.Equals(facultyNumber, StringComparison.OrdinalIgnoreCase) && s.Password == password);
		}
	}
}
