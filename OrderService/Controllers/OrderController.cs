using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("GetOrders/{userId}")]
    public async Task<IActionResult> GetOrders(string userId)
    {
        var result = await _orderService.GetAll("111");
        return Ok(result);
    }
    [HttpGet("GetOrder/{orderId}")]
    public async Task<IActionResult> GetOrders(Guid orderId)
    {
        var result = await _orderService.GetOrderBy(orderId);
        return Ok(result);
    }
}