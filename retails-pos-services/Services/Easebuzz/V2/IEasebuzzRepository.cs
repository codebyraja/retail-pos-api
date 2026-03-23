using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Easebuzz.Models;

namespace EasebuzzPayment.Services.V2
{
    public interface IEasebuzzRepositoryV2
    {
        //CreatePaymentLinkAsync
        Task<EasebuzzPaymentResponse> InitiatePaymentAsync(EasebuzzPaymentRequest request);
    }
}
