using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrderService.Controllers;


[Authorize("ManagementOrder")]
public class OrderManagementController : Controller
{
    public IActionResult Index()
    {
        return Content("Order For Admin");
    }
}