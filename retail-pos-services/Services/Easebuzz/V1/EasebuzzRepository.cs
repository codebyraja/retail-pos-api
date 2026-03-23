using Easebuzz.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QSRAPIServices.Models;
using RetailPosContext.DBContext;
using RetailPosRepository.Services.Repository;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EasebuzzPayment.Services.V1
{
    public class EasebuzzRepositoryV1 : IEasebuzzRepositoryV1
    {
        private readonly RetailPosDBContext _db;
        private readonly IConfiguration _config;
        private readonly IRepository _service;
        public readonly ILogger<EasebuzzRepositoryV1> _logger;

        public EasebuzzRepositoryV1(RetailPosDBContext db, IConfiguration config, IRepository service, ILogger<EasebuzzRepositoryV1> logger) 
        {
            _db = db;
            _config = config;
            _service = service;
            _logger = logger;
        }

        public Task<string> GeneratePaymentFormHtmlAsync(EasebuzzPaymentRequest req)
        {
            var key = _config["EasebuzzV1:Key"];
            var salt = _config["EasebuzzV1:Salt"];
            var txnid = Guid.NewGuid().ToString("N").Substring(0, 20);

            var hash = GenerateHash(key, salt, txnid, req.Amount, req.Purpose, req.BuyerName, req.Email);
            var surl = _config["EasebuzzV1:RedirectUrl1"];
            var furl = _config["EasebuzzV1:RedirectUrl2"];

            var html = $@"
                <html><body onload='document.forms[0].submit()'>
                <form method='post' action='https://testpay.easebuzz.in/payment/initiateLink'>
                <input type='hidden' name='key' value='{key}'/>
                <input name='txnid' value='{txnid}'/>
                <input name='amount' value='{req.Amount}'/>
                <input name='firstname' value='{req.BuyerName}'/>
                <input name='email' value='{req.Email}'/>
                <input name='phone' value='{req.Phone}'/>
                <input name='productinfo' value='{req.Purpose}'/>
                <input name='surl' value='{surl}'/>
                <input name='furl' value='{furl}'/>
                <input name='hash' value='{hash}'/>
                </form></body></html>";
            return Task.FromResult(html);
        }

        public async Task<string> GenerateAccessKeyOnlyAsync(EasebuzzPaymentRequest req)
        {
            var key = _config["EasebuzzV1:Key"];
            var salt = _config["EasebuzzV1:Salt"];
            var baseUrl = _config["EasebuzzV1:BaseUrl"] ?? "https://pay.easebuzz.in";
            //var txnid = "TXN_" + Guid.NewGuid().ToString("N").Substring(0, 20);
            //var amount = Convert.ToDecimal(req.Amount).ToString("0.00");

            var hashString = $"{key}|{req.TxnId}|{req.Amount}|{req.Purpose}|{req.BuyerName}|{req.Email}|||||||||||{salt}";

            using var sha = SHA512.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(hashString));
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            var payload = new Dictionary<string, string>
            {
                { "key", key },
                { "txnid", req.TxnId },
                { "amount", req.Amount },
                { "productinfo", req.Purpose },
                { "firstname", req.BuyerName },
                { "email", req.Email },
                { "phone", req.Phone },
                { "surl", _config["EasebuzzV1:RedirectUrl1"] },
                { "furl", _config["EasebuzzV1:RedirectUrl2"] },
                { "webhook", _config["EasebuzzV1:WebhookUrl"] },
                { "hash", hash }
            };

            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(payload);
            var response = await client.PostAsync($"{baseUrl}/payment/initiateLink", content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                //throw new Exception($"Easebuzz API Error: {response.StatusCode} - {responseBody}");
                throw new Exception($"Easebuzz API Error: {response.StatusCode} - {response.ReasonPhrase}");

            var json = JsonDocument.Parse(responseBody);

            if (json.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.String)
            {
                var accessKey = data.GetString();
                //return $"https://testpay.easebuzz.in/pay/{accessKey}";
                return $"{baseUrl}/pay/{accessKey}";

            }
            throw new Exception($"Easebuzz returned an unexpected response: {responseBody}");
        }

        public async Task<dynamic> CreateOrderAndInitiatePaymentAsync(TProduct order)
        {
            try
            {
                // 1 Generate unique TxnId FIRST
                // order.TxnId = "TXN_" + Guid.NewGuid().ToString("N").Substring(0, 20);
                //order.TxnId = $"TXN_{Guid.NewGuid():N}".Substring(0, 20);
                order.TxnId = "TXN_" + Guid.NewGuid().ToString("N").Substring(0, 16);

                var saveResult = await _service.SaveOrUpdateTransactionDetAsync(order);


                // var getResult = await _service.GetTransactionsDetAsync(9, orderRequest.VchCode);

                if (saveResult.Status == 0) return new { Status = 0, Code = "ORDER_SAVE_FAILED", Msg = saveResult.Msg, Data = (object)null, Timestamp = DateTime.UtcNow, ApiVersion = "1.0" };

                // 2️ Prepare Easebuzz payment request
                var req = new EasebuzzPaymentRequest
                {
                    TxnId = order.TxnId,
                    Amount = order.TotAmt.ToString("0.00"),
                    BuyerName = order.AccName,
                    Email = order.Email ?? "noreply@example.com",
                    Phone = order.Phone ?? "9999999999",
                    Purpose = "Cart Payment"
                };

                // 3 Generate payment URL
                var paymentUrl = await GenerateAccessKeyOnlyAsync(req);

                return new { Status = 1, Code = "ORDER_CREATED", Msg = "Order created and payment initiated", Data = new { Order = new { OrderId = saveResult.OrderId, OrderNo = saveResult.OrderNo }, Transaction = new { TxnId = order.TxnId, PaymentUrl = paymentUrl } }, Timestamp = DateTime.UtcNow, ApiVersion = "1.0" };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Code = "EXCEPTION", Msg = ex.Message, Data = (object)null, Timestamp = DateTime.UtcNow, ApiVersion = "1.0" };
            }
        }

        private string GenerateHash(string key, string salt, string txnid, string amount, string productinfo, string firstname, string email)
        {
            string hashString = $"{key}|{txnid}|{amount}|{productinfo}|{firstname}|{email}|||||||||||{salt}";
            using var sha = SHA512.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(hashString));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        //public bool VerifyHash(EasebuzzWebhookRequest request)
        //{
        //    var key = _config["EasebuzzV1:Key"];
        //    var salt = _config["EasebuzzV1:Salt"];

        //    // According to Easebuzz docs: hash = SHA512(key|txnid|amount|productinfo|firstname|email|...|salt)
        //    var plain = $"{key}|{request.TxnId}|{request.Amount}|{request.Productinfo}|{request.Firstname}|{request.Email}|||||||||||{salt}";
        //    using var sha = SHA512.Create();
        //    var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
        //    var generatedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        //    return generatedHash == request.Hash;
        //}

        public bool VerifyHash(EasebuzzWebhookRequest request)
        {
            try
            {
                var key = _config["EasebuzzV1:Key"];
                var salt = _config["EasebuzzV1:Salt"];

                string status = request.Status?.Trim() ?? "";
                string email = request.Email?.Trim() ?? "";
                string firstname = request.FirstName?.Trim() ?? "";
                string product = request.Productinfo?.Trim() ?? "";
                string amount = request.Amount?.Trim() ?? "";
                string txn = request.TxnId?.Trim() ?? "";

                // Webhook response ka hash pattern
                //string hashString = $"{salt}|{request.Status}|||||||||||{request.Email}|{request.FirstName}|{request.Productinfo}|{request.Amount}|{request.TxnId}|{key}";

                string hashString = $"{salt}|{status}|||||||||||{email}|{firstname}|{product}|{amount}|{txn}|{key}";

                using var sha = SHA512.Create();
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(hashString));
                var generatedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                // 👇 Debug logs add kare
                _logger.LogInformation("GeneratedHash: {GeneratedHash}", generatedHash);
                _logger.LogInformation("ReceivedHash: {ReceivedHash}", request.Hash);
                _logger.LogInformation("HashString Used: {HashString}", hashString);

                //return generatedHash == request.Hash;
                return SlowEquals(generatedHash, request.Hash?.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in VerifyHash: {ErrorMessage}", ex.Message);
                return false;
            }
        }
        
        
        
        private bool SlowEquals(string a, string b)
        {
            if (a == null || b == null) return false;

            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);

            return diff == 0;
        }

        public async Task<dynamic> UpdatePaymentStatusAsync(string txnId, string status, string easepayId, string bankRefNo, string mode)
        {
            try
            {
                //var order = await _db.EasebuzzWebhookRequests.FirstOrDefaultAsync(o => o.TxnId == txnId);
                //if (order == null)
                //    return new { Status = 0, Msg = "Order not found" };

                //order.GatewayTxnId = txnId;
                //order.BankRefNo = bankRefNo;

                //Proper status management
                //switch (status.ToLower())
                //{
                //    case "success":
                //        order.Remarks = "Payment Success";
                //        order.PaymentStatus = "SUCCESS"; // add this column if not exists
                //        break;
                //    case "failure":
                //        order.Remarks = "Payment Failed";
                //        order.PaymentStatus = "FAILED";
                //        break;
                //    case "userCancelled":
                //        order.Remarks = "User Cancelled";
                //        order.PaymentStatus = "CANCELLED";
                //        break;
                //    default:
                //        order.Remarks = "Unknown";
                //        order.PaymentStatus = "PENDING";
                //        break;
                //}

                //await _db.SaveChangesAsync();

                if (!string.IsNullOrEmpty(txnId)) 
                {
                    _logger.LogInformation("Updating payment status for TxnId: {TxnId} with Status: {Status}, EasepayId: {EasepayId}, BankRefNo: {BankRefNo}, Mode: {Mode}", txnId, status, easepayId, bankRefNo, mode);
                }
                else
                {
                    _logger.LogWarning("TxnId is null or empty while trying to update payment status.");
                    return new { Status = 0, Msg = "Invalid TxnId" };
                }

                string sql = @"UPDATE PaymentTransaction SET PaymentStatus = @status, Remarks = @remarks, GatewayTxnId = @easepayId, BankRefNo = @bankRefNo, PaymentMode = @mode WHERE TxnId = @txnId";

                var parameters = new[]
                    { 
                        new SqlParameter("@status", status.ToUpper()),
                        new SqlParameter("@remarks", status == "success" ? "Payment Success" : status == "failure" ? "Payment Failed" : status == "userCancelled" ? "User Cancelled" : "Pending"),
                        new SqlParameter("@easepayId", easepayId ?? (object)DBNull.Value),
                        new SqlParameter("@bankRefNo", bankRefNo ?? (object)DBNull.Value),
                        new SqlParameter("@txnId", txnId),
                        new SqlParameter("@mode", mode.ToUpper()),
                    };

                int rows = await _db.Database.ExecuteSqlRawAsync(sql, parameters);


                return new { Status = 1, Msg = "Payment updated successfully" };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message };
            }
        }

        public async Task<dynamic> GetPaymentStatusAsync(int orderId)
        {
            string sql = $"Select A.[VchCode], ISNULL(B.[PaymentStatus], '') As PaymentStatus, ISNULL(B.[PaymentMode], '') As PaymentMode, ISNULL(B.[Remarks], '') As Remarks, ISNULL(B.[GatewayTxnId], '') As GatewayTxnId, ISNULL(B.[BankRefNo], '') As BankRefNo From RJTran1 A INNER JOIN PaymentTransaction B ON A.VchCode = B.VchCode Where A.VchCode = {orderId}";
            var order = await _db.PaymentStatusDtos.FromSqlRaw(sql).FirstOrDefaultAsync();  

            if (order == null)
                return new { Status = 0, Msg = "Order not found" };

            var dto = new PaymentStatusDto
            {
                VchCode = order.VchCode,
                PaymentStatus = order.PaymentStatus,
                PaymentMode = order.PaymentMode,
                Remarks = order.Remarks,
                GatewayTxnId = order.GatewayTxnId,
                BankRefNo = order.BankRefNo
            };

            return new { Status = 1, Msg = order?.Remarks,  Data = dto };
        }

        //public async Task<dynamic> UpdatePaymentStatusAsync(int orderId, string status, string txnId, string bankRefNo)
        //{
        //    try
        //    {
        //        var order = await _db.EasebuzzWebhookRequests.FirstOrDefaultAsync(o => o.VchCode == orderId);
        //        if (order == null)
        //            return new { Status = 0, Msg = "Order not found" };

        //        order.Remarks = status;  // ✅ तुम्हारे पास PaymentStatus column नहीं था, Remarks में डाल दो या नया column add कर लो
        //        order.GatewayTxnId = txnId;
        //        order.BankRefNo = bankRefNo;
        //        //order.Date = DateTime.Now.ToString("dd-MMM-yyyy");

        //        await _db.SaveChangesAsync();

        //        return new { Status = 1, Msg = "Payment updated successfully" };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { Status = 0, Msg = ex.Message };
        //    }
        //}

        //private string GenerateHash(string key, string salt, string txnid, string amount, string productinfo, string firstname, string email)
        //{
        //    var plain = $"{key}|{txnid}|{amount}|{productinfo}|{firstname}|{email}|||||||||||{salt}";
        //    using (var sha = SHA512.Create())
        //    {
        //        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
        //        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        //    }
        //}
    }
}
