using System.Linq;
using KursovaRabotaCS20252026.Data;
using KursovaRabotaCS20252026.Models;
using KursovaRabotaCS20252026.Services;

internal class Program
{
    private static Store _store = new Store();
    private static StudentService _studentService;
    private static BookService _bookService;
    private static LoanService _loanService;
    private static Student? _currentUser;

    private static async Task Main(string[] args)
    {
        try
        {
            _store = await FileService.LoadAsync<Store>();
            Func<Task> saveAll = async () => await FileService.SaveAsync(_store);
            _studentService = new StudentService(_store.Students, saveAll);
            _bookService = new BookService(_store.Books, saveAll);
            _loanService = new LoanService(_store.Loans, _store.Books, _store.Students, saveAll);

            EnsureAdminExists();

            while (await LoginAsync())
            {
                if (_currentUser!.UserRole == Role.Admin) await AdminLoopAsync(); else await StudentLoopAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void EnsureAdminExists()
    {
        if (_studentService.FindByFacultyNumber("AI1498") is null)
        {
            var a = new Student { Name = "Iliyan Ivanov", FacultyNumber = "AI1498", UserRole = Role.Admin, Password = "admin123" };
            _studentService.Add(a);
            FileService.SaveAsync(_store).GetAwaiter().GetResult();
        }
    }

    private static async Task<bool> LoginAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Login ===\n1) Login\n2) Register (student)\n0) Exit");
            switch ((Console.ReadLine() ?? string.Empty).Trim())
            {
                case "1":
                    Console.Write("Faculty number: ");
                    var fn = (Console.ReadLine() ?? string.Empty).Trim();
                    Console.Write("Password: ");
                    var pw = ReadPassword();
                    _currentUser = _studentService.Authenticate(fn, pw);
                    if (_currentUser is null) { Console.WriteLine("Invalid credentials."); Pause(); continue; }
                    Console.WriteLine($"Welcome, {_currentUser.Name}!"); await Task.Delay(300); return true;
                case "2": await RegisterStudentAsync(); break;
                case "0": return false;
                default: Console.WriteLine("Invalid option."); Pause(); break;
            }
        }
    }

    private static async Task RegisterStudentAsync()
    {
        Console.Clear();
        Console.WriteLine("=== Student Registration ===");
        Console.Write("Full name: "); var name = (Console.ReadLine() ?? string.Empty).Trim();
        Console.Write("Faculty number: "); var fn = (Console.ReadLine() ?? string.Empty).Trim();
        Console.Write("Password: "); var pw = ReadPassword();
        try { _studentService.Add(new Student { Name = name, FacultyNumber = fn, Password = pw }); await FileService.SaveAsync(_store); Console.WriteLine("Registered."); }
        catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
        Pause();
    }

    private static async Task AdminLoopAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== Admin ({_currentUser.Name}) ===\n1) Students 2) Books 3) Loans 4) Search 0) Logout");
            var pick = (Console.ReadLine() ?? string.Empty).Trim();
            if (pick == "0") { _currentUser = null; return; }
            if (pick == "1") await StudentsMenuAsync(true);
            else if (pick == "2") await BooksMenuAsync();
            else if (pick == "3") await LoansMenuAsync();
            else if (pick == "4") SearchMenu();
            else { Console.WriteLine("Invalid"); Pause(); }
        }
    }

    private static async Task StudentLoopAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== Student ({_currentUser.Name}) ===\n1) Books 2) Search 3) Borrow 4) Return 5) My Loans 0) Logout");
            var pick = (Console.ReadLine() ?? string.Empty).Trim();
            if (pick == "0") { _currentUser = null; return; }
            if (pick == "1") PrintBooksTable(_bookService.GetAll());
            else if (pick == "2") { Console.Write("Query: "); PrintBooksTable(_bookService.Search(Console.ReadLine() ?? string.Empty)); }
            else if (pick == "3") await BorrowFlowAsync(_currentUser);
            else if (pick == "4") await ReturnFlowAsync(_currentUser);
            else if (pick == "5") PrintLoansTable(_loanService.GetAll().Where(l => l.StudentId == _currentUser.Id));
            else { Console.WriteLine("Invalid"); }
            Pause();
        }
    }

    private static async Task StudentsMenuAsync(bool isAdmin)
    {
        while (true)
        {
            Console.Clear(); Console.WriteLine("Students: 1) List 2) Add 3) Delete 4) Back");
            switch ((Console.ReadLine() ?? string.Empty).Trim())
            {
                case "1": PrintStudentsTable(_studentService.GetAll()); break;
                case "2":
                    Console.Write("Name: "); var name = (Console.ReadLine() ?? string.Empty).Trim();
                    Console.Write("Faculty#: "); var fn = (Console.ReadLine() ?? string.Empty).Trim();
                    Console.Write("Password: "); var pw = ReadPassword();
                    Console.Write("Role (1=Admin,2=Student): "); var r = (Console.ReadLine() ?? "").Trim();
                    var role = r == "1" ? Role.Admin : Role.Student;
                    try { _studentService.Add(new Student { Name = name, FacultyNumber = fn, Password = pw, UserRole = role }); await FileService.SaveAsync(_store); Console.WriteLine("Added."); }
                    catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
                    break;
                case "3":
                    if (!isAdmin) { Console.WriteLine("Admin only"); break; }
                    var idd = PromptInt("Id to delete: "); if (idd >= 0) { _studentService.Remove(idd); await FileService.SaveAsync(_store); Console.WriteLine("Deleted"); }
                    break;
                case "4": return;
                default: Console.WriteLine("Invalid"); break;
            }
            Pause();
        }
    }

    private static async Task BooksMenuAsync()
    {
        while (true)
        {
            Console.Clear(); Console.WriteLine("Books: 1) List 2) Add 3) Edit 4) Delete 5) Back");
            switch ((Console.ReadLine() ?? string.Empty).Trim())
            {
                case "1": PrintBooksTable(_bookService.GetAll()); break;
                case "2": Console.Write("Title: "); var t = Console.ReadLine() ?? string.Empty; Console.Write("Author: "); var a = Console.ReadLine() ?? string.Empty; _bookService.Add(new Book{ Title = t.Trim(), Author = a.Trim() }); await FileService.SaveAsync(_store); Console.WriteLine("Added"); break;
                case "3": var eid = PromptInt("Id to edit: "); if (eid<0) break; var eb = _bookService.GetById(eid); if (eb == null) { Console.WriteLine("Not found"); break; } Console.Write($"Title ({eb.Title}): "); var nt = Console.ReadLine(); Console.Write($"Author ({eb.Author}): "); var na = Console.ReadLine(); eb.Title = string.IsNullOrWhiteSpace(nt) ? eb.Title : nt.Trim(); eb.Author = string.IsNullOrWhiteSpace(na) ? eb.Author : na.Trim(); _bookService.Update(eb); await FileService.SaveAsync(_store); Console.WriteLine("Updated"); break;
                case "4": var bid = PromptInt("Id to delete: "); if (bid>=0) { _bookService.Delete(bid); await FileService.SaveAsync(_store); Console.WriteLine("Deleted"); } break;
                case "5": return;
                default: Console.WriteLine("Invalid"); break;
            }
            Pause();
        }
    }

    private static async Task LoansMenuAsync()
    {
        while (true)
        {
            Console.Clear(); Console.WriteLine("Loans: 1) List 2) Borrow 3) Return 4) Active 5) Back");
            switch ((Console.ReadLine() ?? string.Empty).Trim())
            {
                case "1": PrintLoansTable(_loanService.GetAll()); break;
                case "2":
                    Console.WriteLine("Students:"); PrintStudentsTable(_studentService.GetAll()); var sid = PromptInt("Student Id: "); if (_studentService.GetById(sid) == null) { Console.WriteLine("Not found"); break; }
                    Console.WriteLine("Books:"); PrintBooksTable(_bookService.GetAll()); var bid2 = PromptInt("Book Id: "); var bobj = _bookService.GetById(bid2); if (bobj==null) { Console.WriteLine("Not found"); break; } if (bobj.IsBorrowed) { Console.WriteLine("Already borrowed"); break; }
                    try { _loanService.Borrow(bid2, sid); await FileService.SaveAsync(_store); Console.WriteLine("Borrowed"); } catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
                    break;
                case "3": var lid = PromptInt("Loan Id to return: "); if (lid<0) break; try { _loanService.Return(lid); await FileService.SaveAsync(_store); Console.WriteLine("Returned"); } catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); } break;
                case "4": PrintLoansTable(_loanService.ActiveLoans()); break;
                case "5": return;
                default: Console.WriteLine("Invalid"); break;
            }
            Pause();
        }
    }

    private static void SearchMenu()
    {
        Console.Clear(); Console.WriteLine("Search: 1)Students 2)Books 3)Back");
        switch ((Console.ReadLine() ?? string.Empty).Trim())
        {
            case "1": Console.Write("Query: "); var q = Console.ReadLine() ?? string.Empty; PrintStudentsTable(_studentService.GetAll().Where(s => s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || s.FacultyNumber.Contains(q, StringComparison.OrdinalIgnoreCase))); break;
            case "2": Console.Write("Query: "); PrintBooksTable(_bookService.Search(Console.ReadLine() ?? string.Empty)); break;
            case "3": return;
            default: Console.WriteLine("Invalid"); break;
        }
        Pause();
    }

    private static async Task BorrowFlowAsync(Student student)
    {
        Console.Clear(); Console.WriteLine("=== Borrow Book ==="); PrintBooksTable(_bookService.GetAll());
        var bid = PromptInt("Enter book id to borrow (or 0 to cancel): "); if (bid <= 0) { Console.WriteLine("Cancelled"); return; }
        try { var loan = _loanService.Borrow(bid, student.Id); await FileService.SaveAsync(_store); Console.WriteLine($"Borrowed. Loan id: {loan.Id}"); }
        catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
    }

    private static async Task ReturnFlowAsync(Student student)
    {
        Console.Clear(); Console.WriteLine("=== Return Book ==="); var myActive = _loanService.ActiveLoans().Where(l => l.StudentId == student.Id).ToList(); if (!myActive.Any()) { Console.WriteLine("No active loans."); return; } PrintLoansTable(myActive);
        var lid = PromptInt("Enter loan id to return: "); if (lid < 0) return; try { _loanService.Return(lid); await FileService.SaveAsync(_store); Console.WriteLine("Returned"); } catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
    }

    private static void PrintBooksTable(IEnumerable<Book> books)
    {
        var list = books.ToList();
        const int idW = 4;
        const int titleW = 40;
        const int authorW = 20;
        var hdr = $"{Pad("Id", idW)} | {Pad("Title", titleW)} | {Pad("Author", authorW)} | Status";
        Console.WriteLine(hdr);
        Console.WriteLine(new string('-', hdr.Length));
        foreach (var b in list)
        {
            var status = b.IsBorrowed ? "Borrowed" : "Available";
            Console.WriteLine($"{Pad(b.Id.ToString(), idW)} | {Pad(Truncate(b.Title, titleW), titleW)} | {Pad(Truncate(b.Author, authorW), authorW)} | {status}");
        }
    }

    private static void PrintStudentsTable(IEnumerable<Student> students)
    {
        var list = students.ToList();
        const int idW = 4;
        const int nameW = 30;
        const int facW = 12;
        var hdr = $"{Pad("Id", idW)} | {Pad("Name", nameW)} | {Pad("Faculty#", facW)} | Role";
        Console.WriteLine(hdr);
        Console.WriteLine(new string('-', hdr.Length));
        foreach (var s in list)
        {
            Console.WriteLine($"{Pad(s.Id.ToString(), idW)} | {Pad(Truncate(s.Name, nameW), nameW)} | {Pad(s.FacultyNumber, facW)} | {s.UserRole}");
        }
    }

    private static void PrintLoansTable(IEnumerable<Loan> loans)
    {
        var list = loans.ToList();
        const int idW = 4;
        const int bookW = 30;
        const int studentW = 20;
        const int dateW = 16; // for short date/time
        var hdr = $"{Pad("Id", idW)} | {Pad("Book", bookW)} | {Pad("Student", studentW)} | {Pad("Loaned", dateW)} | Returned";
        Console.WriteLine(hdr);
        Console.WriteLine(new string('-', hdr.Length));
        foreach (var l in list)
        {
            var bookTitle = _bookService.GetById(l.BookId)?.Title ?? ($"Id {l.BookId}");
            var studentName = _studentService.GetById(l.StudentId)?.Name ?? ($"Id {l.StudentId}");
            var loaned = l.LoanDate.ToLocalTime().ToString("g");
            var returned = l.ReturnDate?.ToLocalTime().ToString("g") ?? "-";
            Console.WriteLine($"{Pad(l.Id.ToString(), idW)} | {Pad(Truncate(bookTitle, bookW), bookW)} | {Pad(Truncate(studentName, studentW), studentW)} | {Pad(loaned, dateW)} | {returned}");
        }
    }

    // --- Utilities ---
    private static int PromptInt(string prompt)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out var v)) return v;
        Console.WriteLine("Invalid number"); return -1;
    }

    private static void Pause() { Console.WriteLine("Press any key..."); Console.ReadKey(true); }
    private static string Pad(string s, int width) => s.PadRight(width);
    private static string Truncate(string s, int max) => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s : s.Substring(0, max - 1) + "…");

    private static string ReadPassword()
    {
        var pass = string.Empty;
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace && pass.Length > 0) { pass = pass[0..^1]; Console.Write("\b \b"); }
            else if (!char.IsControl(key.KeyChar)) { pass += key.KeyChar; Console.Write('*'); }
        }
        Console.WriteLine();
        return pass;
    }
}