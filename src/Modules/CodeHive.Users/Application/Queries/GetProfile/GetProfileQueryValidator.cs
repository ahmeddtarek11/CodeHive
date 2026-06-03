using FluentValidation;

namespace CodeHive.Users.Application.Queries.GetProfile;

public sealed class GetProfileQueryValidator : AbstractValidator<GetProfileQuery>
{
    public GetProfileQueryValidator()
    {
        RuleFor(query => query.Username)
            .Must(username => !string.IsNullOrWhiteSpace(username))
            .MaximumLength(30)
            .Matches("^[A-Za-z0-9_.-]+$");
    }
}
