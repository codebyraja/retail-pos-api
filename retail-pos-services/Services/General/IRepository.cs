using Master.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QSRAPIServices.Models;
using Razorpay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.Services.Repository
{
    public interface IGeneralRepository
    {
        Task<dynamic> GetVchNoAsync(int tranType, int vchType);
    }
}
