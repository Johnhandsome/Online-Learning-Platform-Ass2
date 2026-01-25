using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Learning_Platform_Ass1.Service.DTOs.Payment;
using Online_Learning_Platform_Ass1.Service.Services.Interfaces;

namespace Online_Learning_Platform_Ass1.Web.Controllers;

[Authorize]
public class PaymentController(
    IVnPayService vnPayService,
    IOrderService orderService) : Controller
{
    public async Task<IActionResult> CreatePaymentUrl(Guid orderId)
    {
        var order = await orderService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound("Order not found");
        }
        
        // Ensure order is not already paid
        if (order.Status == "completed")
        {
            return RedirectToAction("Success", "Course", new { id = orderId });
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
        
        // This generates the full URL to redirect to VNPay
        var paymentUrl = vnPayService.CreatePaymentUrl("127.0.0.1", model);
        
        return Redirect(paymentUrl);
    }

    [AllowAnonymous]
    public async Task<IActionResult> PaymentCallback()
    {
        // Log raw query string from VNPay for debugging
        Console.WriteLine("------------------------------------------");
        Console.WriteLine($"VNPAY CALLBACK RAW QUERY STRING: {Request.QueryString.Value}");
        Console.WriteLine($"ALL QUERY PARAMETERS:");
        foreach (var param in Request.Query)
        {
            Console.WriteLine($"  {param.Key} = {param.Value}");
        }
        Console.WriteLine("------------------------------------------");
        
        var queryDictionary = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
        var response = vnPayService.PaymentExecute(queryDictionary);

        if (response.Success && response.VnPayResponseCode == "00")
        {
            // Payment success. Update order status.
            if (Guid.TryParse(response.OrderId, out var orderId))
            {
                 // Pass VNPay transaction ID for idempotency
                 var success = await orderService.ProcessPaymentAsync(orderId, response.TransactionId);
                 if (success)
                 {
                     return RedirectToAction("Success", "Course", new { id = orderId });
                 }
                 else
                 {
                     TempData["ErrorMessage"] = "Payment was successful but enrollment failed. Please contact support.";
                     return RedirectToAction("Index", "Home");
                 }
            }
        }

        // Failure
        TempData["ErrorMessage"] = $"Payment failed or cancelled. Error code: {response.VnPayResponseCode}";
        return RedirectToAction("Index", "Home");
    }
}
