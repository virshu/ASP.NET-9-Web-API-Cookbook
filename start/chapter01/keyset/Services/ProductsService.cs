using cookbook.Data;
using cookbook.Models;
using cookbook.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class ProductReadService(AppDbContext context, IMemoryCache cache) : IProductsService
{
    private const string TotalPagesKey = "TotalPages";
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
        int totalPages = await GetTotalPagesAsync(pageSize);
        List<Product> products;
        bool hasNextPage;
        bool hasPreviousPage;

        if (!lastProductId.HasValue)
        {
            products = [];
            for (int i = 1; i <= pageSize; i++)
            {
                Product? product = await context.Products.FindAsync(i);
                if (product != null)
                {
                    products.Add(product);
                }
            }
            hasNextPage = products.Count == pageSize;
            hasPreviousPage = false;
        } else if (lastProductId == (totalPages - 1) * pageSize)
        {
            products = [];
            for (int i = lastProductId.Value; i < lastProductId.Value + pageSize; i++)
            {
                Product? product = await context.Products.FindAsync(lastProductId.Value - pageSize + i);
                if (product != null)
                {
                    products.Add(product);
                }
            }
            hasNextPage = false;
            hasPreviousPage = true;
        } else {
            context.ChangeTracker.Clear();

            products = await context.Products.Where(p => p.Id > lastProductId.Value)
                .OrderBy(p => p.Id)
                .Take(pageSize)
                .ToListAsync();
            int? lastId = products.LastOrDefault()?.Id;
            hasNextPage = lastId.HasValue && await context.Products.AnyAsync(p => p.Id > lastId);
            hasPreviousPage = true;
        }

        return new()
        {
            Items = products.Select(p => new ProductDTO()
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            }).ToList(),
            PageSize = pageSize,
            HasPreviousPage = hasPreviousPage,
            HasNextPage = hasNextPage,
            TotalPages = totalPages
        };
    }

    private async Task<int> GetTotalPagesAsync(int pageSize)
    {
        if (cache.TryGetValue(TotalPagesKey, out int totalPages)) return totalPages;

        context.ChangeTracker.Clear();
        int totalCount = await context.Products.CountAsync();
        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        cache.Set(TotalPagesKey, totalPages, TimeSpan.FromMinutes(2));
        return totalPages;
    }

    public void InvalidateCache()
    {
        cache.Remove(TotalPagesKey);
    }
}
