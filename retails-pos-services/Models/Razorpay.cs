using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Razorpay.Models
{
    [Table("RJUpiTransactions")]
    public class UpiTransaction
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "PROCESSING";
        public int StatusType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    [Table("Feedbacks")]
    public class Feedback
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
    public class RazorpayOrderRequest
    {
        public int Amount { get; set; }  // in paise
        public string Currency { get; set; } = "INR";
        public string Receipt { get; set; }
    }
    public class RazorpayOptions
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        //public string WebhookSecret { get; set; }
    }
    public class RazorpayVerificationRequest
    {
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }
    }
    public class Razorpay
    {
        public int Id { get; set; }

        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }

        public int Amount { get; set; } // In paise (e.g., ₹100 = 10000)
        public string Currency { get; set; } = "INR";

        public string Status { get; set; } = "Pending"; // Pending, Success, Failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}


