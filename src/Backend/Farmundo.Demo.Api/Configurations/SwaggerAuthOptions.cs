namespace Farmundo.Demo.Api.Configurations;

public class SwaggerSecurityOptions
{
    // If set, configures OpenIdConnect security using this discovery URL
    public string? OpenIdConnectUrl { get; set; }
    // Optional description
    public string? Description { get; set; }
    // If OpenIdConnectUrl is not set, configures Bearer scheme
    public string Scheme { get; set; } = "bearer";
    public string BearerFormat { get; set; } = "JWT";
    // Scopes for OIDC requirement (e.g., ["openid","email","profile"]) 
    public string[]? Scopes { get; set; }
}

public class SwaggerOAuthOptions
{
    public string? ClientId { get; set; }
    public string AppName { get; set; } = "Swagger UI";
    public bool UsePkce { get; set; } = true;
    public string ScopeSeparator { get; set; } = " ";
}
