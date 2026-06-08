using System.Threading.Tasks;
using CodeHive.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CodeHive.Infrastructure.Messaging.Consumers.Users;

public sealed class UserRegisteredConsumer : IConsumer<IUserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
        => _logger = logger;

    public Task Consume(ConsumeContext<IUserRegisteredEvent> context)
    {
        var msg = context.Message;

        // TODO Phase 3: send actual welcome email via SendGrid/Resend
        _logger.LogInformation(
            "Welcome email (stub): sending to {Email} for user {Username}",
            msg.Email, msg.Username);

        return Task.CompletedTask;
    }
}
