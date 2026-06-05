using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace CodeHive.Infrastructure.Caching;

public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDistributedCache _cache = cache;
    private readonly IConnectionMultiplexer _redis = redis;

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class 
    {
        var bytes = await _cache.GetAsync(key ,ct );
        if(bytes is null ) return default ;

        return JsonSerializer.Deserialize<T>(bytes);

    }
    public async Task<T?> SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) where T : class
    {
        var bytes  = await _cache.GetAsync(key,ct );
        if(bytes is null ) return null ; 

        return JsonSerializer.Deserialize<T>(bytes);
    }


    public async Task RemoveAsync(string key, CancellationToken ct = default) => await _cache.RemoveAsync(key, ct);
    



    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{prefix}*").ToArray();
        if(keys.Length > 0)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(keys);
        }
    }

    
}
