using FluentValidation;
using System.Globalization;

namespace CodeHive.Users.Application.Queries.GetFollowers;

public sealed class GetFollowersQueryValidator : AbstractValidator<GetFollowersQuery>
{
    public GetFollowersQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.Limit).GreaterThan(0);
        RuleFor(query => query.Cursor)
            .Must(BeValidCursor)
            .When(query => !string.IsNullOrWhiteSpace(query.Cursor));
    }

    private static bool BeValidCursor(string? cursor)
        => DateTimeOffset.TryParse(
            cursor,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out _);
}
