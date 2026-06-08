using CodeHive.Infrastructure.Caching;
using CodeHive.Infrastructure.Data;
using CodeHive.Infrastructure.Identity;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Domain.Data.Entities;
using CodeHive.Users.Domain.Errors;
using CodeHive.Users.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FollowEntity = CodeHive.Users.Domain.Data.Entities.Follow;

namespace CodeHive.Users.Application.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CodeHiveDbContext _dbContext;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;
    private readonly ICacheService _cache;

    public UpdateProfileCommandHandler(
        UserManager<ApplicationUser> userManager,
        CodeHiveDbContext dbContext,
        ILogger<UpdateProfileCommandHandler> logger,
        ICacheService cache)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<UserProfileDto>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Profile update requested for UserId={UserId}.",
            request.RequestingUserId);

        var user = await _userManager.FindByIdAsync(request.RequestingUserId.ToString());
        if (user is null)
        {
            _logger.LogWarning(
                "Profile update rejected because user was not found. UserId={UserId}.",
                request.RequestingUserId);

            return UserErrors.NotFound;
        }

        if (user.Id != request.RequestingUserId)
        {
            _logger.LogWarning(
                "Profile update rejected because the authenticated user does not match the target user. RequestingUserId={RequestingUserId}, LoadedUserId={LoadedUserId}.",
                request.RequestingUserId,
                user.Id);

            return Error.Forbidden();
        }

        user.DisplayName = request.DisplayName.Trim();
        user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var firstError = updateResult.Errors.FirstOrDefault()?.Description
                ?? "Failed to update user profile.";

            _logger.LogWarning(
                "Profile update failed for UserId={UserId}. Error={Error}.",
                user.Id,
                firstError);

            return new Error("User.UpdateFailed", firstError);
        }

        var followersCount = await _dbContext.Set<FollowEntity>()
            .CountAsync(follow => follow.FolloweeId == user.Id, cancellationToken);

        var followingCount = await _dbContext.Set<FollowEntity>()
            .CountAsync(follow => follow.FollowerId == user.Id, cancellationToken);

        var dto = new UserProfileDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.DisplayName,
            user.Bio,
            user.AvatarUrl,
            followersCount,
            followingCount,
            user.CreatedAt);

         await _cache.RemoveAsync(CacheKeys.UserProfile(user.UserName!.ToLowerInvariant()), cancellationToken);

        _logger.LogInformation(
            "Profile updated successfully for UserId={UserId}.",
            user.Id);

        return dto;
    }
}
