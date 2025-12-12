namespace LojaOnline.Services
{
    /// <summary>
    /// Interface para serviço de pagamento externo
    /// </summary>
    public interface IExternalPaymentService
    {
        /// <summary>
        /// Processa um pagamento
        /// </summary>
        /// <param name="amount">Valor a pagar</param>
        /// <param name="orderId">ID da encomenda</param>
        /// <returns>True se o pagamento foi bem sucedido</returns>
        Task<bool> ProcessPaymentAsync(decimal amount, string orderId);
    }
}
