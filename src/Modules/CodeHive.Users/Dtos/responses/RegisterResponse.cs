using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Users.Dtos.responses;

public sealed record  RegisterResponse(Guid UserId , string message);
