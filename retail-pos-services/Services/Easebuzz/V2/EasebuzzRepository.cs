using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Easebuzz.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EasebuzzPayment.Services.V2
{
    public class EasebuzzRepositoryV2 : IEasebuzzRepositoryV2
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EasebuzzRepositoryV2> _logger;

        public EasebuzzRepositoryV2(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<EasebuzzRepositoryV2> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<EasebuzzPaymentResponse> InitiatePaymentAsync(EasebuzzPaymentRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            var accessKey = _configuration["Easebuzz:AccessKey"];
            var redirectUrl = _configuration["Easebuzz:RedirectUrl"];
            var webhookUrl = _configuration["Easebuzz:WebhookUrl"];

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessKey);

            var payload = new
            {
                amount = request.Amount,
                buyer_name = request.BuyerName,
                email = request.Email,
                phone = request.Phone,
                purpose = request.Purpose,
                redirect_url = redirectUrl,
                webhook_url = webhookUrl
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://pay.easebuzz.in/payment/initiateLink", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var jsonDoc = JsonDocument.Parse(responseBody);
                    var paymentLink = jsonDoc.RootElement.GetProperty("data").GetProperty("payment_link").GetString();

                    return new EasebuzzPaymentResponse
                    {
                        Success = true,
                        PaymentLink = paymentLink
                    };
                }

                _logger.LogError("Easebuzz API Error: {Response}", responseBody);
                return new EasebuzzPaymentResponse
                {
                    Success = false,
                    ErrorMessage = responseBody
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Easebuzz API");
                return new EasebuzzPaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Internal server error"
                };
            }
        }
    }   
}
