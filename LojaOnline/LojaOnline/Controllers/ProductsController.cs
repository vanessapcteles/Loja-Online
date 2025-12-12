using LojaOnline.Data;
using LojaOnline.Models;
using LojaOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaOnline.Controllers
{
  
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        // --- INJEÇÃO DE DEPENDÊNCIA ---

        // Remove a lista 'static' e o contador

        // Declara uma variável privada para o DbContext
        private readonly ApiDbContext _context;
        private readonly ICacheService _cache;
        private const string PRODUCTS_CACHE_KEY = "all_products";

        // Pede o DbContext e CacheService no construtor (Injeção de Dependência)
        public ProductsController(ApiDbContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        // --- FIM DA INJEÇÃO ---


        // GET ALL (Com Cache Redis)
        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts()
        {
            // Tentar obter do cache primeiro
            var cachedProducts = await _cache.GetAsync<List<Product>>(PRODUCTS_CACHE_KEY);
            if (cachedProducts != null)
            {
                return Ok(cachedProducts);
            }

            // Se não estiver em cache, buscar da BD
            var products = await _context.Products.Select(x =>new Product
            {
                Id = x.Id,
                Name = x.Name,
                Sku = x.Sku,
                Description = x.Description,
                Price = x.Price,
                Category = x.Category,
                Gender = x.Gender,
                ImageUrl = x.ImageUrl,
                CreatedAt = x.CreatedAt

            }).ToListAsync();

            // Guardar em cache por 5 minutos
            await _cache.SetAsync(PRODUCTS_CACHE_KEY, products, TimeSpan.FromMinutes(5));

            return Ok(products);
        }

        // GET BY ID 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        

        // CREATE (Com invalidação de cache)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            _context.Products.Add(product); // Adiciona o produto ao DbSet
            await _context.SaveChangesAsync(); // Salva as mudanças na BD

            // Invalidar cache
            await _cache.RemoveAsync(PRODUCTS_CACHE_KEY);

            return Ok(product);
        }
       

        // UPDATE (Com invalidação de cache)
        [HttpPut("EditProduct")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditProduct([FromBody] Product product )
        {
           var rows = await _context.Products
                .Where(p => p.Id == product.Id)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(p => p.Name, product.Name)
                    .SetProperty(p => p.Sku, product.Sku)
                    .SetProperty(p => p.Description, product.Description)
                    .SetProperty(p => p.Price, product.Price)
                    .SetProperty(p => p.Category, product.Category)
                    .SetProperty(p => p.Gender, product.Gender)
                    .SetProperty(p => p.ImageUrl, product.ImageUrl)
                );

            // Invalidar cache
            await _cache.RemoveAsync(PRODUCTS_CACHE_KEY);

            return Ok(product);
        }

        // DELETE (Com invalidação de cache)
        [HttpDelete("DeleteProduct")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(long productId)
        {
            var rows = await _context.Products.Where(p => p.Id == productId).ExecuteDeleteAsync();

            // Invalidar cache
            await _cache.RemoveAsync(PRODUCTS_CACHE_KEY);

            return Ok(true);
        }
    }
}
