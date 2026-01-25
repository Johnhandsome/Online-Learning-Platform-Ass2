using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Learning_Platform_Ass1.Service.DTOs.Payment;
using Online_Learning_Platform_Ass1.Service.DTOs.Order;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;
using System.Security.Claims;

namespace Online_Learning_Platform_Ass1.Web.Controllers;

[Authorize]
public class OrderController(IOrderService orderService, IVnPayService vnPayService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToAction("Login", "User");
        }

        var orders = await orderService.GetUserOrdersAsync(userId);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmPurchase(Guid? courseId, Guid? pathId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var dto = new CreateOrderDto
        {
            CourseId = courseId,
            PathId = pathId
        };

        var preview = await orderService.GetOrderPreviewAsync(userId, dto);
        if (preview == null)
        {
            TempData["ErrorMessage"] = "Unable to load purchase information";
            return RedirectToAction("Index", "Home");
        }

        return View(preview);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(Guid? courseId, Guid? pathId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var dto = new CreateOrderDto
        {
            CourseId = courseId,
            PathId = pathId
        };

        var order = await orderService.CreateOrderAsync(userId, dto);
        if (order == null)
        {
            TempData["ErrorMessage"] = "Unable to create order. You may already own this item or all courses in the path.";
            return RedirectToAction("Index", "Home");
        }

        // Create VnPayRequestModel and redirect to VNPay
        var model = new VnPayRequestModel
        {
            OrderId = order.OrderId,
            Amount = order.Amount,
            CreatedDate = DateTime.Now,
            Description = $"Payment_for_order_{order.OrderId}",
            FullName = (User.Identity?.Name ?? "Guest").Replace(" ", "_")
        };

        // Generate VNPay payment URL
        var paymentUrl = vnPayService.CreatePaymentUrl("127.0.0.1", model);
        return Redirect(paymentUrl);
    }

    [HttpPost]
    public async Task<IActionResult> ContinuePayment(Guid orderId)
    {
        var order = await orderService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            TempData["ErrorMessage"] = "Order not found";
            return RedirectToAction(nameof(MyOrders));
        }

        // Check if order has expired
        var orderEntity = await orderService.GetOrderEntityByIdAsync(orderId);
        if (orderEntity != null && orderEntity.ExpiresAt.HasValue && orderEntity.ExpiresAt.Value < DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "This order has expired. Please create a new order.";
            return RedirectToAction(nameof(MyOrders));
        }

        // Create VnPayRequestModel
        var model = new VnPayRequestModel
        {
            OrderId = order.OrderId,
            Amount = order.Amount,
            CreatedDate = DateTime.Now,
            Description = $"Payment_for_order_{order.OrderId}",
            FullName = (User.Identity?.Name ?? "Guest").Replace(" ", "_")
        };

        // Generate VNPay payment URL
        var paymentUrl = vnPayService.CreatePaymentUrl("127.0.0.1", model);
        return Redirect(paymentUrl);
    }
}
