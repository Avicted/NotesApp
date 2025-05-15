# NotesApp

Authenticated users can perform CRUD operations on markdown notes. 
The application uses dotnet 9.0 and ASP.NET Core Identity for authentication and authorization, and it is built using the Clean Architecture principles.

The database is SQLite and Entity Framework Core is used for data access.

Unit tests are included to ensure the functionality of the application.

### Database Setup
```bash
# Update/Seed database from within the Web project
cd NotesApp.Web
dotnet ef database update --context ApplicationDbContext --project ../NotesApp.Infrastructure/NotesApp.Infrastructure.csproj --startup-project NotesApp.Web.csproj
```

### Database Migrations
```bash
# Create a migration
dotnet ef migrations add <MigrationName> --startup-project  NotesApp.Web/NotesApp.Web.csproj --project NotesApp.Infrastructure/NotesApp.Infrastructure.csproj
```

### Run the Application
```bash
chmod +x ./run.sh
./run.sh
```

### Testing
```bash
dotnet test
```

### License
MIT
