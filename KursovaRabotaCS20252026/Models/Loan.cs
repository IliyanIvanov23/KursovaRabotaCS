using System;

namespace KursovaRabotaCS20252026.Models
{
	public class Loan
	{
		public int Id { get; set; }
		public int StudentId { get; set; }
		public int BookId { get; set; }
		public DateTime LoanDate { get; set; }
		public DateTime? ReturnDate { get; set; }
		public bool IsReturned => ReturnDate.HasValue;
	}
}
