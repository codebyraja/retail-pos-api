using Master.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Pos.Services.Repository;
using QSRAPIServices.Models;
using Razorpay.Models;
using RetailPosRepository.Services.Repository;
using RetailPosToken.Services.Token;
using System.Text.Json;

namespace RetailPosController.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[action]")]
    [ApiController]
    public class RetailsPosController : ControllerBase
    {
        public readonly IRepository _services;
        public readonly IPosRepository _posServices;

        public readonly IPaymentService _paymentService;
        public readonly ITokenService _tokenService;

        public RetailsPosController(IRepository services, IPosRepository posServices, ITokenService tokenService, IPaymentService paymentService)
        {
            this._services = services;
            this._posServices = posServices;
            this._tokenService = tokenService;
            _paymentService = paymentService;
        }

        [HttpPost("sync-locations")]
        public async Task<IActionResult> SyncLocations()
        {
            try
            {
                await _services.SyncLocationsAsync();
                return Ok(new { Status = 1, Msg = "Location sync completed" });
            }
            catch (Exception ex)
            {
                return Ok(new { Status = 0, Msg = ex.Message });
            }
        }

        [HttpGet("catalog")]
        public async Task<IActionResult> CatalogAsync()
        {
            return Ok(await _posServices.GetCatalogAsync());
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

        [HttpPost]
        public async Task<IActionResult> SaveMaster([FromForm] Master1 master, [FromForm] string? variants, [FromForm] string? customFields, [FromForm] string? images, [FromForm] List<IFormFile>? files)
        {
            //var variantList = JsonSerializer.Deserialize<List<VariantDto>>(variants);
            //var customFieldObj = JsonSerializer.Deserialize<CustomFieldsDto>(customFields);
            //var imageList = JsonSerializer.Deserialize<List<string>>(images);

            var result = await _services.SaveMasterAsync(master, files, variants, customFields, images);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMaster(int tranType, int masterType, int code, string? name)
        {
            //if (tranType == 0) return BadRequest(new { Status = 0, Msg = "Transaction type is required." });
            if (masterType == 0) return BadRequest(new { Status = 0, Msg = "Master type is required." });

            if (masterType == 6 && code != 0)
                return Ok(await _services.GetProductByIdAsync(code));
            else
                return Ok(await _services.GetMasterAsync(tranType, masterType, code, name));
        }

        [HttpGet]
        public async Task<IActionResult> Master(int masterType)
        {
            return Ok(await _services.GetMasterListAsync(masterType));
        }

        [HttpPost]
        public async Task<IActionResult> SaveItem(ItemSaveDto model)
        {
            //var itemResult = await _services.SaveMasterAsync(model.Item);

            //if (itemResult.Status != 1)
            //    return Ok(itemResult);

            //int itemCode = itemResult.Code;

            //if (model.Variants != null && model.Variants.Count > 0)
            //{
            //    foreach (var v in model.Variants)
            //    {
            //        await _services.SaveVariantAsync(itemCode, v);
            //    }
            //}

            return Ok(new { Status = 1, Msg = "Item Saved Successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAddon(Addon addon)
        {
            return Ok(await _services.SaveAddonAsync(addon));
        }

        [HttpGet]
        public async Task<IActionResult> GetAddons(int itemCode = 0)
        {
            var result = await _services.GetAddonsAsync(itemCode);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTable([FromBody] RestaurantTable table)
        {
            var result = await _services.SaveTableAsync(table);
            return Ok(result);
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
