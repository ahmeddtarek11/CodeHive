using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Shared;


namespace CodeHive.Users.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NotFound  = Error.NotFound("User");
    public static readonly Error EmailTaken        = new("User.EmailTaken","This email is already registered.");
    public static readonly Error UsernameTaken     = new("User.UsernameTaken","This username is already taken.");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials","Email or password is incorrect.");
    public static readonly Error AlreadyFollowing  = new("User.AlreadyFollowing","You already follow this user.");
    public static readonly Error CannotFollowSelf  = new("User.CannotFollowSelf", "You cannot follow yourself.");
    public static readonly Error TokenExpired      = new("User.TokenExpired", "Refresh token has expired.");
    public static readonly Error TokenInvalid      = new("User.TokenInvalid","Refresh token is invalid.");
}
