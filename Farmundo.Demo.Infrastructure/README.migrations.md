EF Core Migrations (PostgreSQL)

1) Ensure Postgres is running (Docker example):
   docker run --name pg -e POSTGRES_PASSWORD=postgres -e POSTGRES_USER=postgres -e POSTGRES_DB=farmundo_demo -p 5432:5432 -d postgres:16

2) Set connection string environment variable for design-time factory (optional if you use the default above):
   Windows PowerShell:
     $env:POSTGRES_CONNECTION_STRING = "Host=localhost;Port=5432;Database=farmundo_demo;Username=postgres;Password=postgres"
   macOS/Linux:
     export POSTGRES_CONNECTION_STRING="Host=localhost;Port=5432;Database=farmundo_demo;Username=postgres;Password=postgres"

3) Add migration (from solution root):
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate -p Farmundo.Demo.Infrastructure -s Farmundo.Demo.Api

4) Apply migration to database:
   dotnet ef database update -p Farmundo.Demo.Infrastructure -s Farmundo.Demo.Api

5) If you change the model (e.g., ChatMessage), add a new migration:
   dotnet ef migrations add <DescriptiveName> -p Farmundo.Demo.Infrastructure -s Farmundo.Demo.Api
   dotnet ef database update -p Farmundo.Demo.Infrastructure -s Farmundo.Demo.Api

Notes
- The design-time factory reads POSTGRES_CONNECTION_STRING; otherwise uses a sensible default.
- The API also reads appsettings ConnectionStrings:Postgres.
- Ensure Npgsql.EnableLegacyTimestampBehavior is set if needed (already in Program.cs).
