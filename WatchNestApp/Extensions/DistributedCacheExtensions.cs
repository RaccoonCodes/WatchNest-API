using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WatchNestApp.Extensions
{
    public static class DistributedCacheExtensions
    {
        //Attempts to retrieve cache and deserialize it 
        public static bool TryGetValue<T>(this IDistributedCache cache, string key, out T? value)
        {
            value = default;
            var val = cache.Get(key);


            if (val == null)
            {
                return false;
            }

            value = JsonSerializer.Deserialize<T>(val);

            return true;
        }

        //Store value in cache after serializing into a JSON
        public static void Set<T>(this IDistributedCache cache, string key, T value, TimeSpan timeSpan)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
            cache.Set(key, bytes, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = timeSpan
            });
        }

        //Generates cache keys
        public static string GenerateCacheKey(this object input)
        {
            var serializedInput = JsonSerializer.Serialize(input);
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(serializedInput));
            return Convert.ToBase64String(hashBytes);

        }
    }
}
