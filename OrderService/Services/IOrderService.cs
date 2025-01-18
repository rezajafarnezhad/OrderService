using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderService.Domain.Entity;
using OrderService.Infrastructure;
using OrderService.MessagingBus;
using OrderService.MessagingBus.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace OrderService.Services;

public interface IOrderService
{

    Task<List<GetAllOrderModel>> GetAll(string userId);
    Task<GetAllOrderDetailModel> GetOrderBy(Guid orderId);
    Task ReceivedOrderCreatedMessageJob(CancellationToken cancellationToken);
    Task ReceivedPaymentDoneMessageJob(CancellationToken cancellationToken);
    Task<bool> RegisterOrder(BasketModelMessage modelMessage);
    Task<bool> OrderPayment(Guid orderId);
    Task<bool> PaidOrder(Guid orderId);

    Task UpdateProductMessageHandel();
    Task<bool> UpdateProduct(ProductUpdateMessage model);
}

public class OrderService : IOrderService
{
    private readonly OrderDatebaseContext _context;
    private readonly IRabbitMqMessageBusHelper _messageBusHelper;
    private readonly RabbitMqConfiguration _rabbitMqConfiguration;
    private readonly IProductService _productService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMessageBus _messageBus;
    public OrderService(OrderDatebaseContext context, IRabbitMqMessageBusHelper messageBusHelper, IOptions<RabbitMqConfiguration> rabbitMqConfiguration, IProductService productService, IServiceScopeFactory serviceScopeFactory, IMessageBus messageBus)
    {
        _context = context;
        _messageBusHelper = messageBusHelper;
        _productService = productService;
        _serviceScopeFactory = serviceScopeFactory;
        _messageBus = messageBus;
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
                OrderStatus = c.OrderStatus

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
                OrderStatus = c.OrderStatus,
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
                using var scope = _serviceScopeFactory.CreateScope();
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

    public async Task ReceivedPaymentDoneMessageJob(CancellationToken cancellationToken)
    {
        var connection = await _messageBusHelper.CheckCreateRabbitMqConnection(_rabbitMqConfiguration.HostName, _rabbitMqConfiguration.UserName, _rabbitMqConfiguration.Password);
        var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "PaymentDone", durable: true,
            exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (sender, args) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonConvert.DeserializeObject<PaymentDoneMessage>(body);
                using var scope = _serviceScopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var result = await orderService.PaidOrder(message.OrderId);
                if (result)
                {
                    channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
                }
                else
                {
                    channel.BasicNack(args.DeliveryTag, false, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }

        };
        channel.BasicConsume(
            queue: "PaymentDone",
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

    public async Task<bool> OrderPayment(Guid orderId)
    {
        var order = await _context.Order.FindAsync(orderId);
        if (order is null)
            return false;

        var message = new PaymentOrderMessage()
        {
            OrderId = order.Id,
            Amount = order.TotalPrice
        };

        await _messageBus.SendMessage(message, "OrderPayment");
        order.ChangeStatus(OrderStatus.RequestedPayment, false);
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<bool> PaidOrder(Guid orderId)
    {
        var order = await _context.Order.FindAsync(orderId);
        order.ChangeStatus(OrderStatus.IsPaid, true);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task UpdateProductMessageHandel()
    {
        var connection = await _messageBusHelper.CheckCreateRabbitMqConnection(_rabbitMqConfiguration.HostName,
            _rabbitMqConfiguration.UserName, _rabbitMqConfiguration.Password);

        var channel = connection.CreateModel();
        channel.ExchangeDeclare("ProductUpdated", ExchangeType.Topic, true, false, null);
        channel.QueueDeclare("Order_ProductUpdate", true, false, false, null);
        channel.QueueBind("Order_ProductUpdate", "ProductUpdated", "Product.Updated");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (sender, args) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonConvert.DeserializeObject<ProductUpdateMessage>(body);
                using var scope = _serviceScopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var result = await orderService.UpdateProduct(message);
                if (result)
                {
                    channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
                }
                else
                {
                    channel.BasicNack(args.DeliveryTag, false, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }

        };
        channel.BasicConsume(
            queue: "Order_ProductUpdate",
            autoAck: false,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer
        );
    }

    public async Task<bool> UpdateProduct(ProductUpdateMessage model)
    {
        var product = await _context.Product.FindAsync(model.ProductId);
        if (product == null)
            return false;
        product.EditProductName(model.ProductName);
        await _context.SaveChangesAsync();
        return true;
    }
}