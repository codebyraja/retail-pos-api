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

namespace General.Services.Repository
{
    public class GeneralRepository : IGeneralRepository
    {
        private readonly RetailPosDBContext _db;
        private readonly EmailHelper _emailHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceScopeFactory _scopeFactory;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public GeneralRepository(RetailPosDBContext db, EmailHelper emailHelper, IHttpContextAccessor httpContextAccessor, IServiceScopeFactory scopeFactory)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _db = db;
            _emailHelper = emailHelper;
            _httpContextAccessor = httpContextAccessor;
            _scopeFactory = scopeFactory;
        }
        public async Task<dynamic> GetVchNoAsync(int tranType, int vchType)
        {
            try
            {
                if (tranType == 0) tranType = vchType == 9 ? 2 : 1;
                int autoVchNo = GetAutoVchNo(vchType);
                var autoNo = Convert.ToInt32(autoVchNo);
                string prefix, suffix, padStr;
                LoadSuffixPrefix(tranType, out prefix, out suffix, out padStr, ref autoVchNo);

                return new { Status = 1, Msg = "Voucher No has been generated !!!", Data = new[] { new { vchNo = $"{prefix}{padStr}{autoVchNo}{suffix}", refNo = $"{"R/"}{prefix}{padStr}{autoVchNo}{suffix}", orderId = autoVchNo } } };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message };
            }
        }
        private int GetAutoVchNo(int vchType)
        {
            string sql = $"Select IsNull(Max(AutoVchNo),0) as AutoNo From RJTran1 Where VchType = {vchType}";
            var DT1 = _db.AutoVchNos.FromSqlRaw(sql).ToList();

            if (DT1 != null && DT1.Count > 0)
            {
                return Convert.ToInt32(DT1[0].AutoNo) + 1;
            }
            return 1;
        }
        private void LoadSuffixPrefix(int tranType, out string prefix, out string suffix, out string padStr, ref int vchNo)
        {
            prefix = string.Empty;
            suffix = string.Empty;
            padStr = string.Empty;

            string sql = $"SELECT * FROM RTVchConfig WHERE TranType = {tranType}";
            var DT1 = _db.VchConfigs.FromSqlRaw(sql).ToList();

            if (DT1.Count > 0)
            {
                prefix = DT1[0].Prefix?.ToString().Trim() ?? string.Empty;
                suffix = DT1[0].Suffix?.ToString().Trim() ?? string.Empty;

                if (DT1[0].StartNo.HasValue && DT1[0].StartNo.Value > vchNo)
                {
                    vchNo = DT1[0].StartNo.Value;
                }

                string vchStr = vchNo.ToString();
                if (DT1[0].Padding.HasValue && DT1[0].Padding.Value != 0)
                {
                    int padLength = DT1[0].PaddLength ?? 0;
                    char padChar = !string.IsNullOrEmpty(DT1[0].PaddChar) ? DT1[0].PaddChar[0] : ' ';

                    if (padLength > vchStr.Length)
                    {
                        padStr = new string(padChar, padLength - vchStr.Length);
                    }
                }
            }
        }
    }
}
