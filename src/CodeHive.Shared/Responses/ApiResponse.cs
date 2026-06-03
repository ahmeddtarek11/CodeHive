using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Shared.Responses;

public sealed record ApiResponse<T>(string Message, T Data);


public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(string message, T data)
        => new(message, data);
}
