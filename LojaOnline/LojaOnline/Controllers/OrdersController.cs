using LojaOnline.Data;
using LojaOnline.Models;
using LojaOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace LojaOnline.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All order operations require login
    public class OrdersController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IExternalPaymentService _paymentService;

        public OrdersController(ApiDbContext context, IExternalPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        // GET: api/Orders/MyOrders
        [HttpGet("MyOrders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<CartItemDto> cartItems)
        {
            Console.WriteLine("[OrderDebug] Received CreateOrder request");
            
            if (cartItems == null || !cartItems.Any())
            {
                Console.WriteLine("[OrderDebug] Cart is empty or null");
                return BadRequest("Carrinho vazio.");
            }

            try 
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Console.WriteLine($"[OrderDebug] User Claim ID: {userIdClaim ?? "null"}");

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("User ID not found in token");
                }

                var userId = long.Parse(userIdClaim);
                Console.WriteLine($"[OrderDebug] Parsed User ID: {userId}");
                
                // Create the Order shell
                var order = new Order
                {
                    UserId = userId,
                    Status = "Pendente",
                    TotalAmount = 0 // Will verify backend side
                };

                decimal total = 0;

                // Fetch products and create items
                Console.WriteLine($"[OrderDebug] Processing {cartItems.Count} items");
                foreach (var item in cartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        Console.WriteLine($"[OrderDebug] Found product: {product.Name} - {product.Price}");
                        var orderItem = new OrderItem
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Price = product.Price, // Snapshot price
                            Quantity = item.Quantity,
                            Size = item.Size ?? "N/A"
                        };
                        order.Items.Add(orderItem);
                        total += orderItem.Price * orderItem.Quantity;
                    }
                    else 
                    {
                        Console.WriteLine($"[OrderDebug] Product not found for ID: {item.ProductId}");
                    }
                }

                order.TotalAmount = total;
                Console.WriteLine($"[OrderDebug] Total Amount: {total}");

                if (order.TotalAmount == 0)
                {
                    return BadRequest("Não foi possível processar a encomenda (Total 0).");
                }

                // Save to DB
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"[OrderDebug] Order created with ID: {order.Id}");

                // Processar pagamento com Polly (Retry + Circuit Breaker)
                var paymentSuccess = await _paymentService.ProcessPaymentAsync(order.TotalAmount, order.Id.ToString());

                if (paymentSuccess)
                {
                    order.Status = "Pago";
                    Console.WriteLine($"[OrderDebug] Payment successful for Order {order.Id}");
                }
                else
                {
                    order.Status = "Pagamento Falhado";
                    Console.WriteLine($"[OrderDebug] Payment failed for Order {order.Id}");
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    Message = paymentSuccess ? "Encomenda criada e paga com sucesso!" : "Encomenda criada mas pagamento falhou.", 
                    OrderId = order.Id,
                    PaymentStatus = order.Status
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderDebug] ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }

    // DTO for incoming cart data
    public class CartItemDto
    {
        [JsonPropertyName("productId")]
        public long ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; } = string.Empty;
    }
}
