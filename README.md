# Academia

**Academia** is a web-based learning management system built with ASP.NET Core. It supports course delivery, lessons, assessments, enrollments, and role-based dashboards for admins, teachers, and students—aligned with quality education goals (UN SDG 4).

## Features

- **Role-based access** — Admin, Teacher, and Student roles with tailored navigation and permissions
- **Courses** — Create, edit, publish, and search courses; enroll students; assign teachers
- **Lessons & assessments** — Structured content and graded submissions per course
- **Dashboard** — Overview stats (courses, enrollments, average scores) and recent activity
- **To-do list** — Students see upcoming assessments and deadlines in one place
- **Notifications** — In-app alerts for course and assessment activity
- **Student management** — Admins and teachers can browse student profiles, enrollments, and submissions

## Tech stack

- ASP.NET Core MVC (.NET 10)
- ASP.NET Core Identity
- Entity Framework Core with SQL Server (LocalDB)
- Bootstrap 5 and Font Awesome

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio or SQL Server Express)

## Getting started

1. **Clone or download** the repository and open the project folder.

2. **Restore and run:**

   ```bash
   dotnet restore
   dotnet run
   ```

3. Open the URL shown in the console (typically `https://localhost:7044` or `http://localhost:5035`).

On first run, the app creates the database schema and seeds demo roles and users.

### Connection string

The default connection in `appsettings.json` uses LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=AcademiaDb;...
```

Change this if you use a different SQL Server instance.

## Demo accounts

| Role    | Email                 | Password     |
|---------|-----------------------|--------------|
| Admin   | admin@academia.com    | Admin@123    |
| Student | student01@academia.com … student20@academia.com | Student@123 |
| (see login page) | stelayebra@gmail.com | student123 |
| (see login page) | trisha@gmail.com     | teacher1234 |

Additional demo credentials may appear on the login screen for teacher/student test accounts.

## Project structure

```
Academia/
├── Controllers/     # MVC controllers (Account, Courses, Dashboard, etc.)
├── Data/            # EF Core DbContext
├── Models/          # Entities and view models
├── Services/        # Notification service
├── Views/           # Razor views
├── wwwroot/         # Static assets
└── Program.cs       # App startup, Identity, database seeding
```

## Development

- **VS Code** — Use the included `.vscode/launch.json` and `tasks.json` to build and debug `Academia.csproj`.
- **Visual Studio** — Open `Academia.csproj` and run with the **https** profile.

## License

This project is provided as-is for educational use. Add a license file if you plan to distribute it.
