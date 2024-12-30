using Mapster;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entity;
using OrderService.Infrastructure;

namespace OrderService.Services;

public interface IProductService
{
    Task<Product> GetOrCreateProduct(ProductModel model);
}

public class ProductService : IProductService
{
    private readonly OrderDatebaseContext _context;

    public ProductService(OrderDatebaseContext context)
    {
        _context = context;
    }

    public async Task<Product> GetOrCreateProduct(ProductModel model)
    {
        var existProduct = await _context.Product.AsNoTracking()
            .SingleOrDefaultAsync(c => c.ProductId == model.ProductId);

        if (existProduct is not null)
            return existProduct;

        var product = model.Adapt(new Product());
        product.ProductPrice = model.Price;
        _context.Product.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }
}