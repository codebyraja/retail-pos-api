using Easebuzz.Models;
using EasebuzzPayment.Services.V1;
using EasebuzzPayment.Services.V2;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Payments.RealTime;
using QSRAPIServices.Models;
using QSRRepositoryServices.Services.Repository;
using QSRTokenService.Services.Token;

namespace Payments.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/easebuzz")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ILogger<PaymentsController> _logger;
        public readonly IEasebuzzRepositoryV1 _easebuzzRepoV1;
        public readonly IEasebuzzRepositoryV2 _easebuzzRepoV2;
        public readonly ITokenService _tokenService;
        public readonly IRepository _repository;

        public PaymentsController(ILogger<PaymentsController> logger, IEasebuzzRepositoryV1 easebuzzRepoV1, IEasebuzzRepositoryV2 easebuzzRepoV2, ITokenService tokenService, IRepository repository)
        {
            _logger = logger;
            _easebuzzRepoV1 = easebuzzRepoV1;
            _easebuzzRepoV2 = easebuzzRepoV2;
            _tokenService = tokenService;
            _repository = repository;
        }

        // Initiate Payment (HTML Form)
        [HttpPost("initiate/v1")]
        public async Task<IActionResult> InitiateV1([FromBody] EasebuzzPaymentRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (request == null) return BadRequest("Request cannot be null");

            var html = await _easebuzzRepoV1.GeneratePaymentFormHtmlAsync(request);
            return Content(html, "text/html");
        }

        // Generate Access Key only
        [HttpPost("initiate/accesskey")]
        public async Task<IActionResult> InitiatePayment([FromBody] EasebuzzPaymentRequest req)
        {
            var accessKey = await _easebuzzRepoV1.GenerateAccessKeyOnlyAsync(req);

            if (string.IsNullOrEmpty(accessKey))
                return BadRequest("Failed to generate access key");

            return Ok(accessKey); // frontend will load: https://testpay.easebuzz.in/pay/ and live for this https://pay.easebuzz.in/pay/<accessKey>
        }

        // Create Order and Initiate Payment
        [HttpPost("orders/initiate-payment")]
        //public async Task<IActionResult> CreateOrderAndInitiatePayment([FromBody] TProduct model)
        public async Task<IActionResult> InitiateOrder([FromBody] TProduct model)
        {
            if (model == null)
                return BadRequest("Product cannot be null");

            if (model.PaymentMethod == "online")
                return Ok(await _easebuzzRepoV1.CreateOrderAndInitiatePaymentAsync(model));
            else
                return Ok(await _repository.SaveOrUpdateTransactionDetAsync(model));
        }

        // Webhook for payment update
        [HttpPost("webhook")]
        public async Task<IActionResult> EasebuzzWebhook([FromForm] EasebuzzWebhookRequest request, [FromServices] IHubContext<PaymentsHub> hub)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Webhook called with empty request body.");
                    return BadRequest(ApiResponse.Fail("INVALID_REQUEST", "Empty webhook request"));
                }

                _logger.LogInformation("Webhook received for TxnId={TxnId}", request.TxnId);

                // 1) Validate Hash
                bool valid = _easebuzzRepoV1.VerifyHash(request);

                if (!valid)
                {
                    _logger.LogWarning("Invalid hash signature for Txn {TxnId}", request.TxnId);
                    return BadRequest(ApiResponse.Fail("INVALID_SIGNATURE", "Invalid hash signature"));
                }
                //return BadRequest(new { Status = 0, Msg = "Invalid hash signature" });

                var result = await _easebuzzRepoV1.UpdatePaymentStatusAsync(request.TxnId, request.Status, request.EasepayId, request.BankRefNum, request.Mode);

                // 3) Resolve OrderId from TxnId (IMPORTANT)
                int orderId = await _repository.GetOrderIdByTxnId(request.TxnId);
                
                if (orderId <= 0)
                {
                    _logger.LogWarning("Order not found for TxnId={TxnId}", request.TxnId);
                }
                else
                {
                    // 4) Real-time PUSH to frontend via SignalR
                    await hub.Clients.Group(orderId.ToString())
                        .SendAsync("PaymentStatusUpdated", new
                        {
                            orderId = orderId,
                            txnId = request.TxnId,
                            status = request.Status.ToUpper(),
                            mode = request.Mode,
                            bankRef = request.BankRefNum,
                            amount = request.Amount
                        });

                    _logger.LogInformation("Real-time payment update pushed for OrderId={OrderId}", orderId);
                }

                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing error for TxnId={TxnId}", request?.TxnId);
                return BadRequest(ApiResponse.Fail("EXCEPTION", ex.Message));
            }
        }

        // Check Payment Status
        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetPaymentStatus(int orderId)
        {
            return Ok(await _easebuzzRepoV1.GetPaymentStatusAsync(orderId));
        }

        [HttpPost("/payment/redirect/success")]
        public IActionResult RedirectSuccess()
        {
            return Content(@"
            <html>
              <body style='font-family: Arial; padding: 40px; text-align:center;'>

                <h2>Payment Successful</h2>
                <p>This window will close automatically...</p>

                <script>
                  try {
                    // Send to parent (iframe)
                    window.parent.postMessage({ paymentStatus: 'success' }, '*');

                    // Send to opener (new tab)
                    if (window.opener) {
                        window.opener.postMessage({ paymentStatus: 'success' }, '*');
                    }
                    window.close()
                    // setTimeout(() => window.close(), 1500);

                  } catch (e) { console.error(e); }
                </script>

              </body>
            </html>
            ", "text/html");
        }

        [HttpPost("/payment/redirect/failure")]
        public IActionResult RedirectFailure()
        {
            return Content(@"
        <html>
          <body style='font-family: Arial; padding: 40px; text-align:center;'>

            <h2>Payment Failed</h2>
            <p>You may close this window.</p>

            <script>
              try {
                window.parent.postMessage({ paymentStatus: 'failed' }, '*');
                if (window.opener) {
                  window.opener.postMessage({ paymentStatus: 'failed' }, '*');
                }
                window.close()
                // setTimeout(() => window.close(), 500);
              } catch(e) {}
            </script>

          </body>
        </html>
        ", "text/html");
        }


        //[HttpPost("/payment/redirect/failure")]
        //public IActionResult RedirectFailure()
        //{
        //    return Content(@"
        //        <html><body>
        //        <script>
        //          try {
        //            window.opener.postMessage({ paymentStatus: 'failed' }, '*');
        //            window.close();
        //          } catch(e) { console.error(e); }
        //        </script>
        //        <h2>Payment Failed — try again.</h2>
        //        </body></html>", "text/html");
        //    }

        // ✅ Step 3: Frontend Polls Order Status
        //[HttpGet("status/{orderId}")]
        //public async Task<IActionResult> GetOrderStatus(int orderId)
        //{
        //    var order = await _easebuzz.GetOrderByIdAsync(orderId);
        //    if (order == null)
        //        return NotFound(new { Status = 0, Msg = "Order not found" });

        //    return Ok(new { Status = 1, Data = order });
        //}

        //[HttpPost("success")]
        //public IActionResult SuccessRedirect([FromForm] IFormCollection formData)
        //{
        //    // Log or process successful payment data
        //    var txnid = formData["txnid"];
        //    var status = formData["status"];
        //    var amount = formData["amount"];

        //    // You can save the details in DB or send to frontend
        //    return Ok(new
        //    {
        //        Message = "Payment Successful",
        //        TransactionId = txnid,
        //        Amount = amount,
        //        Status = status
        //    });
        //}

        //[HttpPost("failure")]
        //public IActionResult FailureRedirect([FromForm] IFormCollection formData)
        //{
        //    // Log or process failed payment data
        //    var txnid = formData["txnid"];
        //    var status = formData["status"];

        //    return Ok(new
        //    {
        //        Message = "Payment Failed",
        //        TransactionId = txnid,
        //        Status = status
        //    });
        //}
    }
}
