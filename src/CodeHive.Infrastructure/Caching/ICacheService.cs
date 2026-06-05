using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Infrastructure.Caching;

public interface ICacheService
{



     // Returns null if the key does not exist or has expired.
    Task<T?> GetAsync<T>(string key , CancellationToken ct =default) where T : class ;
    Task<T?> SetAsync<T> (string key , T value , TimeSpan ttl , CancellationToken ct = default) where T : class;
    Task RemoveAsync(string key , CancellationToken ct = default);

    //remove all keys that starts with a prefix e.g users:followersIds    
    Task RemoveByPrefixAsync(string prefix , CancellationToken ct = default);
    
}
