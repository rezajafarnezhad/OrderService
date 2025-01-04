using OrderService.Services;

namespace OrderService.Jobs.RabbitReceivedMessage;

public class ReceivedOrderCreatedMessage : BackgroundService
{
    private readonly IServiceScopeFactory _orderService;

    public ReceivedOrderCreatedMessage(IServiceScopeFactory orderService)
    {
        _orderService = orderService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _orderService.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            await orderService.ReceivedOrderCreatedMessageJob(stoppingToken);
        }
    }
}


public class ReceivedPaymentDoneMessage : BackgroundService
{
    private readonly IServiceScopeFactory _orderService;

    public ReceivedPaymentDoneMessage(IServiceScopeFactory orderService)
    {
        _orderService = orderService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _orderService.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            await orderService.ReceivedPaymentDoneMessageJob(stoppingToken);
        }
    }
}


public class ReceivedUpdateProductMessage : BackgroundService
{
    private readonly IServiceScopeFactory _orderService;

    public ReceivedUpdateProductMessage(IServiceScopeFactory orderService)
    {
        _orderService = orderService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _orderService.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            await orderService.UpdateProductMessageHandel();
        }
    }
}