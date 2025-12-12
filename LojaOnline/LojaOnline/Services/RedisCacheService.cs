using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LojaOnline.Services
{
    /// <summary>
    /// Implementação do serviço de cache usando Redis
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("[Cache MISS] Key: {Key}", key);
                    return null;
                }

                _logger.LogInformation("[Cache HIT] Key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cache ERROR] Failed to get key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var serializedData = JsonSerializer.Serialize(value);
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
                };

                await _cache.SetStringAsync(key, serializedData, options);
                _logger.LogInformation("[Cache SET] Key: {Key}, Expiration: {Expiration}s", 
                    key, options.AbsoluteExpirationRelativeToNow?.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cache ERROR] Failed to set key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation("[Cache REMOVE] Key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cache ERROR] Failed to remove key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(key);
                return !string.IsNullOrEmpty(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cache ERROR] Failed to check key existence: {Key}", key);
                return false;
            }
        }
    }
}
