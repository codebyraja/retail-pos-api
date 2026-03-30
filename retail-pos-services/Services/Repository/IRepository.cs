using Master.Models;
using Microsoft.AspNetCore.Http;
using QSRAPIServices.Models;
using Razorpay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailPosRepository.Services.Repository
{
    public interface IRepository
    {
        Task<dynamic> ValidateUserAsync(string username, string password);
        Task<Response> SaveMasterAsync(Master1 master, IFormFile image);
        Task<dynamic> GetMasterListAsync(int masterType);
        Task<dynamic> GetMasterAsync(int tranType, int masterType, int code, string? name);
        Task SaveVariantAsync(int itemCode, ItemVariantDto v);
        //Task<dynamic> GetVariantAsync(int itemCode);
        Task<Response> SaveAddonAsync(Addon addon);
        Task<dynamic> GetAddonsAsync(int itemCode);
        Task<Response> SaveTableAsync(RestaurantTable table);





        Task<dynamic> GetMasterListByType(int tranType, int masterType, int code, string? name);
        Task<dynamic> GetMasterNameToCode(int masterType, string name);
        Task<dynamic> GetMasterCodeToName(int masterType, int code);
        Task<dynamic> Signup(SignupRequest user);
        Task SaveRefreshToken(int userid, string token, DateTime expiry);
        Task<UserModel> GetUserByRefreshToken(string token);
        Task<string> UpdateRefreshToken(string oldToken, string newToken, DateTime expiry);
        Task<bool> RevokeRefreshToken(string refreshToken);
        Task RevokeAllTokensForUser(int userid);
        Task<Response> SendOtpToEmailAsync(string email);
        Task<Response> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword);
        Task<dynamic> SaveMasterDetailRequest(SaveMasterRequest request);
        Task<dynamic> SaveProductMasterDetails(SaveProductMasterRequest request);  
        Task<dynamic> GetMasterDetails(int masterType, int code);
        Task<dynamic> GetProductMasterDetails(int masterType, int code);
        Task<ImageUploadResult> UploadProductImagesAsync(int productId, List<IFormFile> images, string name, string path);
        Task<dynamic> DeleteMasterByTypeAndCode(int tranType, int masterType, int code);
        Task<dynamic> SaveUserMasterRequest(SaveUserMasterRequest request);
        Task<dynamic> GetUserMasterDetAsync(int userType, int code);
        Task<dynamic> SaveProductsFromExcel(ImportProductRequest request);
        Task<dynamic> UpdateStockFromExcel(ImportStockRequest request);
        Task<UpiTransaction> CreateTransaction(decimal amount);
        Task<UpiTransaction> GetStatus(string transactionId);
        Task SimulatePaymentStatusUpdate(string transactionId);
        Task SubmitFeedback(Feedback feedback);
        Task<dynamic> GetPosCategoriesWithProductsAsync();
        Task<dynamic> GetPosCategoriesAsync();
        Task<dynamic> GetPosProductsAsync();
        Task<dynamic> SaveOrUpdateCustomerDetAsync(SvCustomerDet obj);
        Task<dynamic> GetCustomerDetAsync(int code);
        Task<dynamic> GetVchNo(int tranType, int vchType);
        Task<dynamic> GetProductSearchAsync(int tranType, int masterType, int code, SearchProduct obj);
        Task<dynamic> SaveOrUpdateTransactionDetAsync(TProduct obj);
        Task<dynamic> DeleteTransactionDetAsync();
        Task<dynamic> GetTransactionsDetAsync(int vchType, int? vchCode = null);
        Task<dynamic> GetTransactionsReportAsync(int vchType, string? startDate, string? endDate, string? customer, int? status);
        //Task<dynamic> GetInvoiceReportSummaryAsync(int vchType, string? startDate, string? endDate, string? customer, int? status);
        Task<dynamic> GetCardSummaryAsync();
        Task<dynamic> GetSalesDayChartAsync(string range);
        Task<dynamic> GetRecentTransactionsAsync();
        Task<int> GetOrderIdByTxnId(string txnId);
    }
}
