using cookbook.Data;
using cookbook.Models;
using cookbook.Services;
using Microsoft.EntityFrameworkCore;

public class ProductReadService(AppDbContext context) : IProductsService
{
    public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync() => await context.Products
            .AsNoTracking()
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            }).ToListAsync();

    public async Task<PagedProductResponseDTO> GetPagedProductsAsync(int pageSize, int? lastProductId = null)
    {
        IQueryable<Product> query = context.Products.AsQueryable();
        if (lastProductId.HasValue)
        {
            query = query.Where(p => p.Id > lastProductId.Value);
        }

        List<ProductDTO> products = await query
            .OrderBy(p => p.Id)
            .Take(pageSize)
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            }).ToListAsync();
        int? lastId = products.LastOrDefault()?.Id;
        bool hasNextPage = await context.Products.AnyAsync(p => p.Id > lastId);

        return new()
        {
            Items = products.Count > 0 ? products : Array.Empty<ProductDTO>(),
            PageSize = pageSize,
            HasPreviousPage = lastProductId.HasValue,
            HasNextPage = hasNextPage
        };
    }
}
