using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Razorpay.Models;
using RetailPosContext.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class PaymentService : IPaymentService
{
    private readonly RazorpayOptions _options;
    private readonly RetailPosDBContext _db;

    public PaymentService(IOptions<RazorpayOptions> options, RetailPosDBContext db)
    {
        _options = options.Value;
        _db = db;
    }
    public async Task<string> CreateOrderAsync(RazorpayOrderRequest request)
    {
        using var client = new HttpClient();
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.Key}:{_options.Secret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        var payload = new
        {
            amount = request.Amount,
            currency = request.Currency ?? "INR", // ✅ Default to "INR"
            receipt = request.Receipt ?? Guid.NewGuid().ToString(),
            payment_capture = 1
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.razorpay.com/v1/orders", content);
        
        var result = await response.Content.ReadAsStringAsync();

        var json = JsonConvert.DeserializeObject<dynamic>(result);
        string razorpayOrderId = json.id;

        // ✅ Save order info in DB using the new function
        await SaveOrderToDatabaseAsync(razorpayOrderId, request.Amount, request.Currency ?? "INR");

        return result;
    }

    public bool VerifySignature(string orderId, string paymentId, string signature)
    {
        string payload = $"{orderId}|{paymentId}";
        var secret = _options.Secret;

        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(); //ToLower().Replace("-", "");
        //return generatedSignature == signature;

        var expectedSignature = BitConverter.ToString(Convert.FromBase64String(signature)).Replace("-", "").ToLowerInvariant();
        return generatedSignature == expectedSignature;
    }

    public async Task<bool> MarkPaymentSuccessAsync(string orderId, string paymentId, string signature)
    {
        var isValid = VerifySignature(orderId, paymentId, signature);

        var payment = await _db.Payments
            .Where(p => p.RazorpayOrderId == orderId)
            .FirstOrDefaultAsync();

        if (payment == null) return false;

        if (isValid)
        {
            payment.RazorpayPaymentId = paymentId;
            payment.RazorpaySignature = signature;
            payment.Status = "Success";
            payment.PaidAt = DateTime.UtcNow;
        }
        else
        {
            payment.Status = "Failed";
        }

        await _db.SaveChangesAsync();
        return isValid;
    }

    public async Task<Razorpay.Models.Razorpay> SaveOrderToDatabaseAsync(string razorpayOrderId, int amount, string currency = "INR")
    {
        var payment = new Razorpay.Models.Razorpay
        {
            RazorpayOrderId = razorpayOrderId,
            Amount = amount,
            Currency = currency,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return payment;
    }
}

