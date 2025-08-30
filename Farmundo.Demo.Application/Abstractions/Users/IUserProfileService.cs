using Farmundo.Demo.Domain.Models;
using System.Security.Claims;

namespace Farmundo.Demo.Application.Abstractions.Users;

public interface IUserProfileService
{
    Task<UserProfile> UpsertFromClaimsAsync(ClaimsPrincipal user, string provider, CancellationToken ct = default);

    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default);
    Task<(bool Success, bool? NewValue)> ToggleActiveAsync(Guid id, CancellationToken ct = default);

}
