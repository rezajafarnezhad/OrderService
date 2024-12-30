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