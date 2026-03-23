using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Easebuzz.Models
{
    public class EasebuzzPaymentRequest
    {
        public string Amount { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Productinfo { get; set; } = string.Empty;
        public string TxnId { get; set; } = string.Empty;
    }

    public class EasebuzzPaymentResponse
    {
        public bool Success { get; set; }
        public string? PaymentLink { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class EasebuzzPaymentRequestV1
    {
        public string Amount { get; set; }       // should be string like "150.00"
        public string Purpose { get; set; }      // e.g., "Order123"
        public string BuyerName { get; set; }    // e.g., "Raja Singh"
        public string Email { get; set; }        // e.g., "raja@example.com"
        public string Phone { get; set; }        // e.g., "9876543210"
    }

    public class UpdateOrderStatusRequest
    {
        public string OrderId { get; set; }
        public string Status { get; set; }  // "Success" / "Failed"
        public string TxnId { get; set; }
    }

    //public class EasebuzzWebhookRequest
    //{
    //    // Transaction details
    //    public string? TxnId { get; set; }          // Merchant transaction id
    //    public string? EasepayId { get; set; }      // Easebuzz transaction id
    //    public string? Status { get; set; }         // success / failure / userCancelled
    //    public string? Amount { get; set; }         // Paid amount
    //    public string? Productinfo { get; set; }    // Product description
    //    public string? Udf1 { get; set; }           // Optional user-defined fields
    //    public string? Udf2 { get; set; }
    //    public string? Udf3 { get; set; }
    //    public string? Udf4 { get; set; }
    //    public string? Udf5 { get; set; }

    //    // Customer details
    //    public string? Firstname { get; set; }
    //    public string? Email { get; set; }
    //    public string? Phone { get; set; }

    //    // Add missing fields 👇
    //    public string? PaymentStatus { get; set; }
    //    public string? GatewayTxnId { get; set; }
    //    public string? BankRefNo { get; set; }
    //    public string? Remarks { get; set; }

    //    // Bank/Payment details
    //    public string? Mode { get; set; }           // Payment mode (NetBanking, Card, UPI etc.)
    //    public string? BankRefNum { get; set; }     // Bank reference number
    //    public string? Error { get; set; }          // If failure, error message
    //    public string? PGType { get; set; }         // Gateway type

    //    // Security
    //    public string? Hash { get; set; }           // Hash from Easebuzz (for validation)

    //    // Extra (timestamps etc.)
    //    public string? AddedOn { get; set; }        // Transaction date-time
    //    public string? Mihpayid { get; set; }       // Legacy Easebuzz transaction id (some docs use this)
    //}
    public class PaymentStatusDto
    {
        public int VchCode { get; set; }
        public string PaymentStatus { get; set; }
        public string? PaymentMode { get; set; }
        public string Remarks { get; set; }
        public string GatewayTxnId { get; set; }
        public string BankRefNo { get; set; }

    }

    public class EasebuzzWebhookRequest
    {
        [FromForm(Name = "txnid")]
        public string? TxnId { get; set; }

        [FromForm(Name = "status")]
        public string? Status { get; set; }

        [FromForm(Name = "easepayid")]
        public string? EasepayId { get; set; }

        [FromForm(Name = "bank_ref_num")]
        public string? BankRefNum { get; set; }

        [FromForm(Name = "productinfo")]
        public string? Productinfo { get; set; }

        [FromForm(Name = "amount")]
        public string? Amount { get; set; }

        [FromForm(Name = "firstname")]
        public string? FirstName { get; set; }

        [FromForm(Name = "email")]
        public string? Email { get; set; }

        [FromForm(Name = "phone")]
        public string? Phone { get; set; }

        [FromForm(Name = "hash")]
        public string? Hash { get; set; }

        [FromForm(Name = "mode")]  // ✅ Payment Method (UPI, CC, NB, DC)
        public string? Mode { get; set; }
    }
}
