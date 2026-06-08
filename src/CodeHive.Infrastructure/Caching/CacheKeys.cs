using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Infrastructure.Caching;

public static class CacheKeys
{

    //user profile -- varies by username

    public static string UserProfile(string username) => $"profile:{username}";

    public static string feed(Guid userId , string? cursor) =>  $"feed:{userId}:{cursor ?? "first"}";

    public static  string FeedPrefix(Guid userId) => $"feed:{userId}:" ;  // for bulk deletion

     // Trending posts — shared across all users, one key
     public static string Trending() => "trending";
    
}
