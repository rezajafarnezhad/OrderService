using BasketService.Infrastructure;
using Mapster;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entity;

namespace OrderService.Services;

public interface IOrderService
{
    Task CreateOrder(OrderModel model);
    Task<List<GetAllOrderModel>> GetAll();
    Task<GetAllOrderModel> GetOrderBy(Guid orderId);
}

public class OrderService : IOrderService
{
    private readonly OrderDatebaseContext _context;
    public OrderService(OrderDatebaseContext context)
    {
        _context = context;
    }
    public async Task CreateOrder(OrderModel model)
    {
        var order = new Order(model.UserId, DateTime.Now, false, model.OrderItems.Adapt(new List<OrderItem>()));
        _context.Order.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task<List<GetAllOrderModel>> GetAll()
    {
        return await _context.Order.AsNoTracking().Select(c => new GetAllOrderModel()
        {
            UserId = c.UserId,
            DateTime = c.DateTime,
            Id = c.Id,
            OrderPaid = c.OrderPaid,
            OrderItems = c.OrderItems.Adapt(new List<OrderItemModel>()),

        }).ToListAsync();
    }

    public async Task<GetAllOrderModel> GetOrderBy(Guid orderId)
    {
        var data = await _context.Order.AsNoTracking()
            .Select(c => new GetAllOrderModel()
            {
                UserId = c.UserId,
                DateTime = c.DateTime,
                Id = c.Id,
                OrderPaid = c.OrderPaid,
                OrderItems = c.OrderItems.Adapt(new List<OrderItemModel>()),
            }).FirstOrDefaultAsync();
        if (data is null)
            throw new Exception("Order not found ..,");

        return data;

    }
}