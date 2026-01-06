using System.Collections.Generic;
using KursovaRabotaCS20252026.Models;

namespace KursovaRabotaCS20252026.Data
{
    public class Store
    {
        public List<Student> Students { get; set; } = new();
        public List<Book> Books { get; set; } = new();
        public List<Loan> Loans { get; set; } = new();
    }
}
