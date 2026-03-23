using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Xml.Serialization;

namespace QSRAPIServices.Models
{
    public partial class Response
    {
        public int Status { get; set; }
        public string Msg { get; set; }
        public int Code { get; set; }
    }

    public partial class ResponseNew
    {
        public int Status { get; set; }
        public string Msg { get; set; }
        public int Code { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse Ok(string message = "Success", string code = "OK")
            => new ApiResponse
            {
                Success = true,
                Code = code,
                Message = message
            };

        public static ApiResponse Fail(string code, string message)
            => new ApiResponse
            {
                Success = false,
                Code = code,
                Message = message
            };
    }

    public class LoginResult
    {
        public int Status { get; set; }
        public string Msg { get; set; }
        public UserModel Data { get; set; }
    }

    public class SelectOptionDto
    {
        public int Value { get; set; }
        public string Label { get; set; }
    }

    public partial class UnknowList
    {
        public int Value { get; set; }
        public string Label { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserModel
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public bool IsOtpVerified { get; set; } = false;
    }

    public class EmailSettings
    {
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool EnableTls { get; set; }
        public string CompanyHead { get; set; }
    }
    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }
    public class SignupRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

    }

    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;

        [Required]
        public int UserId { get; set; }
    }
    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
    }
    public class EmailRequest
    {
        public string Email { get; set; }
    }
    public class ResetPasswordRequestWithOtp
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
    }
    public class RegisterCompany
    {
        public string? CompanyName { get; set; }
        public string? AdminUserName { get; set; }
        public string? AdminPassword { get; set; }
        public DateTime StartDate { get; set; } // For Financial Year
        public DateTime EndDate { get; set; }
    }
    public class SaveMasterRequest
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? PrintName { get; set; }
        public bool DeactiveMaster { get; set; } = false;
        public int MasterType { get; set; }
        public string? Users { get; set; }

        [NotMapped]
        public List<IFormFile>? Images { get; set; }
    }
    public class SaveProductMasterRequest
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public string? PrintName { get; set; }
        public int ParentGrp { get; set; }
        public string? Slug { get; set; }
        public string? Sku { get; set; }
        public int Unit { get; set; }
        public string? Description { get; set; }
        public int ProductType { get; set; } // 0: veg, 1: non-veg,
        public string? ProductTypeName { get; set; }
        public float Qty { get; set; } // For Product
        public float MinQty { get; set; } // For Product 
        public float Price { get; set; } // For Product 
        public float Discount { get; set; } // For Product
        public int TaxType { get; set; } // For Product
        public string? TaxTypeName { get; set; } // For Product
        public int DiscountType { get; set; } // 0: Flat, 1: Percentage
        public string? DiscountTypeName { get; set; } // 0: Flat, 1: Percentage
        public bool DeactiveMaster { get; set; } = false; // If true, deactivates the master
        public int MasterType { get; set; }
        public string? Users { get; set; }

        [NotMapped]
        public List<IFormFile>? Images { get; set; }
    }
    public class ImageUploadResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public List<string>? UploadedPaths { get; set; }
    }

    [Table("RJProductImages")]
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required, MaxLength(255)]
        public string? FileName { get; set; }

        [Required, MaxLength(500)]
        public string? FilePath { get; set; }

        [Required]
        public long Size { get; set; }

        [Required, MaxLength(100)]
        public string Type { get; set; }

        [Required]
        public int SrNo { get; set; } = 0; // For ordering images
    }
    public class GetProductMasterRequest
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public string? PrintName { get; set; }
        public int ParentGrp { get; set; }
        public string? ParentGrpName { get; set; }
        public string? Slug { get; set; }
        public string? SKU { get; set; }
        public string? Barcode { get; set; }
        public int Unit { get; set; }
        public string? UnitName { get; set; }
        public string? Description { get; set; }
        public int ProductType { get; set; } // 0: veg, 1: non-veg,
        public string? ProductTypeName { get; set; }
        public double Qty { get; set; } // For Product
        public double MinQty { get; set; } // For Product 
        public double Price { get; set; } // For Product 
        public double Discount { get; set; } // For Product
        public int TaxType { get; set; } // For Product
        public string? TaxTypeName { get; set; } // For Product
        public int DiscountType { get; set; } // 0: Flat, 1: Percentage
        public string? DiscountTypeName { get; set; } // 0: Flat, 1: Percentage
        public bool IsActive { get; set; }
        public int MasterType { get; set; }
        public string? Users { get; set; }
        public string? CreationTime { get; set; }

        [NotMapped]
        public List<ProductImageDto> ImageList { get; set; }
    }
    public class ProductImageDto
    {
        public int ProductId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public long Size { get; set; }
        public string? Type { get; set; }
        public int SrNo { get; set; } = 0;
    }
    public class GetMasterRequest
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? PrintName { get; set; }
        public bool IsActive { get; set; }
        public int MasterType { get; set; }
        public string? Users { get; set; }
        public string? CreationTime { get; set; }

        [NotMapped]
        public List<ProductImageDto> ImageList { get; set; }
    }
    public class Product
    {
        public List<string> ImagePaths { get; set; }
    }
    public class SaveUserMasterRequest
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Pwd { get; set; }
        public string? Remark { get; set; }
        public string? Base64 { get; set; }
        public int Role { get; set; } // 0: User, 1: Admin, 2: SuperAdmin
        public int UserType { get; set; }
        public bool Status { get; set; } = true;
        public string? Users { get; set; } // Comma-separated list of user IDs who can access this master
    }
    public class GetUserMasterDetailRequest
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Pwd { get; set; }
        public string? Remark { get; set; }
        public string? Image { get; set; }
        public int Role { get; set; } // 1: SuperAdmin, 2: Admin, 3: User
        public bool IsActive { get; set; } = true;
        public string? CreationOn { get; set; }
        public string? CreationTime { get; set; }
    }
    [XmlRoot("Products")]
    public class ImportProductRequest
    {
        [XmlElement("Product")]
        public List<ImportProduct> ImportProducts { get; set; }
    }
    public class ImportProduct
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? PrintName { get; set; }
        public int ParentGrp { get; set; }
        public string? Slug { get; set; }
        public string? Sku { get; set; }
        public int Unit { get; set; }
        public string? Description { get; set; }
        public int ProductType { get; set; }
        public string? ProductTypeName { get; set; }
        public float Qty { get; set; }
        public float MinQty { get; set; }
        public float Price { get; set; }
        public float Discount { get; set; }
        public int TaxType { get; set; }
        public string? TaxTypeName { get; set; }
        public int DiscountType { get; set; }
        public string? DiscountTypeName { get; set; }
        public bool IsActive { get; set; }
        public int MasterType { get; set; }
        public string? Users { get; set; }

        [XmlIgnore] // ✅ FIX: Exclude from XML
        [NotMapped] // ✅ For EF Core
        public List<IFormFile>? Images { get; set; }
    }
    [XmlRoot("Stocks")]
    public class ImportStockRequest
    {
        [XmlElement("Stock")]
        public List<ImportStock> ImportStocks { get; set; }
    }
    public class ImportStock
    {
        public string Name { get; set; }
        public float Qty { get; set; } // Quantity in stock
    }
    public class AppCategoryWithProduct
    {
        public int Code { get; set; } = 0;
        public string? Name { get; set; }
        public string? Image { get; set; }
        public List<Products> Products { get; set; }
    }
    public class Products
    {
        public int Code { get; set; } = 0;
        public string? Name { get; set; }
        public string? Image { get; set; }
        public double? Qty { get; set; }
        public double? MinQty { get; set; }
        public double? Price { get; set; }
        public double Stock { get; set; }
        public bool IsVeg { get; set; }
        public string? Description { get; set; }
    }

    public class Category
    {
        public int Code { get; set; } = 0;
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public List<Item> Items { get; set; } = new();
    }

    public class Item
    {
        public int Code { get; set; } = 0;
        public string? Name { get; set; }
        public int ParentGrp { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public double? Quantity { get; set; }
        public double? MinimumQuantity { get; set; }
        public double? Price { get; set; }
        public double? Stock { get; set; }
        public bool IsVegetarian { get; set; }
        public string? Description { get; set; }
        public string? Barcode { get; set; }
    }

    public class RawCategoryProduct
    {
        public int CategoryCode { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryImg { get; set; }
        public int ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImg { get; set; }
        public string? ProductDesc { get; set; }
        public double? Qty { get; set; }
        public double? Price { get; set; }
        public double? MinQty { get; set; }
        public int IsVeg { get; set; }
        public double? StockQty { get; set; }
        public string? Barcode { get; set; }
    }

    public class RawCategorie
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
    }

    public class RawProduct
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public int ParentGrp { get; set; }
        public string? Description { get; set; }
        public double? Qty { get; set; }
        public double? Price { get; set; }
        public double? MinQty { get; set; }
        public int IsVeg { get; set; }
        public double? Stock { get; set; }
        public string? Barcode { get; set; }
        public string? Image { get; set; }
    }

    public class SvCustomerDet
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int CountryCode { get; set; }
        public int StateCode { get; set; }
        public int CityCode { get; set; }
        public string? Address { get; set; }
        public bool Status { get; set; }
        public string? Image { get; set; }
        public string? CreatedBy { get; set; }
        public int MasterType { get; set; } = 2;
    }
    public class GetCustomerDet
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int CountryCode { get; set; }
        public string? CountryName { get; set; }
        public int StateCode { get; set; }
        public string? StateName { get; set; }
        public int CityCode { get; set; }
        public string? CityName { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string? Image { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
    }
    public class VchConfig
    {
        public int TranType { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public int? StartNo { get; set; }
        public int? Padding { get; set; }
        public int? PaddLength { get; set; }
        public string PaddChar { get; set; }
    }

    public class AutoVchNo
    {
        public int AutoNo { get; set; }
    }

    public class SearchProduct
    {
        public string Name { get; set; }
    }

    public class GetSearchProductDet
    {
        public int Code { get; set; } = 0;
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public float Qty { get; set; }
        public float Price { get; set; }
        public float Discount { get; set; }
        public float TaxPer { get; set; }
        public float TaxAmt { get; set; }
        public float UnitCast { get; set; }
        public string image { get; set; }
    }


    public class TProduct
    {
        public int VchCode { get; set; } = 0;
        public int VchType { get; set; } = 0;
        public string? Date { get; set; }
        public string? VchNo { get; set; }
        public string? RefNo { get; set; }
        public int AccCode { get; set; } = 0;
        public string? AccName { get; set; }

        public int MCCode { get; set; } = 0;
        public string? MCName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int STType { get; set; } = 0;
        public string? STName { get; set; }
        public double SubTot { get; set; }
        public double TaxAmt { get; set; }
        public double Discount { get; set; }
        public double Shipping { get; set; }
        public double TotAmt { get; set; }
        public string? Remarks { get; set; }
        public int TranType { get; set; } = 0;
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TxnId { get; set; }
        public string? PaymentMode { get; set; }
        public string? GatewayTxnId { get; set; }
        public string? BankRefNo { get; set; }
        public string? PaymentGateway { get; set; }
        public string? PaymentRemark { get; set; }
        public double PayAmt { get; set; }
        public string? OrderNo { get; set; }

        // ✅ FIX: Initialize the list
        public List<TProductDet> TProductDets { get; set; } = new List<TProductDet>();
    }

    public class TProductDet
    {
        public int ItemCode { get; set; } = 0;
        public string? ItemName { get; set; }
        public int IMCCode { get; set; } = 0;
        public string? IMCName { get; set; }
        public string? VchNo { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public double Discount { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double UnitCost { get; set; }
        public string? Image { get; set; }
    }

    public class RawTProductData
    {
        public int VchCode { get; set; }
        public int VchType { get; set; }
        public string? Date { get; set; }
        public string? VchNo { get; set; }
        public string? OrderId { get; set; }
        public string? RefNo { get; set; }
        public int AccCode { get; set; }
        public string? AccName { get; set; }
        public int MCCode { get; set; }
        public string? MCName { get; set; }
        public int STType { get; set; }
        public string? STName { get; set; }
        public double SubTot { get; set; }
        public double TaxAmt { get; set; }
        public double Discount { get; set; }
        public double Shipping { get; set; }
        public double TotAmt { get; set; }
        public string? Remarks { get; set; }
        public string? PaymentGateway { get; set; }
        public string? PaymentMode { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TxnId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? GatewayTxnId { get; set; }
        public string? BankRefNo { get; set; }
        public double PayAmt { get; set; }
        public string? PaymentRemark { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }

        // Details
        public int ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int IMCCode { get; set; }
        public string? IMCName { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public double UnitCost { get; set; }
        public double ItemDiscount { get; set; }
        public double ItemTaxPer { get; set; }
        public double ItemTaxAmt { get; set; }
    }

    public class TransactionReportDto
    {
        public string? OrderId { get; set; }
        public int VchCode { get; set; }
        public string? InvNo { get; set; }
        public string? InvDt { get; set; }
        public int AccCode { get; set; }
        public string? AccName { get; set; }
        public string? PaymentStatus { get; set; }
        public double Amount { get; set; }
    }

    public class ReportSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalUnpaid { get; set; }
        public decimal Overdue { get; set; }
    }

    public class CardSummaryDto
    {
        public decimal TotalSale { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalUnpaid { get; set; }
    }

    public class SalesPurchaseChartDto
    {
        public string HourLabel { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public decimal Purchase { get; set; }
    }

    public class RecentTransaction
    {
        public string? Date { get; set; }
        public int ItemCode { get; set; }
        public string? Items { get; set; }
        public string? PaymentMode { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TxnId { get; set; }
        public decimal Amount { get; set; }
        public string? Image {  get; set; }
    }



    //public class PosOrder
    //{
    //    public int OrderId { get; set; }
    //    public string? OrderNo { get; set; }
    //    public DateTime OrderDate { get; set; }

    //    // Invoice Fields
    //    public string? InvoiceNo { get; set; }
    //    public DateTime? InvoiceDate { get; set; }

    //    // Customer
    //    public int CustomerId { get; set; }
    //    public string? CustomerName { get; set; }
    //    public string? Phone { get; set; }
    //    public string? Email { get; set; }
    //    public string? CustomerGST { get; set; }
    //    public string? BillingAddress { get; set; }
    //    public string? ShippingAddress { get; set; }

    //    // Store / Counter
    //    public int StoreId { get; set; }
    //    public int CounterId { get; set; }

    //    // Order Type & Channel
    //    public int OrderType { get; set; }         // Retail, DineIn, TakeAway, Online
    //    public string? Channel { get; set; }       // Zomato, Swiggy, OwnApp

    //    // Restaurant Fields
    //    public int TableId { get; set; }
    //    public string? TableName { get; set; }
    //    public int WaiterId { get; set; }
    //    public string? WaiterName { get; set; }

    //    // Token / Hold
    //    public string? TokenNo { get; set; }       // Takeaway token
    //    public string? HoldNo { get; set; }        // Hold bill

    //    // Billing Breakdown
    //    public double SubTotal { get; set; }
    //    public double TaxAmount { get; set; }
    //    public double CGST { get; set; }
    //    public double SGST { get; set; }
    //    public double IGST { get; set; }

    //    public double Coupon { get; set; }
    //    public double Discount { get; set; }
    //    public double Shipping { get; set; }
    //    public double PackingCharge { get; set; }
    //    public double ServiceCharge { get; set; }
    //    public double OtherCharges { get; set; }

    //    public double TotalAmount { get; set; }
    //    public double RoundOff { get; set; }
    //    public double FinalAmount { get; set; }     // After rounding

    //    // Payment Summary
    //    public int PaymentMethod { get; set; }      // Cash / UPI / Card
    //    public int PaymentStatus { get; set; }      // Pending / Success / Failed
    //    public int PaymentMode { get; set; }        // Online / Offline
    //    public int PaymentGateway { get; set; }     // Razorpay / Paytm

    //    // Split Payment
    //    public bool IsSplitPayment { get; set; }
    //    public double CashAmount { get; set; }
    //    public double CardAmount { get; set; }
    //    public double UPIAmount { get; set; }

    //    // Payment References
    //    public string? TxnId { get; set; }
    //    public string? GatewayTxnId { get; set; }
    //    public string? BankRefNo { get; set; }
    //    public string? PaymentRemark { get; set; }

    //    public double PaidAmount { get; set; }

    //    // Delivery Fields
    //    public string? DeliveryBoy { get; set; }
    //    public DateTime? DeliveryTime { get; set; }

    //    // Order Status
    //    public int OrderStatus { get; set; }        // Hold, Completed, Cancelled
    //    public string? CancelReason { get; set; }

    //    // Geo Location
    //    public double Latitude { get; set; }
    //    public double Longitude { get; set; }

    //    // Audit Info
    //    public string? CreatedBy { get; set; }
    //    public DateTime CreatedOn { get; set; }

    //    // Items
    //    public List<PosOrderItem> Items { get; set; } = new List<PosOrderItem>();
    //}

    //public class PosOrderItem
    //{
    //    public int ItemCode { get; set; }                 // Product ID
    //    public string? ItemName { get; set; }

    //    // Material Center / Godown
    //    public int MCId { get; set; }
    //    public string? MCName { get; set; }

    //    // Category / Group
    //    public int CategoryId { get; set; }
    //    public string? CategoryName { get; set; }

    //    public string? VchNo { get; set; }

    //    // Product Identifiers
    //    public string? Barcode { get; set; }
    //    public string? SKU { get; set; }
    //    public string? HSNCode { get; set; }

    //    // Batch / MFG / Expiry (Pharmacy, Grocery)
    //    public string? BatchNo { get; set; }
    //    public DateTime? ExpiryDate { get; set; }
    //    public DateTime? MfgDate { get; set; }
    //    public double MRP { get; set; }

    //    // Warehouse Location
    //    public int LocationId { get; set; }

    //    // Units
    //    public string? Unit { get; set; }                 // PCS / Box / KG / Liter
    //    public double UnitConv { get; set; }              // Conversion factor

    //    // Pricing & Quantity
    //    public double Qty { get; set; }
    //    public double Price { get; set; }
    //    public double GrossAmount { get; set; }           // Qty × Price
    //    public double Discount { get; set; }

    //    // Tax
    //    public double TaxPer { get; set; }
    //    public double TaxAmt { get; set; }
    //    public double CGSTAmount { get; set; }
    //    public double SGSTAmount { get; set; }
    //    public double IGSTAmount { get; set; }

    //    public double NetAmount { get; set; }

    //    // Costing
    //    public double UnitCost { get; set; }
    //    public bool ReduceStock { get; set; }
    //    public double OpeningStock { get; set; }
    //    public double ClosingStock { get; set; }
    //    public double BatchQuantity { get; set; }
    //    public string? SerialNumber { get; set; }
    //    public string? PurchaseRefNo { get; set; }
    //    public int PurchaseDetailId { get; set; }              // Purchase price

    //    // Image (Added)
    //    public string? ImageUrl { get; set; }

    //    // POS Special Flags
    //    public bool IsServiceItem { get; set; }           // No stock impact
    //    public bool IsComboItem { get; set; }
    //    public bool IsAddon { get; set; }

    //    // KOT Routing (Restaurant)
    //    public int KOTGroupId { get; set; }
    //    public string? KOTGroupName { get; set; }

    //    public string? Remarks { get; set; }              // Custom notes
    //}


    public class PosOrder
    {
        public int OrderId { get; set; }
        public string? OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string? OrderTime { get; set; }
        public int OrderPriority { get; set; } = 0;

        // Invoice
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? IRN { get; set; }
        public string? AckNo { get; set; }
        public DateTime? AckDate { get; set; }

        // Customer Info
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? CustomerGST { get; set; }
        public string? BillingAddress { get; set; }
        public string? ShippingAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Pincode { get; set; }

        // Store Info
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int StoreId { get; set; }
        public int CounterId { get; set; }
        public int ShiftId { get; set; }

        // Cashier
        public int CashierId { get; set; }
        public string? CashierName { get; set; }

        // Order Type
        public int OrderType { get; set; }
        public string? Channel { get; set; }
        public string? Source { get; set; }
        public int OrderSourceType { get; set; }

        // Restaurant
        public int TableId { get; set; }
        public string? TableName { get; set; }
        public int WaiterId { get; set; }
        public string? WaiterName { get; set; }
        public int Covers { get; set; }

        // Token
        public string? TokenNo { get; set; }
        public string? HoldNo { get; set; }
        public string? KOTNo { get; set; }
        public bool IsKOTPrinted { get; set; }

        // Billing
        public double TaxableAmount { get; set; }
        public double SubTotal { get; set; }
        public double ItemLevelDiscount { get; set; }
        public double OrderLevelDiscount { get; set; }
        public string? CouponCode { get; set; }
        public double CouponAmount { get; set; }

        public double CGST { get; set; }
        public double SGST { get; set; }
        public double IGST { get; set; }
        public double TaxAmount { get; set; }

        public double PackingCharge { get; set; }
        public double ServiceCharge { get; set; }
        public double DeliveryCharge { get; set; }
        public double PartnerDeliveryCharge { get; set; }
        public double OtherCharges { get; set; }
        public double TipAmount { get; set; }
        public double AdjustmentAmount { get; set; }

        public double RoundOff { get; set; }
        public double TotalAmount { get; set; }
        public double FinalAmount { get; set; }

        // Payment Summary
        public double TotalPaid { get; set; }
        public double TotalDue { get; set; }
        public int PaymentMethod { get; set; }
        public int PaymentStatus { get; set; }
        public bool IsPaid { get; set; }

        // Online Platform
        public string? PlatformOrderId { get; set; }
        public double PlatformCommission { get; set; }
        public double PlatformFee { get; set; }

        // Delivery Info
        public string? DeliveryType { get; set; }
        public int DeliveryBoyId { get; set; }
        public string? DeliveryBoy { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public string? DeliveryOtp { get; set; }
        public double CustomerLatitude { get; set; }
        public double CustomerLongitude { get; set; }

        // Status
        public int OrderStatus { get; set; }
        public bool IsHold { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsReturned { get; set; }
        public string? CancelReason { get; set; }
        public double ReturnAmount { get; set; }
        public int RefundStatus { get; set; }
        public DateTime? CancelDate { get; set; }

        // Loyalty
        public double LoyaltyPointsUsed { get; set; }
        public double LoyaltyPointsEarned { get; set; }

        // Reference
        public int RefOrderId { get; set; }
        public string? ExternalOrderId { get; set; }
        public string? ERPRef { get; set; }

        // Print
        public int PrintCount { get; set; }

        // Device Info
        public string? DeviceId { get; set; }
        public string? AppVersion { get; set; }
        public string? IPAddress { get; set; }

        // Sync
        public bool SyncStatus { get; set; }
        public DateTime? SyncDate { get; set; }

        // JSON
        public string? TaxJson { get; set; }
        public string? StatusHistory { get; set; }
        public string? Notes { get; set; }
        public string? InternalNotes { get; set; }

        // Audit
        public string? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // Items & Payments
        public List<PosOrderItem> Items { get; set; } = new();
        public List<PosOrderPayment> Payments { get; set; } = new();
    }


    public class PosOrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }

        // Product
        public int ItemCode { get; set; }
        public string? ItemName { get; set; }

        // Material Center
        public int MCId { get; set; }
        public string? MCName { get; set; }

        // Category
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Identifiers
        public string? Barcode { get; set; }
        public string? SKU { get; set; }
        public string? HSNCode { get; set; }

        // Batch / Expiry
        public string? BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? MfgDate { get; set; }
        public double MRP { get; set; }

        // Warehouse
        public int LocationId { get; set; }
        public string? Rack { get; set; }
        public string? Shelf { get; set; }

        // Units
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public double UnitConv { get; set; }
        public int SecondaryUnitId { get; set; }
        public double SecondaryQty { get; set; }

        // Qty & Pricing
        public double Qty { get; set; }
        public double Weight { get; set; }
        public double FreeQty { get; set; }
        public double ReturnQty { get; set; }

        public double Price { get; set; }
        public double GrossAmount { get; set; }
        public double DiscountPercent { get; set; }
        public double Discount { get; set; }

        // Tax
        public int TaxType { get; set; }
        public double TaxableAmount { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double CGSTAmount { get; set; }
        public double SGSTAmount { get; set; }
        public double IGSTAmount { get; set; }

        // Final Amount
        public double NetAmount { get; set; }

        // Costing
        public double UnitCost { get; set; }
        public double WeightedAvgCost { get; set; }

        // Stock Tracking
        public bool ReduceStock { get; set; }
        public double OpeningStock { get; set; }
        public double ClosingStock { get; set; }
        public double BatchQuantity { get; set; }
        public string? SerialNumber { get; set; }
        public string? PurchaseRefNo { get; set; }
        public int PurchaseDetailId { get; set; }

        // Image
        public string? ImageUrl { get; set; }

        // Flags
        public int ItemType { get; set; }
        public bool IsComboItem { get; set; }
        public int ComboParentId { get; set; }
        public bool IsAddon { get; set; }
        public string? AddonGroup { get; set; }

        // Surcharge / Modifiers
        public double SurchargeAmount { get; set; }
        public string? ModifiersJson { get; set; }

        // KOT
        public int KOTId { get; set; }
        public int KOTGroupId { get; set; }
        public string? KOTGroupName { get; set; }
        public bool IsKOTPrinted { get; set; }
        public DateTime? KOTPrintTime { get; set; }
        public string? KOTRemark { get; set; }

        // Extra
        public string? Remarks { get; set; }
        public string? ItemNote { get; set; }

        // Audit
        public DateTime CreatedOn { get; set; }
    }


    public class PosOrderPayment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }

        public int PaymentMethod { get; set; }
        public int PaymentMode { get; set; }
        public int PaymentType { get; set; }
        public int PaymentSubType { get; set; }

        public int PaymentGateway { get; set; }
        public string? TxnId { get; set; }
        public string? GatewayTxnId { get; set; }
        public string? BankRefNo { get; set; }
        public string? GatewayOrderId { get; set; }
        public string? GatewayStatus { get; set; }

        public double Amount { get; set; }
        public double PaidAmount { get; set; }
        public double DueAmount { get; set; }

        public int PaymentStatus { get; set; }
        public DateTime? PaymentTime { get; set; }
        public int PaymentRetryCount { get; set; }

        public bool IsSplitPayment { get; set; }
        public bool IsRefund { get; set; }
        public double RefundAmount { get; set; }
        public DateTime? RefundDate { get; set; }

        public int SettlementStatus { get; set; }
        public DateTime? SettlementDate { get; set; }
        public bool IsReconciled { get; set; }
        public DateTime? ReconciledOn { get; set; }
        public string? ReconciledBy { get; set; }

        public double GatewayFee { get; set; }
        public double GatewayTax { get; set; }
        public double NetSettlementAmount { get; set; }

        public string? PaymentIPAddress { get; set; }
        public string? PaymentDeviceId { get; set; }
        public string? PaymentNotes { get; set; }
        public string? PaymentResponseJson { get; set; }

        public string Currency { get; set; } = "INR";
        public double ExchangeRate { get; set; } = 1;

        public string? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }


    public class PosOrderItem1
    {
        public int ItemCode { get; set; } = 0;
        public string? ItemName { get; set; }
        public int IMCCode { get; set; } = 0;
        public string? IMCName { get; set; }
        public string? VchNo { get; set; }
        public double Qty { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public double Discount { get; set; }
        public double TaxPer { get; set; }
        public double TaxAmt { get; set; }
        public double UnitCost { get; set; }
        public string? Image { get; set; }
    }
}
