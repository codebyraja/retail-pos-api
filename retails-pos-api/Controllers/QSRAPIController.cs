using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using QSRAPIServices.Models;
using QSRRepositoryServices.Services.Repository;
using QSRTokenService.Services.Token;
using Razorpay.Models;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace QSRAPIController.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[action]")]
    [ApiController]
    public class QSRAPIController : ControllerBase
    {
        public readonly IRepository _services;
        public readonly IPaymentService _paymentService;
        public readonly ITokenService _tokenService;

        public QSRAPIController(IRepository services, ITokenService tokenService, IPaymentService paymentService)
        {
            this._services = services;
            this._tokenService = tokenService;
            _paymentService = paymentService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest login)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(login.Username))
                return BadRequest(new { Status = 0, Msg = "Username is required." });

            if (string.IsNullOrWhiteSpace(login.Password))
                return BadRequest(new { Status = 0, Msg = "Password is required." });

            if (!ModelState.IsValid)
                return BadRequest(new { Status = 0, Msg = "Invalid request data." });

            var result = await _services.ValidateUserAsync(login.Username, login.Password);

            if (result.Status != 1 || result.Data == null)
                return Unauthorized(new { Status = 0, Msg = result.Msg });

            var user = result.Data;
            int userId = user.Id;

            // JWT + Refresh Token logic
            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            await _services.RevokeAllTokensForUser(userId);
            await _services.SaveRefreshToken(userId, refreshToken, DateTime.UtcNow.AddDays(7));

            return Ok(new
            {
                Status = 1,
                Msg = "Login successful",
                Token = accessToken,
                RefreshToken = refreshToken,
                Data = user
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest model)
        {
            if (!_tokenService.ValidateRefreshToken(model.RefreshToken))
            {
                return Unauthorized(new { Status = 0, Msg = "Invalid refresh token." });
            }

            var user = await _services.GetUserByRefreshToken(model.RefreshToken);
            if (user == null)
            {
                return Unauthorized(new { Status = 0, Msg = "Refresh token expired or invalid." });
            }

            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _services.UpdateRefreshToken(model.RefreshToken, newRefreshToken, DateTime.UtcNow.AddDays(7));

            return Ok(new
            {
                Status = 1,
                Msg = "Token refreshed successfully",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { Status = 0, Msg = "Refresh token is required." });
            }

            var result = await _services.RevokeRefreshToken(request.RefreshToken);

            if (!result)
            {
                return BadRequest(new { Status = 0, Msg = "Invalid or already revoked token." });
            }

            return Ok(new { Status = 1, Msg = "User logged out and token revoked successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupRequest user)
        {
            return Ok(await _services.Signup(user));
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] EmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { Status = 0, Msg = "Email is required." });
            }

            var result = await _services.SendOtpToEmailAsync(request.Email);

            if (result.Status == 1)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("verify-otp-and-reset")]
        public async Task<IActionResult> VerifyOtpAndReset([FromBody] ResetPasswordRequestWithOtp req)
        {
            var result = await _services.VerifyOtpAndResetPasswordAsync(req.Email, req.Otp, req.NewPassword);
            return Ok(result);
        }

        [HttpGet("dashboard/card-summary")]
        public async Task<IActionResult> GetCardSummaryAsync()
        {
            return Ok (await _services.GetCardSummaryAsync());
        }

        [HttpGet("dashboard/day-chart")]
        public async Task<IActionResult> GetSalesPurchaseDayChartAsync(string range)
        {
            return Ok(await _services.GetSalesDayChartAsync(range));
        }

        [HttpGet("dashboard/recent-transactions")]
        public async Task<IActionResult> GetRecentTransactionsAsync()
        {
            return Ok(await _services.GetRecentTransactionsAsync());
        }

        [HttpGet("generate-sku")]
        public IActionResult GenerateSku()
        {
            //var sku = $"SKU-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            var helper = new QSRHelperApiServices.Helper.Helper();
            var sku = helper.GenerateSku();
            return Ok(new { Status = 1, Msg = "SKU generated successfully", Sku = sku });
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SaveMasterDetailRequest([FromForm] SaveMasterRequest obj)
        {
            return Ok(await _services.SaveMasterDetailRequest(obj));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SaveProductMasterDetails([FromForm] SaveProductMasterRequest obj)
        {
            return Ok(await _services.SaveProductMasterDetails(obj));
        }

        [HttpGet("{masterType:int}")]
        public async Task<IActionResult> GetMasterDetails(int masterType, int code)
        {
            return Ok(await _services.GetMasterDetails(masterType, code));
        }

        [HttpGet("{masterType:int}")]
        public async Task<IActionResult> GetProductMasterDetails(int masterType, int code)
        {
            return Ok(await _services.GetProductMasterDetails(masterType, code));
        }

        [HttpGet("masters")]
        public async Task<IActionResult> GetMasterListByType(int tranType, int masterType, int code, string? name)
        {
            if (tranType == 0) return BadRequest(new { Status = 0, Msg = "tranType type is required." });
                
            if (masterType == 0) return BadRequest(new { Status = 0, Msg = "Master type is required." });

            return Ok(await _services.GetMasterListByType(tranType, masterType, code, name));
        }

        [HttpGet()]
        public async Task<IActionResult> GetMasterNameToCode(int masterType, string name)
        {
            return Ok(await _services.GetMasterNameToCode(masterType, name));
        }

        [HttpGet()]
        public async Task<IActionResult> GetMasterCodeToName(int masterType, int code)
        {
            return Ok(await _services.GetMasterCodeToName(masterType, code));
        }

        [HttpPost()]
        public async Task<IActionResult> DeleteMasterByTypeAndCode(int tranType, int masterType, int code)
        {
            if (tranType == 0)
                return BadRequest(new { Status = 0, Msg = "Transaction type is required." });
            if (masterType == 0)
                return BadRequest(new { Status = 0, Msg = "Master type is required." });
            if (code <= 0)
                return BadRequest(new { Status = 0, Msg = "Code must be greater than zero." });
            if (tranType < 1 || tranType > 3)
                return BadRequest(new { Status = 0, Msg = "Invalid transaction type. Must be between 1 and 3." });

            return Ok(await _services.DeleteMasterByTypeAndCode(tranType, masterType, code));
        }

        [HttpPost("users")]
        public async Task<IActionResult> SaveUserMasterDetails(SaveUserMasterRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Status = 0, Msg = "Invalid request data." });
            }
            return Ok(await _services.SaveUserMasterRequest(request));
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUserMasterDetails(int userType, int code)
        {
            return Ok(await _services.GetUserMasterDetAsync(userType, code));
        }

        [HttpPost]
        public async Task<IActionResult> SaveProductsFromExcel(ImportProductRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Status = 0, Msg = "Invalid request data." });
            }
            return Ok(await _services.SaveProductsFromExcel(request));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStockFromExcel(ImportStockRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { Status = 0, Msg = "Invalid request data." });
            }
            return Ok(await _services.UpdateStockFromExcel(request));
        }

        //[HttpPost("create")]
        //public async Task<IActionResult> Create([FromBody] decimal amount)
        //{
        //    var txn = await _services.CreateTransaction(amount);
        //    var upiUrl = $"upi://pay?pa=9971620518@ptsbi&pn=NXI&tn=OrderPayment&am={amount}&cu=INR&tr={txn.TransactionId}";
        //    return Ok(new { transactionId = txn.TransactionId, upiUrl });
        //}

        //[HttpGet("status/{transactionId}")]
        //public async Task<IActionResult> GetStatus(string transactionId)
        //{
        //    var txn = await _services.GetStatus(transactionId);
        //    if (txn == null) return NotFound();
        //    return Ok(new { status = txn.Status, amount = txn.Amount });
        //}

        //[HttpPost("feedback")]
        //public async Task<IActionResult> SubmitFeedback([FromBody] Feedback feedback)
        //{
        //    await _services.SubmitFeedback(feedback);
        //    return Ok(new { message = "Feedback saved" });
        //}

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] RazorpayOrderRequest request)
        {
            var response = await _paymentService.CreateOrderAsync(request);
            return Ok(response);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Payment([FromForm] string razorpay_order_id, [FromForm] string razorpay_payment_id, [FromForm] string razorpay_signature)
        {
            var isValid = await _paymentService.MarkPaymentSuccessAsync(razorpay_order_id, razorpay_payment_id, razorpay_signature);
            return isValid ? Ok("Payment Verified") : BadRequest("Invalid Signature");
        }

        [HttpGet("categories-with-products")]
        public async Task<IActionResult> GetCategoriesWithProducts(int tranType)
        {
            var result = tranType switch
            {
                1 => await _services.GetPosCategoriesAsync(),
                2 => await _services.GetPosProductsAsync(),
                _ => await _services.GetPosCategoriesWithProductsAsync()
            };
            return Ok(result);
        }

        [HttpPost("customers")]
        public async Task<IActionResult> SaveOrUpdateCustomerDet([FromBody] SvCustomerDet obj)
        {
            return Ok(await _services.SaveOrUpdateCustomerDetAsync(obj));
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomerMasterDet(int code)
        {
            return Ok(await _services.GetCustomerDetAsync(code));   
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetVchNo(int tranType, int vchtype)
        {
            if (vchtype == 0)
                return BadRequest(new { Status = 0, Msg = "VchType is required" });
            return Ok(await _services.GetVchNo(tranType, vchtype));
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchProducts(int tranType, int masterType, int code, SearchProduct obj)
        {
            return Ok(await _services.GetProductSearchAsync(tranType, masterType, code, obj));
        }

        [HttpPost("transactions/{type}")]  
        public async Task<IActionResult> SaveOrUpdateTransactionDet(string type, [FromBody] TProduct obj)
        {
            if (string.IsNullOrEmpty(type))
                return BadRequest(new { Status = 0, Msg = "Transaction type is required" });

            // case insensitive bana lo
            //type = type.ToLower();

            if (type?.ToLower() != "purchase" && type?.ToLower() != "sale")
            {
                return BadRequest(new { Status = 0, Msg = "Invalid transaction type. Use 'purchase' or 'sale'." });
            }
            return Ok(await _services.SaveOrUpdateTransactionDetAsync(obj));
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactionsDet(int vchType, int vchCode)
        {
             return Ok(await _services.GetTransactionsDetAsync(vchType, vchCode));
        }

        [HttpGet("transactions/report")]
        public async Task<IActionResult> GetTransactionsReportAsync(int vchType, string? startDate, string? endDate, string? customer, int? status)
        {
            return Ok(await _services.GetTransactionsReportAsync(vchType, startDate, endDate, customer, status));
        }

        //[HttpGet("transactions/invoice/summary")]
        //public async Task<IActionResult> GetInvoiceReportSummaryAsync(int vchType, string? startDate, string? endDate, string? customer, int? status)
        //{
        //    return Ok(await _services.GetInvoiceReportSummaryAsync(vchType, startDate, endDate, customer, status));
        //}
    } 
}
