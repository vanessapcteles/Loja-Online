using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace LojaOnline.MockServices
{
    /// <summary>
    /// Configuração do WireMock para simular serviços externos
    /// </summary>
    public static class WireMockSetup
    {
        private static WireMockServer? _server;

        public static void Start()
        {
            if (_server != null && _server.IsStarted)
            {
                Console.WriteLine("[WireMock] Server already running");
                return;
            }

            _server = WireMockServer.Start(5050);
            Console.WriteLine($"[WireMock] Server started at {_server.Url}");

            ConfigurePaymentEndpoint();
            ConfigureEmailEndpoint();
        }

        private static void ConfigurePaymentEndpoint()
        {
            // Endpoint de pagamento - 80% sucesso
            _server!
                .Given(Request.Create()
                    .WithPath("/api/payment/process")
                    .UsingPost())
                .WithProbability(0.8)
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""success"": true,
                        ""transactionId"": """ + Guid.NewGuid() + @""",
                        ""message"": ""Payment processed successfully""
                    }")
                    .WithDelay(TimeSpan.FromMilliseconds(Random.Shared.Next(100, 500)))
                );

            // Endpoint de pagamento - 20% falha
            _server!
                .Given(Request.Create()
                    .WithPath("/api/payment/process")
                    .UsingPost())
                .WithProbability(0.2)
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""success"": false,
                        ""message"": ""Payment gateway temporarily unavailable""
                    }")
                    .WithDelay(TimeSpan.FromMilliseconds(Random.Shared.Next(100, 500)))
                );

            Console.WriteLine("[WireMock] Payment endpoint configured: POST /api/payment/process (80% success)");
        }

        private static void ConfigureEmailEndpoint()
        {
            // Endpoint de email - sempre sucesso
            _server!
                .Given(Request.Create()
                    .WithPath("/api/email/send")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"{
                        ""success"": true,
                        ""messageId"": """ + Guid.NewGuid() + @""",
                        ""message"": ""Email sent successfully""
                    }")
                    .WithDelay(TimeSpan.FromMilliseconds(Random.Shared.Next(50, 200)))
                );

            Console.WriteLine("[WireMock] Email endpoint configured: POST /api/email/send (100% success)");
        }

        public static void Stop()
        {
            if (_server != null && _server.IsStarted)
            {
                _server.Stop();
                Console.WriteLine("[WireMock] Server stopped");
            }
        }

        public static string GetUrl()
        {
            return _server?.Url ?? "http://localhost:5050";
        }
    }
}
