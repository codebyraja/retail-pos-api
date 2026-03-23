using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Razorpay.Models;

public interface IPaymentService
{
    Task<string> CreateOrderAsync(RazorpayOrderRequest request);
    Task<bool> MarkPaymentSuccessAsync(string orderId, string paymentId, string signature);
}


