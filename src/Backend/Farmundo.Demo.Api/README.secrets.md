User Secrets setup (local development)

We use .NET User Secrets for local configuration instead of environment variables.

1) Ensure Farmundo.Demo.Api has a UserSecretsId (already present).
2) Set secrets:
   dotnet user-secrets --project ./Farmundo.Demo.Api set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=farmundo_demo;Username=postgres;Password=postgres"
   dotnet user-secrets --project ./Farmundo.Demo.Api set "ConnectionStrings:Redis" "localhost:6379"
   dotnet user-secrets --project ./Farmundo.Demo.Api set "Auth:Provider" "AzureB2C"
   dotnet user-secrets --project ./Farmundo.Demo.Api set "AzureAdB2C:Authority" "https://<tenant>.b2clogin.com/<tenant>.onmicrosoft.com/<policy>/v2.0/"
   dotnet user-secrets --project ./Farmundo.Demo.Api set "AzureAdB2C:ClientId" "<client-id>"
   # or Cognito
   dotnet user-secrets --project ./Farmundo.Demo.Api set "Auth:Provider" "Cognito"
   dotnet user-secrets --project ./Farmundo.Demo.Api set "Cognito:Region" "<region>"
   dotnet user-secrets --project ./Farmundo.Demo.Api set "Cognito:UserPoolId" "<pool-id>"
   dotnet user-secrets --project ./Farmundo.Demo.Api set "Cognito:ClientId" "<client-id>"

Design-time EF Core
- The design-time AppDbContextFactory reads the same user secrets via the API's secrets id, so migrations and database update work without env vars.
- To add/apply migrations (from solution root):
  dotnet dotnet-ef migrations add <Name> -p Farmundo.Demo.Infrastructure -s Farmundo.Demo.Api
  dotnet dotnet-ef database update -p Farmundo.Demo.Infrastructure -s Farmundo.Demo.Api
