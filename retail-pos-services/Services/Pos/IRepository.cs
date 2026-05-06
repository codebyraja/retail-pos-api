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

namespace Pos.Services.Repository
{
    public interface IPosRepository
    {
        Task<dynamic> GetCatalogAsync();
    }
}
