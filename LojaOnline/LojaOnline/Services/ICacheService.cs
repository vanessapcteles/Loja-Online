namespace LojaOnline.Services
{
    /// <summary>
    /// Interface para serviço de cache distribuído
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Obtém um valor do cache
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Guarda um valor no cache
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Remove um valor do cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Verifica se uma chave existe no cache
        /// </summary>
        Task<bool> ExistsAsync(string key);
    }
}
