namespace Farmundo.Demo.Domain.Models;

public static class Roles
{
    public const string Farmer = "farmer";
    public const string Buyer = "buyer";
    public const string Admin = "admin";
    public static readonly string[] All = [Farmer, Buyer, Admin];
}
