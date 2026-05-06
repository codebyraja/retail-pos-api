using Master.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Pos.Models;
using QSRAPIServices.Models;
using QSRHelperApiServices.Helper;
using Razorpay.Models;
using RetailPosContext.DBContext;
using RetailPosEmail.Services.Email;
using System.Buffers.Text;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Text.Json;

namespace Pos.Services.Repository
{
    public class PosRepository : IPosRepository
    {
        private readonly RetailPosDBContext _db;
        private readonly EmailHelper _emailHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceScopeFactory _scopeFactory;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public PosRepository(RetailPosDBContext db, EmailHelper emailHelper, IHttpContextAccessor httpContextAccessor, IServiceScopeFactory scopeFactory)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _db = db;
            _emailHelper = emailHelper;
            _httpContextAccessor = httpContextAccessor;
            _scopeFactory = scopeFactory;
        }
        public async Task<dynamic> GetCatalogAsync()
        {
            try
            {
                var request = _httpContextAccessor?.HttpContext?.Request;
                var baseUrl = Helper.GetBaseUrl(request);

                const string sql = @"SELECT C.Code AS CategoryId, C.Name AS CategoryName, ISNULL(C.Image, '') AS CategoryImage, ISNULL(P.Code, 0) AS ProductId, ISNULL(P.Name, '') AS ProductName, ISNULL(P.Image, '') AS ProductImage, ISNULL(P.D1, 0) AS Price, ISNULL(P.D2, 0) AS Stock FROM Master1 C LEFT JOIN Master1 P ON P.ParentGrp = C.Code AND P.MasterType = 6 WHERE C.MasterType = 5 ORDER BY C.Code DESC";

                var rawData = await _db.Set<TempCatalog>().FromSqlRaw(sql).AsNoTracking().ToListAsync();

                // 🔹 No Data
                if (rawData == null || rawData.Count == 0)
                {
                    return new
                    {
                        Status = 0,
                        Msg = "No data found",
                        Data = new List<object>()
                    };
                }

                // 🔥 Grouping + Optimization
                var result = rawData.GroupBy(x => new { x.CategoryId, x.CategoryName, x.CategoryImage })

                    // ❗ Only categories having products
                    //.Where(g => g.Any(p => p.ProductId > 0))

                    .Select(g => new
                    {
                        Code = g.Key.CategoryId,
                        Name = g.Key.CategoryName ?? string.Empty,
                        Image = string.IsNullOrEmpty(g.Key.CategoryImage) ? "": baseUrl + g.Key.CategoryImage,

                        Products = g
                            .Where(p => p.ProductId > 0)

                            // ❗ Remove duplicate products
                            .GroupBy(p => p.ProductId)
                            .Select(p => p.First())

                            // ❗ Sorting for better UX
                            .OrderBy(p => p.ProductName)

                            .Select(p => new
                            {
                                Code = p.ProductId,
                                Name = p.ProductName ?? string.Empty,
                                Image = string.IsNullOrEmpty(p.ProductImage) ? "" : baseUrl + p.ProductImage,
                                Price = p.Price,
                                Stock = p.Stock
                            })
                            .ToList()
                    })
                    .ToList();

                // 🔹 Final Response
                return new { Status = 1, Msg = "Catalog fetched successfully", Data = result };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message, Data = (object)null };
            }
        }
    }
}
