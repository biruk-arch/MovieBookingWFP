d# MovieBookingWPF

A WPF movie booking application built with .NET 8 and Entity Framework Core.

## Features

- Login screen with email/password authentication
- Admin dashboard for movie management
  - Add, edit, and delete movies
- Customer dashboard for browsing movies
  - Select showtimes and seats
  - Save bookings to SQL Server LocalDB
- Persistent storage using Entity Framework Core and LocalDB
- Database configuration stored in `appsettings.json`

## Project Structure

- `MovieBookingWPF.sln` - solution file
- `MovieBookingWPF/` - main WPF application project
  - `App.xaml` / `App.xaml.cs` - application startup
  - `MainWindow.xaml` / `MainWindow.xaml.cs` - login screen
  - `Models/` - EF Core models and DbContext
  - `Migrations/` - database migration files
  - various window classes for admin/customer UI

## Requirements

- .NET 8 SDK
- Visual Studio 2022 or newer with WPF support
- SQL Server LocalDB (installed with Visual Studio by default)

## Database

The application uses LocalDB via connection string in `MovieBookingWPF/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MovieBookingsDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Run the app

1. Open `MovieBookingWPF.sln` in Visual Studio.
2. Build the solution.
3. Run the `MovieBookingWPF` project.

## Notes

- Passwords are currently stored and compared in plaintext.
- There is no registration screen; users must exist in the database before login.
- `BCrypt.Net-Next` is installed but not yet integrated for password hashing.
- The admin and customer UIs are built in code-behind rather than full XAML.

## Improvements

- Add user registration and password hashing
- Refactor UI into XAML + MVVM for maintainability
- Sync `MovieStore` in-memory state with database usage
- Ensure booking deletion updates the database

## Contributors

- Mekdes Merga
-Biruk Aemero
-Mihret Habtamu