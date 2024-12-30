using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderService.Domain.Entity;
using OrderService.Infrastructure;
using OrderService.MessagingBus;
using OrderService.MessagingBus.Models;
using RabbitMQ.Client.Events;
using System.Text;

namespace OrderService.Services;

public interface IOrderService
{

    Task<List<GetAllOrderModel>> GetAll(string userId);
    Task<GetAllOrderDetailModel> GetOrderBy(Guid orderId);
    Task ReceivedOrderCreatedMessageJob(CancellationToken cancellationToken);
    Task<bool> RegisterOrder(BasketModelMessage modelMessage);
}

public class OrderService : IOrderService
{
    private readonly OrderDatebaseContext _context;
    private readonly IRabbitMqMessageBusHelper _messageBusHelper;
    private readonly RabbitMqConfiguration _rabbitMqConfiguration;
    private readonly IProductService _productService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public OrderService(OrderDatebaseContext context, IRabbitMqMessageBusHelper messageBusHelper, IOptions<RabbitMqConfiguration> rabbitMqConfiguration, IProductService productService, IServiceScopeFactory serviceScopeFactory)
    {
        _context = context;
        _messageBusHelper = messageBusHelper;
        _productService = productService;
        _serviceScopeFactory = serviceScopeFactory;
        _rabbitMqConfiguration = rabbitMqConfiguration.Value;
    }


    public async Task<List<GetAllOrderModel>> GetAll(string userId)
    {
        return await _context.Order
            .Include(c => c.OrderItems)
            .ThenInclude(c => c.Product)
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => new GetAllOrderModel()
            {
                Id = c.Id,
                DateTime = c.DateTime,
                OrderPaid = c.OrderPaid,
                OrderItemsCount = c.OrderItems.Count(),
                TotalPrice = c.TotalPrice,

            }).ToListAsync();
    }

    public async Task<GetAllOrderDetailModel> GetOrderBy(Guid orderId)
    {
        var data = await _context.Order
            .Include(c => c.OrderItems)
            .ThenInclude(c => c.Product)
            .AsNoTracking()
            .Where(c => c.Id == orderId)
            .Select(c => new GetAllOrderDetailModel()
            {
                Id = c.Id,
                DateTime = c.DateTime,
                OrderPaid = c.OrderPaid,
                OrderItemsCount = c.OrderItems.Count(),
                TotalPrice = c.TotalPrice,
                UserInfo = new UserInfo()
                {
                    UserId = c.UserId,
                    PostalCode = c.PostalCode,
                    Address = c.Address,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    PhoneNumber = c.PhoneNumber,
                },
                OrderItems = c.OrderItems.Select(c => new OrderItemModel
                {
                    OrderId = c.OrderId,
                    Quantity = c.Quantity,
                    ProductId = c.ProductId,
                    Price = c.Product.ProductPrice,
                    ProductName = c.Product.ProductName

                }).ToList()
            }).SingleOrDefaultAsync();

        if (data is null)
            throw new Exception("Order not found ..,");

        return data;

    }

    public async Task ReceivedOrderCreatedMessageJob(CancellationToken cancellationToken)
    {
        var connection = await _messageBusHelper.CheckCreateRabbitMqConnection(_rabbitMqConfiguration.HostName, _rabbitMqConfiguration.UserName, _rabbitMqConfiguration.Password);
        var channel = connection.CreateModel();
        channel.QueueDeclare(queue: _rabbitMqConfiguration.QueueName, durable: true,
           exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (sender, eventArg) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(eventArg.Body.ToArray());
                var message = JsonConvert.DeserializeObject<BasketModelMessage>(body);
                using var scope = _serviceScopeFactory.CreateScope(); // ایجاد محدوده جدید
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var result = await orderService.RegisterOrder(message);
                if (result)
                {
                    channel.BasicAck(deliveryTag: eventArg.DeliveryTag, multiple: false);
                }
                else
                {
                    channel.BasicNack(eventArg.DeliveryTag, false, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        };

        channel.BasicConsume(
           queue: _rabbitMqConfiguration.QueueName,
           autoAck: false,
           consumerTag: string.Empty,
           noLocal: false,
           exclusive: false,
           arguments: null,
           consumer: consumer
       );

    }
    public async Task<bool> RegisterOrder(BasketModelMessage modelMessage)
    {
        foreach (var c in modelMessage.BasketItemMessage)
        {
            var product = await _productService.GetOrCreateProduct(new ProductModel()
            {
                ProductId = c.ProductId,
                Price = c.Price,
                ProductName = c.ProductName
            });
        }
        var orderLineModel = modelMessage.BasketItemMessage.Adapt(new List<OrderItem>());
        var order = new Order(modelMessage.UserId, DateTime.Now, false, modelMessage.PhoneNumber,
            modelMessage.Address, modelMessage.FirstName, modelMessage.LastName,
            modelMessage.PostalCode, orderLineModel, modelMessage.TotalPrice);

        _context.Order.Add(order);
        await _context.SaveChangesAsync();
        return true;
    }
}