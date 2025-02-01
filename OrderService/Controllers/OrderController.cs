using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize("GetOrder")]
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
        var userId2 = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        var result = await _orderService.GetAll("111");
        return Ok(result);
    }
    [HttpGet("GetOrder/{orderId}")]
    public async Task<IActionResult> GetOrders(Guid orderId)
    {
        var result = await _orderService.GetOrderBy(orderId);
        return Ok(result);
    }

    [HttpGet("PaymentOrder/{orderId}")]
    public async Task<IActionResult> PaymentOrder(Guid orderId)
    {
        var result = await _orderService.OrderPayment(orderId);
        return Ok(result);
    }

}