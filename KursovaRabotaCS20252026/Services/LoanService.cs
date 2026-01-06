using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KursovaRabotaCS20252026.Models;

namespace KursovaRabotaCS20252026.Services
{
	public class LoanService
	{
		private readonly List<Loan> _loans;
		private readonly List<Book> _books;
		private readonly List<Student> _students;
		private readonly Func<Task>? _saveCallback;

		public LoanService(List<Loan> loans, List<Book> books, List<Student> students, Func<Task>? saveCallback = null)
		{
			_loans = loans ?? throw new ArgumentNullException(nameof(loans));
			_books = books ?? throw new ArgumentNullException(nameof(books));
			_students = students ?? throw new ArgumentNullException(nameof(students));
			_saveCallback = saveCallback;
		}

		public IEnumerable<Loan> GetAll() => _loans.OrderBy(l => l.LoanDate);

		public IEnumerable<Loan> ActiveLoans() => _loans.Where(l => !l.IsReturned);

		public bool IsBookAvailable(int bookId) => !_loans.Any(l => l.BookId == bookId && !l.IsReturned);

		public Loan Borrow(int bookId, int studentId)
		{
			var student = _students.FirstOrDefault(s => s.Id == studentId) ?? throw new InvalidOperationException("Student not found");
			var book = _books.FirstOrDefault(b => b.Id == bookId) ?? throw new InvalidOperationException("Book not found");
			if (!IsBookAvailable(bookId)) throw new InvalidOperationException("Book already loaned");

			var loan = new Loan
			{
				Id = _loans.Any() ? _loans.Max(l => l.Id) + 1 : 1,
				BookId = bookId,
				StudentId = studentId,
				LoanDate = DateTime.UtcNow
			};
			_loans.Add(loan);
			book.IsBorrowed = true;
			_ = _saveCallback?.Invoke();
			return loan;
		}

		public void Return(int loanId)
		{
			var loan = _loans.FirstOrDefault(l => l.Id == loanId) ?? throw new InvalidOperationException("Loan not found");
			if (loan.IsReturned) throw new InvalidOperationException("Already returned");
			loan.ReturnDate = DateTime.UtcNow;
			var book = _books.FirstOrDefault(b => b.Id == loan.BookId);
			if (book != null) book.IsBorrowed = false;
			_ = _saveCallback?.Invoke();
		}
	}
}