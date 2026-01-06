# Student Management & Library Assistant

A minimal C# console application for managing students and a small library system. The app supports an admin account and student accounts, book CRUD, borrowing and returning, and data persistence in JSON.

## Features
- Role-based menus: Admin and Student
- Create / edit / delete students and books
- Borrow and return books (loan tracking)
- Search students and books using LINQ
- Data persisted as JSON files
- Console UI with simple, aligned tables

## Requirements
- .NET 8 SDK
- Visual Studio 2022 or `dotnet` CLI

## Run (CLI)
1. Open a terminal in the project folder:

   ```bash
   cd KursovaRabotaCS20252026
   dotnet run
   ```

2. On first run the app will create or use `Data/data.json` in the project working directory to store students, books and loans.

## Default Admin
- Faculty number: `AI1498`
- Password: `admin123`

Use the admin account to manage students, books and loans.

## Data and Passwords
- All application data is stored in `Data/data.json` (relative to the working directory).
- For this simple exercise passwords are stored in plain text in the JSON file. Do not use real passwords.

## License
This project contains example code for educational use. No license is attached.
