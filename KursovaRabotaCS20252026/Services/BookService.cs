using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KursovaRabotaCS20252026.Models;

namespace KursovaRabotaCS20252026.Services
{
	public class BookService
	{
		private readonly List<Book> _books;
		private readonly Func<Task>? _saveCallback;

		public BookService(List<Book> books, Func<Task>? saveCallback = null)
		{
			_books = books ?? throw new ArgumentNullException(nameof(books));
			_saveCallback = saveCallback;
		}

		public IEnumerable<Book> GetAll() => _books.OrderBy(b => b.Id);

		public Book? GetById(int id) => _books.FirstOrDefault(b => b.Id == id);

		public void Add(Book book)
		{
			if (book == null) throw new ArgumentNullException(nameof(book));
			if (string.IsNullOrWhiteSpace(book.Title)) throw new ArgumentException("Title is required.");

			book.Id = _books.Any() ? _books.Max(b => b.Id) + 1 : 1;
			_books.Add(book);
			_ = _saveCallback?.Invoke();
		}

		public void Update(Book book)
		{
			var existing = GetById(book.Id) ?? throw new InvalidOperationException("Book not found.");
			existing.Title = book.Title;
			existing.Author = book.Author;
			existing.IsBorrowed = book.IsBorrowed;
			_ = _saveCallback?.Invoke();
		}

		public void Delete(int id)
		{
			_books.RemoveAll(b => b.Id == id);
			_ = _saveCallback?.Invoke();
		}

		public IEnumerable<Book> Search(string q)
		{
			if (string.IsNullOrWhiteSpace(q)) return Enumerable.Empty<Book>();
			return _books.Where(b =>
				b.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
				b.Author.Contains(q, StringComparison.OrdinalIgnoreCase));
		}
	}
}