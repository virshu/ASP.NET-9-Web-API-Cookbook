using countBy.Models;
using CountBy.Models;
using CountBy.Services;
using Microsoft.EntityFrameworkCore;

public class ProductReadService(AppDbContext context) : IProductReadService
{
    public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync() =>
        await context.Products
            .AsNoTracking()
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            }).ToListAsync();

    public async Task<ProductDTO?> GetAProductAsync(int id) =>
        await context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId
            }).FirstOrDefaultAsync();

    public IReadOnlyCollection<CategoryDTO> GetCategoryInfo()
    {
        IEnumerable<Product> products =  context.Products.AsNoTracking().AsEnumerable();
        IOrderedEnumerable<KeyValuePair<int, int>> productsByCategory =
            products.CountBy(p => p.CategoryId).OrderBy(x => x.Key);

        return productsByCategory.Select(categoryGroup => new CategoryDTO
            {
                CategoryId = categoryGroup.Key,
                ProductCount = categoryGroup.Value
            }).ToList();
    }
}
