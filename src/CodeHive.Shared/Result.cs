using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Shared;

public class Result
{
    protected Result(bool isSuccess , Error error)
    {
        IsSuccess = isSuccess ; 
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure  => !IsSuccess;
    public Error Error { get; }
    public static Result Ok() => new (true , Error.None);
    public static  Result Fail(Error e ) => new (false , e);
    public static Result<T> Ok<T>(T value) => new(value, true, Error.None);
    public static Result<T> Fail<T>(Error e)=> new (default , false, e);
}


public sealed class Result<T> : Result
{
    private readonly T? _value;
    internal Result(T? value , bool isSuccess , Error error) : base(isSuccess, error) => _value = value;
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot read Value of a failed result.");


    // implict operators 

    public static implicit operator Result<T> (T value) => Ok<T>(value);
    public static implicit operator Result<T> (Error e ) => Fail<T>(e);
}