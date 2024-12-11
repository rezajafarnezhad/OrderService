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

    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder([FromForm] OrderModel model)
    {
        var result = _orderService.CreateOrder(model);
        return NoContent();
    }

    [HttpGet("GetOrders")]
    public async Task<IActionResult> GetOrders()
    {
        var result = await _orderService.GetAll();
        return Ok(result);
    }
    [HttpGet("GetOrder/{orderId}")]
    public async Task<IActionResult> GetOrders(Guid orderId)
    {
        var result = await _orderService.GetOrderBy(orderId);
        return Ok(result);
    }
}