using Polly;
using Polly.Retry;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace LojaOnline.Services
{
    /// <summary>
    /// Serviço de pagamento externo com Polly (Retry)
    /// </summary>
    public class ExternalPaymentService : IExternalPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalPaymentService> _logger;
        private readonly ResiliencePipeline _retryPipeline;

        public ExternalPaymentService(HttpClient httpClient, ILogger<ExternalPaymentService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configurar Polly Retry Pipeline
            _retryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = args =>
                    {
                        _logger.LogWarning("[Polly Retry] Attempt {AttemptNumber} after {Delay}ms delay", 
                            args.AttemptNumber, args.RetryDelay.TotalMilliseconds);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        public async Task<bool> ProcessPaymentAsync(decimal amount, string orderId)
        {
            try
            {
                _logger.LogInformation("[Payment] Processing payment for Order {OrderId}, Amount: {Amount}", 
                    orderId, amount);

                return await _retryPipeline.ExecuteAsync(async cancellationToken =>
                {
                    var payload = new
                    {
                        orderId = orderId,
                        amount = amount,
                        timestamp = DateTime.UtcNow
                    };

                    var content = new StringContent(
                        JsonSerializer.Serialize(payload),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await _httpClient.PostAsync("/api/payment/process", content, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("[Payment SUCCESS] Order {OrderId} paid successfully", orderId);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("[Payment FAILED] Order {OrderId}, Status: {Status}", 
                            orderId, response.StatusCode);
                        
                        // Lançar exceção para acionar retry
                        throw new HttpRequestException($"Payment failed with status {response.StatusCode}");
                    }
                }, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Payment ERROR] Failed to process payment for Order {OrderId} after retries", orderId);
                return false;
            }
        }
    }
}
