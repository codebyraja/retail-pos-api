using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easebuzz.Models;
using QSRAPIServices.Models;

namespace EasebuzzPayment.Services.V1
{
    public interface IEasebuzzRepositoryV1
    {
        Task<string> GenerateAccessKeyOnlyAsync(EasebuzzPaymentRequest model);
        Task<string> GeneratePaymentFormHtmlAsync(EasebuzzPaymentRequest request);
        Task<dynamic> CreateOrderAndInitiatePaymentAsync(TProduct product);
        //Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusRequest request);
        //Task<bool> ProcessWebhookAsync(EasebuzzWebhookRequest request);
        Task<dynamic> UpdatePaymentStatusAsync(string txnId, string status, string easepayId, string bankRefNo, string mode);
        bool VerifyHash(EasebuzzWebhookRequest request);
        Task<dynamic> GetPaymentStatusAsync(int orderId);
    }
}
