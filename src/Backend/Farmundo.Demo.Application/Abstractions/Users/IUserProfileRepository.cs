using Farmundo.Demo.Domain.Models;

namespace Farmundo.Demo.Application.Abstractions.Users;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetBySubjectAsync(string subject, CancellationToken ct);
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<UserProfile> UpsertAsync(UserProfile profile, CancellationToken ct);

    Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct);
    Task<(bool Success, bool? NewValue)> ToggleActiveAsync(Guid id, CancellationToken ct);

}
