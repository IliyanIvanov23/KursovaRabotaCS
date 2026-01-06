# Student Management & Library Assistant

## Description
A C# console application for managing students and a library system.

## Features
- Create, edit, and delete students
- Create, edit, and delete books
- Borrow and return books
- LINQ-based search
- JSON data storage
- Async/await operations
- Error handling with try/catch
- Role-based (Admin/Student) menu access

## Requirements
- .NET 8 SDK
- Visual Studio 2022 or `dotnet` CLI

## How to Run
1. Open the solution in Visual Studio or use the `dotnet` CLI.
2. From project folder run:

```
cd KursovaRabotaCS20252026
dotnet run
```

Data is persisted to `Data/data.json` in the project working directory.

## Tests
A test project using xUnit is included. Run tests with:

```
cd KursovaRabotaCS20252026.Tests
dotnet test
