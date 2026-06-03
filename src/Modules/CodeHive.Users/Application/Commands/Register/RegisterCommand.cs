using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared.Cqrs;

namespace CodeHive.Users.Application.Commands.Register;

public record RegisterCommand(
    string Email,
    string Username,
    string Password,
    string DisplayName
) : ICommand<Guid>;
