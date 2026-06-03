using CodeHive.Infrastructure.Data;
using CodeHive.Shared;
using CodeHive.Shared.Cqrs;
using CodeHive.Users.Domain.Data.Entities;
using CodeHive.Users.Domain.Errors;
using CodeHive.Users.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeHive.Users.Application.Queries.GetProfile;

public sealed class GetProfileQueryHandler : IQueryHandler<GetProfileQuery, UserProfileDto>
{
    private readonly CodeHiveDbContext _dbContext;
    private readonly ILogger<GetProfileQueryHandler> _logger;

    public GetProfileQueryHandler(
        CodeHiveDbContext dbContext,
        ILogger<GetProfileQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.Trim().ToLowerInvariant();

        _logger.LogInformation(
            "Profile query started for Username={Username}.",
            username);

        var dto = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.UserName == username)
            .Select(user => new UserProfileDto(
                user.Id,
                user.UserName!,
                user.DisplayName,
                user.Bio,
                user.AvatarUrl,
                _dbContext.Set<Follow>().Count(follow => follow.FolloweeId == user.Id),
                _dbContext.Set<Follow>().Count(follow => follow.FollowerId == user.Id),
                user.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (dto is null)
        {
            _logger.LogWarning(
                "Profile query failed because username was not found. Username={Username}.",
                username);

            return UserErrors.NotFound;
        }

        _logger.LogInformation(
            "Profile query succeeded for Username={Username}, UserId={UserId}.",
            username,
            dto.Id);

        return dto;
    }
}
