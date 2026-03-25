using Master.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using QSRAPIServices.Models;
using QSRHelperApiServices.Helper;
using Razorpay.Models;
using RetailPosContext.DBContext;
using RetailPosEmail.Services.Email;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RetailPosRepository.Services.Repository
{
    public class Repository : IRepository
    {
        private readonly RetailPosDBContext _db;
        private readonly EmailHelper _emailHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceScopeFactory _scopeFactory;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Repository(RetailPosDBContext db, EmailHelper emailHelper, IHttpContextAccessor httpContextAccessor, IServiceScopeFactory scopeFactory)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _db = db;
            _emailHelper = emailHelper;
            _httpContextAccessor = httpContextAccessor;
            _scopeFactory = scopeFactory;
        }

        public async Task<dynamic> ValidateUserAsync(string username, string password)
        {
            dynamic response = new ExpandoObject();
            try
            {
                //string sql = "Select [Id], [Name], [Email], [Username], [Password], '' as [OtpCode], CAST(IsOtpVerified AS BIT) AS IsOtpVerified, [OtpExpiry] as OtpExpiry From Users Where [Username] = '" + UName.Replace("'", "''").Trim() + "' And Password = '" + PWD.Replace("'", "''").Trim() + "' ";
                string sql = $"Select [Code] as Id, [Name], [Email], [Username], [Password], '' as [OtpCode] ,CAST(0 AS BIT) AS IsOtpVerified, NULL AS OtpExpiry From RJUserMaster Where ([Username] = @username OR Email = @username) And Password = @password";
                var user = await _db.Users.FromSqlRaw(sql, new SqlParameter("@username", username.Replace("'", "''").Trim()), new SqlParameter("@password", password.Replace("'", "''").Trim())).FirstOrDefaultAsync();

                if (user == null)
                {
                    return new LoginResult
                    {
                        Status = 0,
                        Msg = "Invalid username or password",
                        Data = null
                    };
                }

                // ✅ Check hashed password
                //if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                //    return response;

                var userModel = new UserModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Username = user.Username,
                    IsOtpVerified = user.IsOtpVerified,
                    OtpCode = user.OtpCode,
                    OtpExpiry = user.OtpExpiry
                };

                return new LoginResult
                {
                    Status = 1,
                    Msg = "Login successful",
                    Data = userModel
                };

            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    Status = 0,
                    Msg = ex.Message,
                    Data = null
                };
            }
        }

        //public async Task<ApiResponse> ValidateUser(string UName, string PWD)
        //{
        //    var response = new ApiResponse { Status = 0, Msg = "User Not Found", Data = null };

        //    try
        //    {
        //        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == UName);
        //        if (user == null || !BCrypt.Net.BCrypt.Verify(PWD, user.Password))
        //        {
        //            response.Msg = "Invalid username or password";
        //            return response;
        //        }

        //        var USData = new UserModel
        //        {
        //            Id = user.Id,
        //            Name = user.Name,
        //            Email = user.Email,
        //            Username = user.Username,
        //            Password = user.Password,
        //            OtpCode = user.OtpCode,
        //            IsOtpVerified = user.IsOtpVerified,
        //            OtpExpiry = user.OtpExpiry
        //        };

        //        response.Status = 1;
        //        response.Msg = "Login Successfully Done";
        //        response.Data = USData;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Status = 0;
        //        response.Msg = ex.Message;
        //    }

        //    return response;
        //}
        public async Task SaveRefreshToken(int userid, string token, DateTime expiry)
        {
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = userid,
                ExpiryDate = expiry,
                IsRevoked = false
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();
        }

        //public async Task SaveOrUpdateRefreshToken(int userid, string token, DateTime expiry)
        //{
        //    var existing = await _db.RefreshTokens
        //        .FirstOrDefaultAsync(t => t.UserId == userid);

        //    if (existing != null)
        //    {
        //        existing.Token = token;
        //        existing.ExpiryDate = expiry;
        //        existing.IsRevoked = false;
        //        _db.RefreshTokens.Update(existing);
        //    }
        //    else
        //    {
        //        var newToken = new RefreshToken
        //        {
        //            UserId = userid,
        //            Token = token,
        //            ExpiryDate = expiry
        //        };
        //        _db.RefreshTokens.Add(newToken);
        //    }

        //    await _db.SaveChangesAsync();
        //}

        public async Task<bool> RevokeRefreshToken(string refreshToken)
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (token == null || token.IsRevoked)
                return false;

            token.IsRevoked = true;
            _db.RefreshTokens.Update(token);
            await _db.SaveChangesAsync();

            return true;
        }
        public async Task RevokeAllTokensForUser(int userid)
        {
            var tokens = await _db.RefreshTokens.Where(t => t.UserId == userid && !t.IsRevoked).ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            _db.RefreshTokens.UpdateRange(tokens);
            await _db.SaveChangesAsync();
        }
        public async Task<UserModel> GetUserByRefreshToken(string token)
        {
            var refresh = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token && x.ExpiryDate > DateTime.UtcNow);
            if (refresh == null || refresh.IsRevoked) return null;

            return await _db.Users.FirstOrDefaultAsync(u => u.Id == refresh.UserId);
        }
        public async Task<string> UpdateRefreshToken(string oldToken, string newToken, DateTime expiry)
        {
            var old = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == oldToken);
            if (old == null) return null;

            old.Token = newToken;
            old.ExpiryDate = expiry;

            _db.RefreshTokens.Update(old);
            await _db.SaveChangesAsync();

            return newToken;
        }
        public async Task<Response> SendOtpToEmailAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new Response { Status = 0, Msg = "User not found." };
            }

            string otp = new Random().Next(100000, 999999).ToString();

            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _db.SaveChangesAsync();

            var messageRepo = new EmailTemplates();
            string body = messageRepo.GetMessage("OTP_Reset", otp); // loads /EmailTemplates/OTP_Reset.html

            await _emailHelper.SendEmailAsync(user.Email, "OTP for Password Reset", body);

            return new Response { Status = 1, Msg = "OTP sent successfully!" };
        }
        public async Task<Response> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return new Response { Status = 0, Msg = "User not found." };

            if (user.OtpCode != otp || user.OtpExpiry < DateTime.UtcNow)
                return new Response { Status = 0, Msg = "Invalid or expired OTP." };

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.OtpCode = null;
            user.OtpExpiry = null;
            user.IsOtpVerified = true;

            await _db.SaveChangesAsync();

            string body = $"<p>Your password has been successfully changed on {DateTime.UtcNow.ToString("dd MMM yyyy HH:mm:ss")} UTC.</p>";
            await _emailHelper.SendEmailAsync(user.Email, "Password Changed", body);

            return new Response { Status = 1, Msg = "Password reset successful." };
        }
        public async Task<dynamic> Signup(SignupRequest user)
        {
            int Status = 0; string Msg = string.Empty;
            try
            {
                // Hash the password before saving
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                SqlParameter param1 = new SqlParameter("@p1", user.Name);
                SqlParameter param2 = new SqlParameter("@p2", user.Email);
                SqlParameter param3 = new SqlParameter("@p3", user.Username);
                SqlParameter param4 = new SqlParameter("@p4", hashedPassword);

                var DT1 = await _db.Responses.FromSqlRaw("EXEC [sp_SaveSignup] @p1, @p2, @p3, @p4", param1, param2, param3, param4).ToListAsync();

                Status = DT1[0].Status;
                Msg = DT1[0].Msg;
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString() };
            }
            return new { Status, Msg };
        }


        public async Task<Response> SaveMasterAsync(Master1 master, IFormFile image)
        {
            try
            {
                string folders = master.MasterType == 4 ? "subcategory" 
                    : master.MasterType == 5 ? "categories" 
                    : master.MasterType == 6 ? "product" 
                    : master.MasterType == 7 ? "brand" 
                    : master.MasterType == 8 ? "unit" : "";

                // 🔥 Image save
                if (image != null)
                    master.Image = await FileUploadHelper.SaveFileAsync(image, Directory.GetCurrentDirectory(), folders, master.Name);
                else
                    master.Image = "";
                

                string users = master.Code == 0 ? master.CreatedBy ?? "SYSTEM" : master.ModifiedBy ?? "SYSTEM";

                SqlParameter param0 = new SqlParameter("@p0", master.Code);
                SqlParameter param1 = new SqlParameter("@p1", master.MasterType);
                SqlParameter param2 = new SqlParameter("@p2", master.Name);
                SqlParameter param3 = new SqlParameter("@p3", master.Alias);
                SqlParameter param4 = new SqlParameter("@p4", master.PrintName);
                SqlParameter param5 = new SqlParameter("@p5", master.ParentGrp);
                SqlParameter param6 = new SqlParameter("@p6", master.HSNCode);
                SqlParameter param7 = new SqlParameter("@p7", master.CM1);
                SqlParameter param8 = new SqlParameter("@p8", master.CM2);
                SqlParameter param9 = new SqlParameter("@p9", master.CM3);
                SqlParameter param10 = new SqlParameter("@p10", master.CM4);
                SqlParameter param11 = new SqlParameter("@p11", master.CM5);
                SqlParameter param12 = new SqlParameter("@p12", master.D1);
                SqlParameter param13 = new SqlParameter("@p13", master.D2);
                SqlParameter param14 = new SqlParameter("@p14", master.D3);
                SqlParameter param15 = new SqlParameter("@p15", master.D4);
                SqlParameter param16 = new SqlParameter("@p16", master.D5);
                SqlParameter param17 = new SqlParameter("@p17", master.C1);
                SqlParameter param18 = new SqlParameter("@p18", master.C2);
                SqlParameter param19 = new SqlParameter("@p19", master.C3);
                SqlParameter param20 = new SqlParameter("@p20", master.C4);
                SqlParameter param21 = new SqlParameter("@p21", master.C5);
                SqlParameter param22 = new SqlParameter("@p22", master.Remark);
                SqlParameter param23 = new SqlParameter("@p23", master.Blocked);
                SqlParameter param24 = new SqlParameter("@p24", master.Status ? 0 : 1);
                SqlParameter param25 = new SqlParameter("@p25", string.IsNullOrEmpty(master.Image) ? "" : master.Image);
                SqlParameter param26 = new SqlParameter("@p26", users);

                var result = await _db.Responses.FromSqlRaw("EXEC dbo.[sp_SaveMaster] @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18, @p19, @p20, @p21, @p22, @p23, @p24, @p25, @p26", param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21, param22, param23, param24, param25, param26).ToListAsync();

                //.ContinueWith(t =>
                //{
                //    if (t.IsFaulted)
                //    {
                //        return new { Status = 0, Msg = t.Exception?.GetBaseException().Message ?? "An error occurred." };
                //    }
                //    else
                //    {
                //        var res = t.Result.FirstOrDefault();
                //        return new { Status = res?.Status ?? 0, Msg = res?.Msg ?? "No response from database." };
                //    }
                //});
                //return result;


                //90 rupres, 40, 4000, 20K, 

                var res = result.First();

                int Status = result[0].Status;
                string Msg = result[0].Msg;
                int Code = result[0].Code;

                if (result == null || result.Count == 0)
                {
                    return new Response { Status = 0, Msg = "No response from database." };
                }
                return new Response { Status = Status, Msg = Msg, Code = Code };

            }
            catch (Exception ex)
            {
                return new Response { Status = 0, Msg = ex.Message.ToString() };
            }
        }

        public async Task SaveVariantAsync(int itemCode, ItemVariantDto variant)
        {
            SqlParameter p1 = new SqlParameter("@ItemCode", itemCode);
            SqlParameter p2 = new SqlParameter("@VariantName", variant.VariantName);
            SqlParameter p3 = new SqlParameter("@Price", variant.Price);
            SqlParameter p4 = new SqlParameter("@IsDefault", variant.IsDefault);

            await _db.Database.ExecuteSqlRawAsync("EXEC sp_SaveItemVariant @ItemCode,@VariantName,@Price,@IsDefault", p1, p2, p3, p4);
        }

        public async Task<dynamic> GetMasterAsync(int tranType, int masterType, int code, string? name)
        {
            try
            {
                var request = _httpContextAccessor?.HttpContext?.Request;
                var baseUrl = Helper.GetBaseUrl(request);

                string sql = "Select ISNULL(A.[Code], 0) as Code, ISNULL(A.[MasterType], 0) as MasterType, ISNULL(A.[Name], '') as Name, ISNULL(A.[Alias], '') as Alias, ISNULL(A.[PrintName], '') as PrintName, ISNULL(A.[ParentGrp], 0) as ParentGrpCode, ISNULL(M1.[Name], '') as ParentGrpName, ISNULL(A.[HSNCode], '') as HSNCode, ISNULL(A.[CM1], 0) as CM1, ISNULL(A.[CM2], 0) as CM2, ISNULL(A.[CM3], 0) as CM3, ISNULL(A.[CM4], 0) as CM4, ISNULL(A.[CM5], 0) as CM5, ISNULL(A.[D1], 0) as D1, ISNULL(A.[D2], 0) as D2, ISNULL(A.[D3], 0) as D3, ISNULL(A.[D4], 0) as D4, ISNULL(A.[D5], 0) as D5, ISNULL(STRING_AGG(V.[Value], ','), '') as [Values], ISNULL(A.[Remark], '') as Remark, 10 as NoOfProducts, ISNULL(A.[BlockedMaster], 0) as Blocked, ISNULL(A.[DeactiveMaster], 0) as Deactive, ISNULL(A.[Image], '') as Image, ISNULL(A.[CreatedBy], '') as CreatedBy, ISNULL(A.[CreationTime], '') as CreatedOn, ISNULL(A.[ModifiedBy], '') ModifiedBy, ISNULL(A.[ModificationTime], '') as ModifiedOn from Master1 A LEFT JOIN Master1 M1 ON A.ParentGrp = M1.Code LEFT JOIN VariantValues V ON V.VariantId = A.Code Where A.[MasterType] = @masterType GROUP BY A.Code, A.MasterType, A.Name, A.Alias, A.PrintName, A.ParentGrp, M1.Name, A.HSNCode, A.[CM1], A.[CM2], A.[CM3], A.[CM4], A.[CM5], A.[D1], A.[D2], A.[D3], A.[D4],A.[D5], A.Remark, A.BlockedMaster, A.DeactiveMaster, A.Image, A.CreatedBy, A.CreationTime, A.ModifiedBy, A.ModificationTime Order By A.[Name]";
                var DT1 = await _db.Masters2.FromSqlRaw(sql, new SqlParameter("@masterType", masterType)).ToListAsync();
                //where MasterType = @masterType and(@code = 0 or Code = @code) and(@name is null or Name like '%' + @name + '%')
                //var DT1 = await _db.Masters.FromSqlRaw(sql, new SqlParameter("@masterType", masterType), new SqlParameter("@code", code), new SqlParameter("@name", name ?? (object)DBNull.Value)).ToListAsync();

                if (DT1 == null || DT1.Count == 0)
                {
                    return new { Status = 0, Msg = "No masters found." };
                }

                foreach (var item in DT1)
                {
                    if (!string.IsNullOrEmpty(item.Image))
                    {
                        item.Image = baseUrl + item.Image;
                    }
                    else
                    {
                        item.Image = "https://tse1.mm.bing.net/th/id/OIP.1bN_NV0O39xEAR7gjO8MgwHaIM?rs=1&pid=ImgDetMain&o=7&rm=3";
                    }
                }

                return new { Status = 1, Msg = "Masters retrieved successfully.", Data = DT1 };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString() };
            }
        }

        public async Task<Response> SaveAddonAsync(Addon addon)
        {
            try
            {

                string users = addon.Code == 0 ? addon.CreatedBy ?? "SYSTEM" : addon.ModifiedBy ?? "SYSTEM";

                SqlParameter param0 = new SqlParameter("@p0", addon.Code);
                SqlParameter param1 = new SqlParameter("@p1", addon.Name);
                SqlParameter param2 = new SqlParameter("@p2", addon.Price);
                SqlParameter param3 = new SqlParameter("@p3", addon.Description ?? "");
                SqlParameter param4 = new SqlParameter("@p4", addon.Image ?? "");
                SqlParameter param5 = new SqlParameter("@p5", addon.IsActive);
                SqlParameter param6 = new SqlParameter("@p6", users);

                var result = await _db.Responses.FromSqlRaw("EXEC dbo.sp_SaveAddon @p0, @p1, @p2, @p3, @p4, @p5, @p6", param0, param1, param2, param3, param4, param5, param6).ToListAsync();

                if (result == null || result.Count == 0)
                {
                    return new Response { Status = 0, Msg = "No response from database." };
                }

                var addonResult = result.First();

                // अगर addon save fail हुआ
                if (addonResult.Status == 0)
                {
                    return addonResult;
                }

                int addonCode = addonResult.Code;

                if (addon.ItemCode > 0)
                {

                    // Save ItemAddonMapping
                    SqlParameter map1 = new SqlParameter("@p0", addon.ItemCode);
                    SqlParameter map2 = new SqlParameter("@p1", addonCode);

                    var mapResult = await _db.Responses.FromSqlRaw("EXEC dbo.sp_SaveItemAddonMapping @p0,@p1", map1, map2).ToListAsync();

                    if (mapResult != null && mapResult.Count > 0)
                    {
                        if (mapResult[0].Status == 0)
                            return mapResult[0];
                    }

                }
                string finalMsg = addonResult?.Msg;

                if (addonResult?.Msg == "Addon already exists." && addon.ItemCode > 0)
                    finalMsg = "Addon already exists and mapped successfully.";

                return new Response { Status = addonResult.Status, Msg = finalMsg, Code = addonCode };

            }
            catch (Exception ex)
            {
                return new Response { Status = 0, Msg = ex.Message };
            }
        }

        public async Task<dynamic> GetAddonsAsync(int itemCode)
        {
            try
            {
                string sql = "SELECT IA.Code, IA.ItemCode, M.Name AS ItemName, A.Code AS AddonCode, A.Name AS AddonName, A.Price, A.Description, A.Image, A.IsActive FROM ItemAddon IA INNER JOIN Addons A ON A.Code = IA.AddonCode INNER JOIN Master1 M ON M.Code = IA.ItemCode WHERE 1=1";

                if (itemCode > 0)
                {
                    sql += " AND IA.ItemCode = @ItemCode";
                }
                var DT1 = await _db.AddonLists.FromSqlRaw(sql).ToListAsync();

                if (DT1 == null || DT1.Count == 0)
                {
                    return new { Status = 0, Msg = "No addons found." };
                }

                return new { Status = 1, Msg = "Addons retrieved successfully.", Data = DT1 };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString() };
            }
        }

        public async Task<Response> SaveTableAsync(RestaurantTable table)
        {
            try
            {
                string users = table.Code == 0 ? table.CreatedBy ?? "SYSTEM" : table.ModifiedBy ?? "SYSTEM";

                SqlParameter param0 = new SqlParameter("@p0", table.Code);
                SqlParameter param1 = new SqlParameter("@p1", table.Name ?? "");
                SqlParameter param2 = new SqlParameter("@p2", table.Floor ?? "");
                SqlParameter param3 = new SqlParameter("@p3", table.TableSize);
                SqlParameter param4 = new SqlParameter("@p4", table.NoOfGuests);
                SqlParameter param5 = new SqlParameter("@p5", table.Status);
                SqlParameter param6 = new SqlParameter("@p6", table.Remark ?? "");
                SqlParameter param7 = new SqlParameter("@p7", users);

                var result = await _db.Responses.FromSqlRaw("EXEC dbo.sp_SaveTable @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7", param0, param1, param2, param3, param4, param5, param6, param7).ToListAsync();

                if (result == null || result.Count == 0)
                {
                    return new Response { Status = 0, Msg = "No response from database." };
                }

                return new Response { Status = result[0].Status, Msg = result[0].Msg, Code = result[0].Code };
            }
            catch (Exception ex)
            {
                return new Response { Status = 0, Msg = ex.Message };
            }
        }

        public async Task<dynamic> SaveProductMasterDetails(SaveProductMasterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return new { Status = 0, Msg = "Product name is required." };
            if (request.ProductType == 0)
                return new { Status = 0, Msg = "Product type is required." };
            int Status = 0; string Msg = string.Empty; int productId = 0;
            var helper = new Helper(); // Create an instance of Helper

            try
            {
                SqlParameter param0 = new SqlParameter("@p0", request.Code);
                SqlParameter param1 = new SqlParameter("@p1", request.Name);
                SqlParameter param2 = new SqlParameter("@p2", request.PrintName ?? string.Empty);
                SqlParameter param3 = new SqlParameter("@p3", request.ParentGrp);
                SqlParameter param4 = new SqlParameter("@p4", request.Slug ?? string.Empty);
                SqlParameter param5 = new SqlParameter("@p5", request.Sku ?? string.Empty);
                SqlParameter param6 = new SqlParameter("@p6", request.Unit);
                SqlParameter param7 = new SqlParameter("@p7", request.Description ?? string.Empty);
                SqlParameter param8 = new SqlParameter("@p8", request.ProductType);
                SqlParameter param9 = new SqlParameter("@p9", request.ProductTypeName ?? string.Empty);
                SqlParameter param10 = new SqlParameter("@p10", request.Qty);
                SqlParameter param11 = new SqlParameter("@p11", request.MinQty);
                SqlParameter param12 = new SqlParameter("@p12", request.Price);
                SqlParameter param13 = new SqlParameter("@p13", request.Discount);
                SqlParameter param14 = new SqlParameter("@p14", request.TaxType);
                SqlParameter param15 = new SqlParameter("@p15", request.TaxTypeName ?? string.Empty);
                SqlParameter param16 = new SqlParameter("@p16", request.DiscountType);
                SqlParameter param17 = new SqlParameter("@p17", request.DiscountTypeName ?? string.Empty);
                SqlParameter param18 = new SqlParameter("@p18", request.DeactiveMaster);
                SqlParameter param19 = new SqlParameter("@p19", request.MasterType);
                SqlParameter param20 = new SqlParameter("@p20", "Product");
                SqlParameter param21 = new SqlParameter("@p21", request.Users ?? "Admin");

                var DT1 = await _db.ResponseNew.FromSqlRaw("EXEC dbo.[sp_SaveProductMasterDet] @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18, @p19, @p20, @p21", param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20, param21).ToListAsync();

                Status = DT1[0].Status; Msg = DT1[0].Msg; productId = DT1[0].Code;

                if (productId > 0)
                {
                    // ✅ Check if barcode already exists
                    string existingBarcode = await GetExistingBarcodeAsync(productId);

                    if (string.IsNullOrEmpty(existingBarcode))
                    {
                        // ✅ Generate new barcode only if not already exists
                        string barcode = helper.GenerateUniqueBarcode("products", productId);

                        // ✅ Save barcode in database
                        await SaveEntityBarcodeAsync("RJMaster1", "Code", productId, barcode);
                    }

                    // ✅ Check if product saved successfully
                    if (Status == 1 && request.Images != null && request.Images.Any())
                    {
                        // ✅ Upload images and save metadata
                        var uploadResult = await UploadProductImagesAsync(productId, request?.Images, request?.Name, "products");

                        if (!uploadResult.IsSuccess)
                        {
                            // Optionally log or handle image upload failure
                            Msg += " | " + uploadResult.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString() };
            }
            return new { Status = Status, Msg = Msg };
        }
        private async Task<string> GetExistingBarcodeAsync(int productId)
        {
            var param = new SqlParameter("@ProductId", productId);

            var result = await _db.Database.SqlQueryRaw<string>("SELECT TOP 1 Barcode FROM RJMaster1 WHERE Code = @ProductId", param).ToListAsync();

            return result.FirstOrDefault();
        }
        private async Task SaveEntityBarcodeAsync(string tableName, string keyColumn, int entityId, string barcode)
        {
            string sql = $"UPDATE {tableName} SET Barcode = @Barcode WHERE {keyColumn} = @Id";

            var param1 = new SqlParameter("@Barcode", barcode);
            var param2 = new SqlParameter("@Id", entityId);

            await _db.Database.ExecuteSqlRawAsync(sql, param1, param2);
        }
        public async Task<ImageUploadResult> UploadProductImagesAsync(int productId, List<IFormFile> images, string name, string path)
        {
            var uploadedPaths = new List<string>(); int srno = 0;
            string uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", path);

            try
            {
                // Ensure upload directory exists
                if (!Directory.Exists(uploadRoot))
                    Directory.CreateDirectory(uploadRoot);

                var helper = new Helper(); // Create an instance of Helper

                // Delete existing images for the product
                bool deleted = await DeleteProductImagesAsync(productId); 

                if (!deleted)
                {
                    return new ImageUploadResult { IsSuccess = false, Message = $"Unable to remove images for Product ID: {productId}. Please try again.", UploadedPaths = new List<string>() };
                };

                foreach (var image in images)
                {
                    if (images.Count <= 0)
                        continue;

                    srno++; // Increment image number

                    // Get the file extension
                    string extension = Path.GetExtension(image.FileName); // Example: ".jpg", ".png"

                    // Generate new file name based on product name and serial number
                    string sanitizedProductName = helper.SanitizeFileName(name); // Optional: helper function to remove special characters
                    string fileName = $"{sanitizedProductName}-{srno}{extension}";

                    string filePath = Path.Combine(uploadRoot, fileName);
                    string relativePath = $"/uploads/{path}/{fileName}";

                    // Save image to disk
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // Save metadata to DB
                    await SaveProductImageMetadataAsync(productId, name, fileName, relativePath, image.Length, image.ContentType, srno);

                    uploadedPaths.Add(relativePath);
                }


                if (!uploadedPaths.Any())
                {
                    return new ImageUploadResult
                    {
                        IsSuccess = false,
                        Message = "No valid images uploaded.",
                        UploadedPaths = new List<string>()
                    };
                }

                return new ImageUploadResult
                {
                    IsSuccess = true,
                    Message = "Images uploaded successfully.",
                    UploadedPaths = uploadedPaths
                };
            }
            catch (Exception ex)
            {
                return new ImageUploadResult
                {
                    IsSuccess = false,
                    Message = $"Image upload failed: {ex.Message}",
                    UploadedPaths = new List<string>()
                };
            }
        }
        public async Task<bool> DeleteProductImagesAsync(int productId)
        {
            try
            {
                // Step 1: Get all image records for the product
                var images = await _db.ProductImages.Where(p => p.ProductId == productId).ToListAsync();

                if (images.Count > 0)
                {
                    // Step 2: Delete each physical image file
                    foreach (var img in images)
                    {
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", img.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }

                    // Step 3: Remove image records from DB
                    _db.ProductImages.RemoveRange(images);
                    await _db.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                // You can log the exception here if needed
                Console.WriteLine("Error deleting product images: " + ex.Message);
                return false;
            }
        }
        private async Task SaveProductImageMetadataAsync(int productId, string name, string fileName, string filePath, long fileSize, string contentType, int srNo)
        {
            // Example EF Core logic (update this according to your schema)
            var image = new ProductImage
            {
                ProductId = productId,
                FileName = fileName,
                FilePath = filePath,
                Size = fileSize,
                Type = contentType,
                SrNo = srNo,
            };

            _db.ProductImages.Add(image);
            await _db.SaveChangesAsync();
        }
        public async Task<dynamic> GetProductMasterDetails(int masterType, int code)
        {
            if (masterType == 0) return new { Status = 0, Msg = "Master type is required." };

            try
            {
                string sql = $@"SELECT ISNULL(A.[Code], 0) as Code, ISNULL(A.[Name], '') as Name, ISNULL(A.[Alias], '') Alias, ISNULL(A.[PrintName],'') as PrintName, ISNULL(A.[ParentGrp],0) as ParentGrp, ISNULL(B.[Name], '') as ParentGrpName,ISNULL(A.[Barcode], '') as Barcode, IsNull(A.[C1], '') as Slug, ISNULL(A.[C2], '') as SKU, IsNULL(A.[CM1], 0) as Unit, IsNull(C.[Name], '') as UnitName, ISNULL(A.[Remarks], '') as Description, ISNULL(A.[ProductType],0) as ProductType, ISNULL(A.[ProductTypeName], '') as ProductTypeName, ISNULL(CAST(A.[D1] as FLOAT), 0) as Qty, ISNULL(A.[D2], 0) as Price, ISNULL(A.[D3], 0) as MinQty, ISNULL(A.[D4], 0) as Discount, ISNULL(A.[TaxType], 0) as TaxType, ISNULL(A.[TaxTypeName], '') as TaxTypeName, ISNULL(A.[DiscountType], 0) as DiscountType, ISNULL(A.[DiscountTypeName], 0) as DiscountTypeName, CAST(CASE WHEN A.[DeactiveMaster] = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive, CAST(A.[MasterType] as INT) MasterType, ISNULL(A.[CreatedBy], '') as Users, CONVERT(VARCHAR(20), ISNULL(A.[CreationTime], ''), 113) as CreationTime FROM RJMaster1 A LEFT JOIN RJMaster1 B ON A.[ParentGrp] = B.[Code] And B.[MasterType] = 5 LEFT JOIN RJMaster1 C ON A.[CM1] = C.[Code] And C.[MasterType] = 8 WHERE A.[MasterType] = {masterType} 
                    {(code > 0 ? $"AND A.[Code] = {code}" : "")} ORDER BY A.[Name]; ";

                var data = await _db.GetProductMasterRequests.FromSqlRaw(sql).ToListAsync();

                if (data == null || data.Count == 0) return new { Status = 0, Msg = "No master details found." };

                // ✅ Base URL build karo (domain nikalne ke liye)
                //var httpContext = _httpContextAccessor.HttpContext;
                //var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

                var mDetails = new List<GetProductMasterRequest>();

                foreach (var item in data)
                {
                    var imageList = await MapProductWithFullImagePathsAsync(item.Code, baseUrl);

                    mDetails.Add(new GetProductMasterRequest
                    {
                        Code = item.Code,
                        Name = item.Name,
                        Alias = item.Alias,
                        PrintName = item.PrintName,
                        ParentGrp = item.ParentGrp,
                        ParentGrpName = item.ParentGrpName,
                        Barcode = item.Barcode,
                        Slug = item.Slug,
                        SKU = item.SKU,
                        Unit = item.Unit,
                        UnitName = item.UnitName,
                        Description = item.Description,
                        ProductType = item.ProductType,
                        ProductTypeName = item.ProductTypeName,
                        Qty = Math.Round(Convert.ToDouble(item.Qty), 2),
                        MinQty = Math.Round(Convert.ToDouble(item.MinQty), 2),
                        Price = Math.Round(Convert.ToDouble(item.Price), 2),
                        Discount = Math.Round(Convert.ToDouble(item.Discount), 2),
                        TaxType = item.TaxType,
                        TaxTypeName = item.TaxTypeName,
                        DiscountType = item.DiscountType,
                        DiscountTypeName = item.DiscountTypeName,
                        IsActive = item.IsActive,
                        MasterType = item.MasterType,
                        Users = item.Users,
                        CreationTime = item.CreationTime,
                        ImageList = imageList
                    });
                }

                return new { Status = 1, Msg = "Master details fetched successfully.", Data = mDetails };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = "Error fetching master details." + ex.Message };
            }
        }
        public async Task<dynamic> GetMasterListByType(int tranType, int masterType, int code, string? name)
        {
            var mDetails = new List<UnknowList>(); 
            try
            {
                string sql = string.Empty;

                if (tranType == 1)
                {
                    sql = code > 0 ? $"Select IsNull([Code], 0) as Value, IsNull([Name], '') as Label From RJMaster1 Where [MasterType] = {masterType} And Code = {code}" : $"Select IsNull([Code], 0) as Value, IsNull([Name], '') Label From RJMaster1 Where [MasterType] = {masterType}";
                
                }else if (tranType == 2)
                {
                    sql = code > 0 ? $"Select IsNull([Code], 0) as Value, IsNull([Name], '') as Label From RJUserMaster Where [UserType] = {masterType} And Code = {code}" : $"Select IsNull([Code], 0) as Value, IsNull([Name], '') Label From RJUserMaster Where [UserType] = {masterType}";
                }
                var DT1 = await _db.UnknowLists.FromSqlRaw(sql).ToListAsync();

                if (DT1.Count > 0)
                {
                    foreach (var item in DT1)
                    {
                        mDetails.Add(new UnknowList
                        {
                            Value = item.Value,
                            Label = item.Label
                        });
                    }
                    return new { Status = 1, Msg = "Master list fetched successfully.", Data = mDetails };
                }
                else
                {
                    return new { Status = 0, Msg = "No master list found." };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString()};
            }
        }
        public async Task<dynamic> GetMasterNameToCode(int masterType, string name)
        {
            if (masterType == 0)
                return Task.FromResult<dynamic>(new { Status = 0, Msg = "Master type is required." });
            if (name.Length == 0)
                return Task.FromResult<dynamic>(new { Status = 0, Msg = "Name is required." });
            var mDetails = new List<UnknowList>();
            try
            {
                string sql = $"Select IsNull([Code], 0) as Value, IsNull([Name], '') as Label From RJMaster1 Where [MasterType] = {masterType} And Name = '{name.Trim().Replace("'", "''")}' Group by [Code], [Name]";
                var DT1 = await _db.UnknowLists.FromSqlRaw(sql).ToListAsync();

                if (DT1.Count > 0)
                {
                    foreach (var item in DT1)
                    {
                        mDetails.Add(new UnknowList
                        {
                            Value = item.Value,
                            Label = item.Label
                        });
                    }
                    return new { Status = 1, Msg = "Master list fetched successfully.", Data = mDetails };
                }
                else
                {
                    return new { Status = 0, Msg = "No master list found." };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString(), Data = mDetails };
            }
        }
        public async Task<dynamic> GetMasterCodeToName(int masterType, int code)
        {
            if (masterType == 0)
                return Task.FromResult<dynamic>(new { Status = 0, Msg = "Master type is required." });
            if (code == 0)
                return Task.FromResult<dynamic>(new { Status = 0, Msg = "Code is required." });
            var mDetails = new List<UnknowList>();

            try
            {
                string sql = $"Select IsNull([Code], 0) as Value, IsNull([Name], '') as Label From RJMaster1 Where [MasterType] = {masterType} And Name = {code} ";
                var DT1 = await _db.UnknowLists.FromSqlRaw(sql).ToListAsync();

                if (DT1.Count > 0)
                {
                    foreach (var item in DT1)
                    {
                        mDetails.Add(new UnknowList
                        {
                            Value = item.Value,
                            Label = item.Label
                        });
                    }
                    return new { Status = 1, Msg = "Master list fetched successfully.", Data = mDetails };
                }
                else
                {
                    return new { Status = 0, Msg = "No master list found." };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString(), Data = mDetails };
            }
        }
        public async Task<dynamic> SaveMasterDetailRequest(SaveMasterRequest request)
        {
            int Status = 0; string Msg = string.Empty; int code = 0;
            try
            {
                SqlParameter param0 = new SqlParameter("@p0", request.Code);
                SqlParameter param1 = new SqlParameter("@p1", request.Name);
                SqlParameter param2 = new SqlParameter("@p2", request.PrintName ?? string.Empty);
                SqlParameter param3 = new SqlParameter("@p3", request.DeactiveMaster);
                SqlParameter param4 = new SqlParameter("@p4", request.MasterType);
                SqlParameter param5 = new SqlParameter("@p5", request.MasterType == 5 ? "Category" : request.MasterType == 8 ? "Unit" : "");
                SqlParameter param6 = new SqlParameter("@p6", request.Users ?? string.Empty);

                var DT1 = await _db.ResponseNew.FromSqlRaw("EXEC dbo.[sp_SaveMasterWithDetails] @p0, @p1, @p2, @p3, @p4, @p5, @p6", param0, param1, param2, param3, param4, param5, param6).ToListAsync();

                Status = DT1[0].Status;
                Msg = DT1[0].Msg;
                code = DT1[0].Code;


                // ✅ Check if categories saved successfully
                if (code > 0 && request.MasterType == 5)
                {
                    if (Status == 1 && request.Images != null && request.Images.Any())
                    {
                        // ✅ Upload images and save metadata
                        var uploadResult = await UploadProductImagesAsync(code, request.Images, request.Name, "categories");

                        if (!uploadResult.IsSuccess)
                        {
                            // Optionally log or handle image upload failure
                            Msg += " | " + uploadResult.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString() };
            }
            return new { Status = Status, Msg = Msg };
        }
        public async Task<dynamic> GetMasterDetails(int masterType, int code)
        {
            if (masterType == 0)
                return new { Status = 0, Msg = "Master type is required." };

            List<GetMasterRequest> mDetails = new List<GetMasterRequest>();
            try
            {
                string sql = string.Empty;
                if (code > 0)
                {
                    sql = $"SELECT ISNULL(A.[Code], 0) as Code, ISNULL(A.[Name], '') as Name, ISNULL(A.[Alias], '') Alias, ISNULL(A.[PrintName],'') as PrintName, ISNULL(A.[CreatedBy], '') as Users, CAST(CASE WHEN A.[DeactiveMaster] = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive, CAST(A.[MasterType] as INT) MasterType, CONVERT(VARCHAR(20), (ISNULL([CreationTime], '')), 113) as CreationTime FROM RJMaster1 A WHERE A.[Code] = {code} And A.[MasterType] = {masterType} ORDER BY A.[Name]";
                }
                else
                {
                    sql = $"SELECT ISNULL(A.[Code], 0) as Code, ISNULL(A.[Name], '') as Name, ISNULL(A.[Alias], '') Alias, ISNULL(A.[PrintName],'') as PrintName, ISNULL(A.[CreatedBy], '') as Users, CAST(CASE WHEN A.[DeactiveMaster] = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive, CAST(A.[MasterType] as INT) MasterType, CONVERT(VARCHAR(20), (ISNULL([CreationTime], '')), 113) as CreationTime FROM RJMaster1 A WHERE A.[MasterType] = {masterType} ORDER BY A.[Name]";
                }
                var DT1 = await _db.GetMasterRequests.FromSqlRaw(sql).ToListAsync();

                if (DT1.Count > 0)
                {
                    var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

                    foreach (var item in DT1)
                    {
                        var imageList = await MapProductWithFullImagePathsAsync(item.Code, baseUrl);
                        mDetails.Add(new GetMasterRequest
                        {
                            Code = item.Code,
                            Name = item.Name,
                            PrintName = item.PrintName,
                            IsActive = item.IsActive,
                            MasterType = item.MasterType,
                            Users = item.Users,
                            CreationTime = item.CreationTime,
                            ImageList = imageList
                        });
                    }
                    return new { Status = 1, Msg = "Master details fetched successfully.", Data = mDetails };
                }
                else
                {
                    return new { Status = 0, Msg = "No master details found." };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = "Error fetching master details." + ex.Message };
            }
        }
        private async Task<dynamic> MapProductWithFullImagePathsAsync(int code, string baseUrl)
        {
            var imageList = await _db.ProductImages
            .Where(img => img.ProductId == code)
            .OrderBy(img => img.SrNo)
            .Select(img => new ProductImageDto
            {
                ProductId = code,
                FilePath = $"{baseUrl}{img.FilePath}",
                FileName = img.FileName,
                Size = img.Size,
                Type = img.Type,
                SrNo = img.SrNo
            })
            .ToListAsync();

            return imageList;
        }
        public async Task<dynamic> DeleteMasterByTypeAndCode(int tranType, int masterType, int code)
        {
            try
            {
                // Delete associated product images
                bool deleted = await DeleteProductImagesAsync(code); 

                if (!deleted)
                {
                    return new { Status = 0, Msg = $"Unable to remove images for Product ID: {code}. Please try again." };
                }

                // Delete master details from the database
                string sql = $"EXEC dbo.[sp_DeleteMasterByTypeAndCode] @TranType = {tranType}, @MasterType = {masterType}, @Code = {code}";
                var result = await _db.Responses.FromSqlRaw(sql).ToListAsync();

                if (result.Count > 0)
                {
                    return new { Status = result[0].Status, Msg = result[0].Msg };
                }
                else
                {
                    return new { Status = 0, Msg = "No master details found for deletion." };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = "Error deleting master details: " + ex.Message };
            }
        }
        public async Task<dynamic> SaveUserMasterRequest(SaveUserMasterRequest request)
        {
            try
            {
                SqlParameter p1 = new SqlParameter("@p1", request.Code);
                SqlParameter p2 = new SqlParameter("@p2", request.Name);
                SqlParameter p3 = new SqlParameter("@p3", request.Mobile);
                SqlParameter p4 = new SqlParameter("@p4", request.Email);
                SqlParameter p5 = new SqlParameter("@p5", request.Username);
                SqlParameter p6 = new SqlParameter("@p6", request.Pwd);
                SqlParameter p7 = new SqlParameter("@p7", request.Role);
                SqlParameter p8 = new SqlParameter("@p8", request.UserType);
                SqlParameter p9 = new SqlParameter("@p9", request.Remark);
                SqlParameter p10 = new SqlParameter("@p10", request.Base64 ?? string.Empty);
                SqlParameter p11 = new SqlParameter("@p11", request.Status);
                SqlParameter p12 = new SqlParameter("@p12", request.Users ?? string.Empty);

                var DT1 = await _db.ResponseNew.FromSqlRaw("EXEC dbo.[sp_SaveUserMasterDet] @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12", p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12).ToListAsync();
                
                int Status = DT1[0].Status;
                string Msg = DT1[0].Msg;

                // ✅ Check if user saved successfully
                //if (Status == 1 && request.Base64 != null)
                //{
                //     //✅ Upload image and save metadata
                //    var uploadResult = await UploadProductImagesAsync(DT1[0].Code, new List<IFormFile> { new FormFile(new MemoryStream(Convert.FromBase64String(request.Base64)), 0, request.Base64.Length, "file", "profile.jpg") }, request.Name, "users");
                //    if (!uploadResult.IsSuccess)
                //    {
                //        // Optionally log or handle image upload failure
                //        Msg += " | " + uploadResult.Message;
                //    }
                //}
                //else
                //{
                //    return new { Status = Status, Msg = Msg };
                //}
                return new { Status = Status, Msg = Msg };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString() };
            }
        }
        public async Task<dynamic> GetUserMasterDetAsync(int userType, int code)
        {
            try
            {
                string sql = code == 0 ? "select ISNULL(Code, 0) as Code, ISNULL(Name, '') as Name, ISNULL(Email, '') Email, ISNULL(Mobile, '') Mobile, ISNULL(UserName, '') as Username, ISNULL(Password, '') as PWD, ISNULL(Role, 0) as Role, ISNULL(Remark, '') as [Remark], ISNULL(Base64, '') as Image, ISNULL(CreationOn, '') as CreationOn, CONVERT(VARCHAR, ISNULL(CreationTime, ''), 29) as CreationTime, CASE WHEN [Status] = 2 THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END IsActive From RJUserMaster" : $"select ISNULL(Code, 0) as Code, ISNULL(Name, '') as Name, ISNULL(Email, '') Email, ISNULL(Mobile, '') Mobile, ISNULL(UserName, '') as Username, ISNULL(Password, '') as PWD, ISNULL(Role, 0) as Role, ISNULL(Remark, '') as [Remark], ISNULL(Base64, '') as Image, ISNULL(CreationOn, '') as CreationOn, CONVERT(VARCHAR, ISNULL(CreationTime, ''), 29) as CreationTime, CASE WHEN [Status] = 2 THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END IsActive From RJUserMaster Where Code = {code}";
                var DT1 = await _db.GetUserMasterDetailRequests.FromSqlRaw(sql).ToListAsync();

                if (DT1.Count > 0) 
                {
                    var userDetails = DT1.Select(item => new GetUserMasterDetailRequest
                    {
                        Code = item.Code,
                        Name = item.Name,
                        Email = item.Email,
                        Mobile = item.Mobile,
                        Username = item.Username,
                        Pwd = item.Pwd,
                        Role = item.Role,
                        Remark = item.Remark,
                        Image = item.Image,
                        CreationOn = item.CreationOn,
                        CreationTime = item.CreationTime,
                        IsActive = item.IsActive
                    }).ToList();

                    return new { Status = 1, Msg = "User details fetched successfully.", Data = userDetails };
                }
                else
                {
                    return new { Status = 0, Msg = "No user details found." };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message };
            }
        }
        public async Task<dynamic> SaveProductsFromExcel(ImportProductRequest request)
        {
            try
            {
                string XML = Helper.CreateXml(request);
                SqlParameter p1 = new SqlParameter("@p1", XML);
                SqlParameter p2 = new SqlParameter("@p2", 6);
                SqlParameter p3 = new SqlParameter("p3", "Product");
                SqlParameter p4 = new SqlParameter("p4", "Admin");
                
                var DT1 = await _db.Responses.FromSqlRaw("EXEC dbo.[sp_SaveProductsFromExcel] @p1, @p2, @p3, @p4", p1, p2, p3, p4).ToListAsync();
                
                int Status = DT1[0].Status;
                string Msg = DT1[0].Msg;

                // ✅ Check if import saved successfully
                //if (Status == 1 && request.Images != null && request.Images.Any())
                //{
                //    // ✅ Upload images and save metadata
                //    var uploadResult = await UploadProductImagesAsync(DT1[0].Code, request.Images, request.Name, "products");
                //    if (!uploadResult.IsSuccess)
                //    {
                //        // Optionally log or handle image upload failure
                //        Msg += " | " + uploadResult.Message;
                //    }
                //}
                if (Status == 1)
                {
                    return new { Status = 1, Msg = Msg };
                }
                else
                {
                    return new { Status = 0, Msg = Msg };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString() };
            }
        }
        public async Task<dynamic> UpdateStockFromExcel(ImportStockRequest request)
        {
            try
            {
                string XML = Helper.CreateXml(request);
                SqlParameter p1 = new SqlParameter("@p1", XML);
                SqlParameter p2 = new SqlParameter("@p2", 6);

                var DT1 = await _db.Responses.FromSqlRaw("EXEC dbo.[sp_UpdateStockFromExcel] @p1, @p2", p1, p2).ToListAsync();

                int Status = DT1[0].Status;
                string Msg = DT1[0].Msg;

                if (Status == 1)
                {
                    return new { Status = 1, Msg = Msg };
                }
                else
                {
                    return new { Status = 0, Msg = Msg };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message.ToString() };
            }
        }
        public async Task<UpiTransaction> CreateTransaction(decimal amount)
        {
            var transactionId = "TXN" + DateTime.UtcNow.Ticks;

            var txn = new UpiTransaction
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                Amount = amount,
                CreatedAt = DateTime.UtcNow,
            };

            await _db.UpiTransactions.AddAsync(txn);
            await _db.SaveChangesAsync();

            _ = SimulatePaymentStatusUpdate(transactionId);

            return txn;
        }
        public async Task<UpiTransaction> GetStatus(string transactionId)
        {
            return await _db.UpiTransactions.FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }
        //public async Task SimulatePaymentStatusUpdate(string transactionId)
        //{
        //    await Task.Delay(15000); // simulate delay
        //    var txn = await _db.UpiTransactions.FirstOrDefaultAsync(x => x.TransactionId == transactionId);
        //    if (txn == null) return;

        //    var rand = new Random().NextDouble();
        //    txn.Status = rand < 0.7 ? "SUCCESS" : rand < 0.9 ? "FAILED" : "CANCELLED";
        //    txn.StatusType = txn.Status == "SUCCESS" ? 1 : txn.Status == "FAILED" ? 2 : txn.Status == "CANCELLED" ? 3 : 0;
        //    await _db.SaveChangesAsync();
        //}

        //public Product MapProductWithFullImagePaths(Product product)
        //{
        //    var request = _httpContextAccessor.HttpContext.Request;
        //    var baseUrl = $"{request.Scheme}://{request.Host}";

        //    var fullPaths = product.ImagePaths
        //        .Select(path => $"{baseUrl}{path}") // e.g. https://example.com/uploads/products/image.jpg
        //        .ToList();

        //    product.ImagePaths = fullPaths;
        //    return product;
        //}

        //public async Task<dynamic> UploadProductImagesAsync(List<IFormFile> images, int productId)
        //{
        //    var uploadedPaths = new List<string>();
        //    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");

        //    if (!Directory.Exists(uploadPath))
        //        Directory.CreateDirectory(uploadPath);

        //    foreach (var image in images)
        //    {
        //        if (image.Length > 0)
        //        {
        //            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        //            var filePath = Path.Combine(uploadPath, fileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await image.CopyToAsync(stream);
        //            }

        //            var relativePath = $"/uploads/products/{fileName}";
        //            uploadedPaths.Add(relativePath);

        //            // Save metadata in DB (use your ORM or SQL here)
        //            //string sql = "INSERT INTO ProductImages (ProductId, FileName, FilePath, FileSize, FileType, UploadedAt) VALUES (@ProductId, @FileName, @FilePath, @FileSize, @FileType, @UploadedAt)";
        //            //var DT1 = await _db.
        //            //await _db.ExecuteAsync(sql, new
        //            //{
        //            //    ProductId = productId,
        //            //    FileName = fileName,
        //            //    FilePath = relativePath,
        //            //    FileSize = image.Length,
        //            //    FileType = image.ContentType,
        //            //    UploadedAt = DateTime.UtcNow
        //            //});
        //        }
        //    }

        //    if (uploadedPaths.Count == 0)
        //        return (false, "No valid images uploaded.", new List<string>());

        //    return (true, "Images uploaded and saved in DB.", uploadedPaths);
        //}
        public async Task SimulatePaymentStatusUpdate(string transactionId)
        {
            Console.WriteLine("⏳ Starting SimulatePaymentStatusUpdate...");

            await Task.Delay(15000); // Simulate delay

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<RetailPosDBContext>();

                var txn = await db.UpiTransactions.FirstOrDefaultAsync(x => x.TransactionId == transactionId);
                if (txn == null)
                {
                    Console.WriteLine("❌ Transaction not found: " + transactionId);
                    return;
                }

                Console.WriteLine("✅ Transaction found: " + txn.TransactionId);

                var rand = new Random().NextDouble();
                txn.Status = rand < 0.7 ? "SUCCESS" : rand < 0.9 ? "FAILED" : "CANCELLED";
                txn.StatusType = txn.Status == "SUCCESS" ? 1 : txn.Status == "FAILED" ? 2 : 3;

                db.UpiTransactions.Update(txn);
                await db.SaveChangesAsync();

                Console.WriteLine($"✅ Transaction updated: {txn.TransactionId} → {txn.Status}");
            }
        }
        public async Task SubmitFeedback(Feedback feedback)
        {
            feedback.Id = Guid.NewGuid();
            feedback.SubmittedAt = DateTime.UtcNow;
            await _db.Feedbacks.AddAsync(feedback);
            await _db.SaveChangesAsync();
        }
        public async Task<dynamic> GetPosCategoriesWithProductsAsync()
        {
            try
            {
                var categoryList = new List<Category>();
                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

                string sql = @"SELECT A.Code AS CategoryCode, A.Name AS CategoryName, '' AS CategoryImg, ISNULL(B.Code, 0) AS ProductCode, ISNULL(B.Name, '') AS ProductName, '' AS ProductImg, ISNULL(B.Remarks, '') AS ProductDesc, CAST(100 AS FLOAT) AS StockQty, ISNULL(B.D1, 0) AS Qty, ISNULL(B.D2, 0) AS Price, ISNULL(B.D3, 0) AS MinQty, CASE WHEN ISNULL(B.ProductType, 0) = 1 THEN 1 WHEN ISNULL(B.ProductType, 0) = 2 THEN 0 ELSE 1 END AS IsVeg, ISNULL(B.Barcode, '') AS Barcode FROM RJMaster1 A LEFT JOIN RJMaster1 B ON A.Code = B.ParentGrp WHERE A.MasterType = 5 AND (A.DeactiveMaster IS NULL OR A.DeactiveMaster = 0) ORDER BY A.Name, B.Name";

                var rawList = await _db.RawCategoryProducts.FromSqlRaw(sql).ToListAsync();

                if (rawList.Count == 0)
                    return new { Status = 0, Msg = "No data found", Data = categoryList };

                foreach (var row in rawList)
                {
                    var category = categoryList.FirstOrDefault(c => c.Code == row.CategoryCode);
                    if (category == null)
                    {
                        var catImages = await MapProductWithFullImagePathsAsync(row.CategoryCode, baseUrl);
                        category = new Category
                        {
                            Code = row.CategoryCode,
                            Name = row.CategoryName,
                            ImageUrl = catImages != null && catImages?.Count > 0 ? catImages?[0]?.FilePath : string.Empty,
                            Items = new List<Item>()
                        };
                        categoryList.Add(category);
                    }

                    if (row.ProductCode > 0)
                    {
                        var prodImages = await MapProductWithFullImagePathsAsync(row.ProductCode, baseUrl);
                        category.Items.Add(new Item
                        {
                            Code = row.ProductCode,
                            Name = row.ProductName,
                            Description = row.ProductDesc,
                            ImageUrl = prodImages != null && prodImages?.Count > 0 ? prodImages?[0]?.FilePath : string.Empty,
                            Stock = row.StockQty,
                            Quantity = row.Qty,
                            Price = row.Price,
                            MinimumQuantity = row.MinQty,
                            IsVegetarian = row.IsVeg == 1,
                            Barcode = row.Barcode
                        });
                    }
                }

                return new { Status = 1, Msg = "categories-with-products successfully fetch", Data = categoryList };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message, Data = new List<Category>() };
            }
        }
        public async Task<dynamic> GetPosCategoriesAsync()
        {
            try
            {
                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
                
                string sql = @"SELECT A.Code AS Code, A.Name AS Name, '' AS Image From RJMaster1 A WHERE A.MasterType = 5 AND (A.DeactiveMaster IS NULL OR A.DeactiveMaster = 0) ORDER BY A.[Name]";

                var categories = await _db.RawCategories.FromSqlRaw(sql).ToListAsync();

                if (categories.Count == 0)
                    return new { Status = 0, Msg = "No categories found", Data = new List<Category>() };

                var result = new List<Category>();

                foreach (var row in categories)
                {
                    var images = await MapProductWithFullImagePathsAsync(row.Code, baseUrl);

                    result.Add(new Category
                    {
                        Code = row.Code,
                        Name = row.Name,
                        ImageUrl = images != null && images?.Count > 0 ? images?[0]?.FilePath : string.Empty,
                        Items = new List<Item>()
                    });
                }

                return new { Status = 1, Msg = "categories successfully fetch", Data = result };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message, Data = new List<Category>() };
            }
        }

        public async Task<dynamic> GetPosProductsAsync()
        {
            try
            {
                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
                
                string sql = @"SELECT A.Code AS Code, A.Name AS Name, '' AS Image, ISNULL(A.Remarks, '') AS Description, CAST(100 AS FLOAT) AS Stock, ISNULL(A.D1, 0) AS Qty, ISNULL(A.D2, 0) AS Price, ISNULL(A.D3, 0) AS MinQty, CASE WHEN ISNULL(A.ProductType, 0) = 1 THEN 1 WHEN ISNULL(A.ProductType, 0) = 2 THEN 0 ELSE 1 END AS IsVeg, ISNULL(A.Barcode, '') AS Barcode, A.ParentGrp AS ParentGrp FROM RJMaster1 A WHERE A.MasterType = 6 And (A.DeactiveMaster IS NULL OR A.DeactiveMaster = 0) ORDER BY A.Name";

                var products = await _db.RawProducts.FromSqlRaw(sql).ToListAsync();

                if (products.Count == 0)
                    return new { Status = 0, Msg = "No products found", Data = new List<Item>() };

                // Fetch all images for all products in a single query
                var productIds = products.Select(p => p.Code).ToList();
                var images = await _db.ProductImages.Where(img => productIds.Contains(img.ProductId)).OrderBy(img => img.SrNo).ToListAsync();

                // Map images by product
                var imageDict = images.GroupBy(img => img.ProductId)
                    .ToDictionary(g => g.Key, g => g.Select(img => new ProductImageDto
                    {
                        ProductId = img.ProductId,
                        FilePath = $"{baseUrl}{img.FilePath}",
                        FileName = img.FileName,
                        Size = img.Size,
                        Type = img.Type,
                        SrNo = img.SrNo
                    }).ToList());

                var result = products.Select(row =>
                {
                    var prodImages = imageDict.ContainsKey(row.Code) ? imageDict[row.Code] : new List<ProductImageDto>();

                    return new Item
                    {
                        Code = row.Code,
                        Name = row.Name,
                        ParentGrp = row.ParentGrp,
                        Description = row.Description,
                        ImageUrl = prodImages.FirstOrDefault()?.FilePath ?? string.Empty,
                        Stock = row.Stock,
                        Quantity = row.Qty,
                        Price = row.Price,
                        MinimumQuantity = row.MinQty,
                        IsVegetarian = row.IsVeg == 1,
                        Barcode = row.Barcode
                    };
                }).ToList();

                return new { Status = 1, Msg = "products successfully fetch", Data = result };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message, Data = new List<Item>() };
            }
        }

        public async Task<dynamic> SaveOrUpdateCustomerDetAsync(SvCustomerDet obj)
        {
            try
            {
                SqlParameter param0 = new("@p0", obj.Code);
                SqlParameter param1 = new("@p1", obj.Name); 
                SqlParameter param2 = new("@p2", obj.Email);                          // Email
                SqlParameter param3 = new("@p3", obj.Phone);                          // Phone
                SqlParameter param4 = new("@p4", obj.CountryCode);                    // Country
                SqlParameter param5 = new("@p5", obj.StateCode);                      // State
                SqlParameter param6 = new("@p6", obj.CityCode);                       // City
                SqlParameter param7 = new("@p7", obj.Address);                        // Address
                SqlParameter param8 = new("@p8", obj.Status ? 1 : 0);               // Status
                SqlParameter param9 = new("@p9", (object?)obj.Image ?? DBNull.Value); // Image (nullable)
                SqlParameter param10 = new("@p10", obj.CreatedBy);                    // User
                SqlParameter param11 = new("@p11", obj.MasterType);

                var DT1 = await _db.Responses.FromSqlRaw(@"EXEC dbo.[sp_SaveOrUpdateCustomer] @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11", param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11).ToListAsync();

                return new { Status = DT1[0].Status, Msg = DT1[0].Msg };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message };
            }
        }
        public async Task<dynamic> GetCustomerDetAsync(int code)
        {
            try
            {
                var list = new List<GetCustomerDet>();

                string sql = code == 0 ? "SELECT A.[Code], A.[Name], ISNULL(A.[Email], '') as Email, ISNULL(A.[Mobile], '') as Phone, ISNULL(A.[CM1], 0) as CountryCode, ISNULL(M.[Name], '') as CountryName, ISNULL(A.[CM2], 0) as StateCode, ISNULL(M1.[Name], '') as StateName, ISNULL(A.[CM3], 0) as CityCode, ISNULL(M2.[Name], '') as CityName, ISNULL(A.[C3], '') as [Address], ISNULL(A.[ImageLink], '') as Image, ISNULL(A.[CreatedBy], '') as CreatedBy, CONVERT(VARCHAR, ISNULL(A.CreationTime, ''), 113) as CreatedOn, CASE WHEN A.[Active] = 2 THEN CAST(0 as BIT) ELSE CAST(1 as BIT) END IsActive FROM RJMaster1 A LEFT JOIN RJMaster1 M ON A.CM1 = M.Code And M.MasterType = 55 LEFT JOIN RJMaster1 M1 ON A.CM2 = M1.Code And M1.MasterType = 56 LEFT JOIN RJMaster1 M2 ON A.CM3 = M2.Code And M2.MasterType = 57 Where A.MasterType = 2" : $"SELECT A.[Code], A.[Name], ISNULL(A.[C1], '') as FirstName, ISNULL(A.[C2], '') as LastName, ISNULL(A.[Email], '') as Email, ISNULL(A.[Mobile], '') as Phone, ISNULL(A.[CM1], 0) as CountryCode, ISNULL(M.[Name], '') as CountryName, ISNULL(A.[CM2], 0) as StateCode, ISNULL(M1.[Name], '') as StateName, ISNULL(A.[CM3], 0) as CityCode, ISNULL(M2.[Name], '') as CityName, ISNULL(A.[C3], '') as [Address], ISNULL(A.[ImageLink], '') as Image, ISNULL(A.[CreatedBy], '') as CreatedBy, CONVERT(VARCHAR, ISNULL(A.CreationTime, ''), 113) as CreatedOn, CASE WHEN A.[Active] = 2 THEN CAST(0 as BIT) ELSE CAST(1 as BIT) END IsActive FROM RJMaster1 A LEFT JOIN RJMaster1 M ON A.CM1 = M.Code And M.MasterType = 55 LEFT JOIN RJMaster1 M1 ON A.CM2 = M1.Code And M1.MasterType = 56 LEFT JOIN RJMaster1 M2 ON A.CM3 = M2.Code And M2.MasterType = 57 Where A.MasterType = 2 And A.[Code] = {code}";
                list = await _db.GetCustomerDets.FromSqlRaw(sql).ToListAsync();

                if (list.Count == 0)
                {
                    return new { Status = 1, Msg = "Data Not Found!!!", Data = list };
                }
                else
                {
                    return new { Status = 1, Msg = "Success", Data = list };
                }
                
            }
            catch(Exception ex)
            {
                return new { Status = 1, Msg = ex.Message, Data = new List<GetCustomerDet>() };
            }
        }
        public async Task<dynamic> GetProductSearchAsync(int tranType, int masterType, int code, SearchProduct obj)
        {
            try
            {
                string sql = string.Empty;

                if (tranType == 1)
                {
                    if (!string.IsNullOrWhiteSpace(obj?.Name))
                    {
                        var keywords = obj.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var keywordConditions = keywords.Select(k => $"([Name] LIKE '%{k}%' OR [Barcode] LIKE '%{k}%' OR [Alias] LIKE '%{k}%')").ToList();
                        var combinedConditions = string.Join(" AND ", keywordConditions);

                        sql = $@"SELECT ISNULL([Code], 0) AS Value, ISNULL([Name], '') AS Label FROM RJMaster1 WHERE [MasterType] = {masterType} AND {combinedConditions}";
                    }
                }

                List<UnknowList>? list = await _db.UnknowLists.FromSqlRaw(sql).ToListAsync() ?? throw new Exception("Data Not Found !!!");

                return new { Status = 1, Msg = "Success", Data = list };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message };
            }
        }
        public async Task<dynamic> SaveOrUpdateTransactionDetAsync(TProduct obj)
        {
            try
            {
                // 1️ Auto Voucher Number
                int autoVchNo = GetAutoVchNo(obj.VchType);

                // 2️ Get voucher info
                var vchNoResult = GetVchNo(obj.TranType, obj.VchType);
                var vchno = !string.IsNullOrWhiteSpace(obj.VchNo) ? obj.VchNo : vchNoResult?.Result?.Data[0]?.vchNo ?? $"VCH-{DateTime.Now:yyyyMMddHHmmss}";
                var refNo = obj?.VchType == 9 ? vchNoResult?.Result?.Data[0]?.refNo : obj?.RefNo ?? "";
                var ordNo = !string.IsNullOrWhiteSpace(obj?.OrderNo) ? obj.OrderNo : vchNoResult?.Result?.Data[0]?.orderId ?? "";

                // 3️ Handle payment method safely
                var method = obj?.PaymentMethod?.Trim().ToUpper();
                var pMethod = method switch
                {
                    "CASH" => 1,
                    "ONLINE" => 2,
                    _ => 0
                };

                //DateTime safeDate = string.IsNullOrEmpty(obj?.Date?.ToString()) ? DateTime.Now : Convert.ToDateTime(obj.Date);
                // Date को dd-MMM-yyyy format में convert करना
                //string formattedDate = safeDate.ToString("dd-MMM-yyyy");

                // 4️ Safe Date Parsing (IST fallback)
                DateTime safeDate;
                string[] formats = { "dd-MMM-yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fffZ" };

                //!DateTime.TryParseExact(obj.Date.ToString(), formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out safeDate))
        
                if (string.IsNullOrEmpty(obj?.Date?.ToString()) || !DateTime.TryParseExact(obj.Date.ToString(), formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out safeDate))
                {
                    // null case → आज की IST date
                    var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    safeDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone).Date;
                }

                // 5️ Convert Product List to XML
                string XMLData = Helper.CreateXml(obj.TProductDets);

                // 6️ SQL Parameters
                SqlParameter param0 = new("@p0", obj.VchCode);
                SqlParameter param1 = new("@p1", obj.VchType);
                SqlParameter param2 = new("@p2", safeDate.Date);
                SqlParameter param3 = new("@p3", vchno);
                SqlParameter param4 = new("@p4", refNo);
                SqlParameter param5 = new("@p5", autoVchNo);
                SqlParameter param6 = new("@p6", obj.AccCode);
                SqlParameter param7 = new("@p7", obj.MCCode);
                SqlParameter param8 = new("@p8", obj.STType);
                SqlParameter param9 = new("@p9", obj.SubTot);
                SqlParameter param10 = new("@p10", obj.TaxAmt);
                SqlParameter param11 = new("@p11", obj.Discount);
                SqlParameter param12 = new("@p12", obj.Shipping);
                SqlParameter param13 = new("@p13", obj.TotAmt);
                SqlParameter param14 = new("@p14", obj.TranType);
                SqlParameter param15 = new("@p15", obj.Remarks ?? "");
                SqlParameter param16 = new("@p16", obj.CreatedBy ?? "");
                SqlParameter param17 = new("@p17", pMethod);
                SqlParameter param18 = new("@p18", obj.TxnId ?? "");
                SqlParameter param19 = new("@p19", ordNo ?? "");
                SqlParameter param20 = new("@p20", XMLData);

                // 7️ Execute SP
                var result = await _db.ResponseNew.FromSqlRaw(@"EXEC dbo.[sp_SaveOrUpdateTransaction] @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, @p17, @p18, @p19, @p20", param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19, param20).ToListAsync();

                return new { Status = result[0].Status, Msg = result[0].Msg, OrderId = result[0].Code, OrderNo = ordNo };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message };
            }
        }

        //public async Task<dynamic> GetAllProducts()
        //{
        //    var productList = new List<TProduct>();

        //    try
        //    {
        //        string sql = @"Select ISNULL(CONVERT(VARCHAR, A.[Date], 113), '') as Date, ISNULL(A.[VchNo],'') as VchNo, A.[MasterCode1] as AccCode, ISNULL(M1.[Name], '') as AccName, A.[MasterCode2] as MCCode, ISNULL(M1.[Name], '') as MCName, A.[STType] as STType, 
        //                        ISNULL(M3.[Name], '') As STName, ISNULL(A.[ReferenceNo], '') as RefNo, ISNULL(A.[D1], 0) as SubTot, ISNULL(A.[D1], 0) as SubTot, ISNULL(A.[D2], 0) as TaxAmt, ISNULL(A.[D2], 0) as TaxAmt, ISNULL(A.[D3], 0) As Discount, 
        //                        ISNULL(A.[D4], 0) As Shipping, ISNULL(A.[D5], 0) As GrandTotal, ISNULL(B.[MasterCode1], 0) as ItemCode,  ISNULL(M4.[Name], '') As ItemName, ISNULL(B.[MasterCode2], 0) As IMCCode, ISNULL(M5.[Name], '') As IMCName,
        //                        ISNULL(B.Value1, 0) as Qty, ISNULL(B.Value2, 0) as Price, ISNULL(B.Value3, 0) as Amount, ISNULL(B.[D1], 0) as UnitCost, ISNULL(B.[D2], 0) as Discount, ISNULL(B.[D1], 0) as TaxPer, ISNULL(B.[D1], 0) as TaxAmt From RJTran1 A INNER JOIN RJTran2 B ON A.VchCode = B.VchCode 
        //                        LEFT JOIN RJMaster1 M1 ON A.MasterCode1 = M1.Code
        //                        LEFT JOIN RJMaster1 M2 ON A.MasterCode2 = M2.Code
        //                        LEFT JOIN RJMaster1 M3 ON A.STType = M3.Code
        //                        LEFT JOIN RJMaster1 M4 ON B.MasterCode1 = M4.Code
        //                        LEFT JOIN RJMaster1 M5 ON B.MasterCode2 = M5.Code";

        //        var rawList = await _db.RawProductDatas.FromSqlRaw(sql).ToListAsync();

        //        if (!rawList.Any())
        //            return new { Status = 0, Msg = "No Data Found", Data = productList };

        //        foreach (var row in rawList)
        //        {
        //            var existingProduct = productList.FirstOrDefault(p => p.VchCode == row.VchCode);

        //            if (existingProduct == null)
        //            {
        //                existingProduct = new TProduct
        //                {
        //                    VchCode = row.VchCode,
        //                    VchType = row.VchType,
        //                    Date = string.IsNullOrEmpty(row.Date) ? null : DateTime.Parse(row.Date),
        //                    VchNo = row.VchNo,
        //                    RefNo = row.RefNo,
        //                    AccCode = row.AccCode,
        //                    AccName = row.AccName,
        //                    MCCode = row.MCCode,
        //                    MCName = row.MCName,
        //                    STType = row.STType,
        //                    STName = row.STName,
        //                    SubTot = row.SubTot,
        //                    TaxAmt = row.TaxAmt,
        //                    Discount = row.Discount,
        //                    Shipping = row.Shipping,
        //                    TotAmt = row.TotAmt,
        //                    TProductDets = new List<TProductDet>()
        //                };

        //                productList.Add(existingProduct);
        //            }

        //            // Add detail row
        //            var detail = new TProductDet
        //            {
        //                ItemCode = row.ItemCode,
        //                ItemName = row.ItemName,
        //                MCCode = row.IMCCode,
        //                MCName = row.IMCName,
        //                Qty = row.Qty,
        //                Price = row.Price,
        //                Amount = row.Amount,
        //                UnitCost = row.UnitCost,
        //                Discount = row.ItemDiscount,
        //                TaxPer = row.TaxPer,
        //                TaxAmt = row.ItemTaxAmt
        //            };

        //            existingProduct.TProductDets.Add(detail);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { Status = 0, Msg = ex.Message, Data = productList };
        //    }

        //    return new { Status = 1, Msg = "Success", Data = productList };
        //}
        public async Task<dynamic> GetTransactionsDetAsync(int vchType, int? vchCode = null)
        {
            var transactionList = new List<TProduct>();

            try
            {
                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

                string sql = @"Select A.[VchCode], A.[VchType], ISNULL(CONVERT(VARCHAR, A.[Date], 106), '') as Date, ISNULL(A.[VchNo],'') as VchNo, A.[MasterCode1] as AccCode, ISNULL(M1.[Name], '') as AccName, A.[MasterCode2] as MCCode, ISNULL(M1.[Name], '') as MCName, A.[STType] as STType, 
                            ISNULL(M3.[Name], '') As STName, ISNULL(A.[RefNo], '') as RefNo, ISNULL(CAST(A.[D1] as FLOAT), 0) as SubTot, ISNULL(CAST(A.[D2] as FLOAT), 0) as TaxAmt, ISNULL(CAST(A.[D3] as FLOAT), 0) As Discount, 
                            ISNULL(CAST(A.[D4] as FLOAT), 0) As Shipping, ISNULL(CAST(A.[D5] as FLOAT), 0) As TotAmt, ISNULL(B.[MasterCode1], 0) as ItemCode,  ISNULL(M4.[Name], '') As ItemName, ISNULL(B.[MasterCode2], 0) As IMCCode, ISNULL(M5.[Name], '') As IMCName,
                            ISNULL(CAST(B.[Value1] as FLOAT), 0) as Qty, ISNULL(CAST(B.[Value2] As FLOAT), 0) as Price, ISNULL(CAST(B.[Value3] As FLOAT), 0) as Amount, ISNULL(CAST(B.[D1] as FLOAT), 0) as UnitCost, ISNULL(CAST(B.[D2] as FLOAT), 0) as ItemDiscount, 
                            ISNULL(CAST(B.[D3] as FLOAT), 0) as ItemTaxPer, ISNULL(CAST(B.[D4] as FLOAT), 0) as ItemTaxAmt, ISNULL(A.[Remarks], '') as Remarks, ISNULL(CASE WHEN A.[PaymentMethod] = 1 THEN 'CASH' WHEN A.[PaymentMethod] = 2 THEN 'ONLINE' ELSE 'UNKNOW' END, '') as PaymentMethod, ISNULL(C.[PaymentStatus], 'PENDING') as PaymentStatus, ISNULL(C.[PaymentMode], '') as PaymentMode,
                            ISNULL(C.[TxnId], '') as txnId, ISNULL(C.[GatewayTxnId], '') as GatewayTxnId, ISNULL(C.[BankRefNo], '') as BankRefNo, ISNULL(C.[Gateway], '') as PaymentGateway, ISNULL(C.[Remarks], '') as PaymentRemark, ISNULL(CAST(C.[Amount] AS FLOAT), 0) as PayAmt, ISNULL(A.[OrderId] , '') As OrderId, ISNULL(A.[CreatedBy], '') As CreatedBy, ISNULL(CONVERT(VARCHAR, A.[CreationTime], 109), '') as CreatedOn From RJTran1 A INNER JOIN RJTran2 B ON A.VchCode = B.VchCode 
                            LEFT JOIN RJMaster1 M1 ON A.MasterCode1 = M1.Code
                            LEFT JOIN RJMaster1 M2 ON A.MasterCode2 = M2.Code
                            LEFT JOIN RJMaster1 M3 ON A.STType = M3.Code
                            LEFT JOIN RJMaster1 M4 ON B.MasterCode1 = M4.Code
                            LEFT JOIN RJMaster1 M5 ON B.MasterCode2 = M5.Code
                            LEFT JOIN PaymentTransaction C ON A.VchCode = C.VchCode
                            WHERE A.[VchType] = @vchType AND (@VchCode IS NULL OR @VchCode <= 0 OR A.VchCode = @VchCode) ORDER BY VchCode Desc";

                var rawList = await _db.RawTProductDatas.FromSqlRaw(sql, new SqlParameter("@VchType", vchType), new SqlParameter("@VchCode", (object?)vchCode ?? DBNull.Value)).ToListAsync();

                if (!rawList.Any())
                    return new { Status = 0, Msg = "No Data Found", Data = transactionList };

                foreach (var row in rawList)
                {
                    var existingTxn = transactionList.FirstOrDefault(p => p.VchCode == row.VchCode);

                    var imageList = await MapProductWithFullImagePathsAsync(row.ItemCode, baseUrl);

                    if (existingTxn == null)
                    {
                        existingTxn = new TProduct
                        {
                            VchCode = row.VchCode,
                            VchType = row.VchType,
                            Date = row.Date,
                            VchNo = row.VchNo,
                            RefNo = row.RefNo,
                            AccCode = row.AccCode,
                            AccName = row.AccName,
                            MCCode = row.MCCode,
                            MCName = row.MCName,
                            STType = row.STType,
                            STName = row.STName,
                            SubTot = row.SubTot,
                            TaxAmt = row.TaxAmt,
                            Discount = row.Discount,
                            Shipping = row.Shipping,
                            TotAmt = row.TotAmt,
                            Remarks = row.Remarks,
                            CreatedBy = row.CreatedBy,
                            CreatedOn = row.CreatedOn,

                            // Payment Details----------
                            PaymentMethod = row.PaymentMethod,
                            PaymentStatus = row.PaymentStatus,
                            TxnId = row.TxnId,
                            GatewayTxnId = row.GatewayTxnId,
                            PaymentMode = row.PaymentMode,
                            BankRefNo = row.BankRefNo,
                            PaymentGateway = row.PaymentGateway,
                            PaymentRemark = row.PaymentRemark,
                            PayAmt = row.PayAmt,
                            OrderNo = row.OrderId,
                        };
                        transactionList.Add(existingTxn);
                    }

                    // Add transaction detail row
                    existingTxn.TProductDets.Add(new TProductDet
                    {
                        VchNo = row.VchNo,
                        ItemCode = row.ItemCode,
                        ItemName = row.ItemName,
                        IMCCode = row.IMCCode,
                        IMCName = row.IMCName,
                        Qty = row.Qty,
                        Price = row.Price,
                        Amount = row.Amount,
                        UnitCost = row.UnitCost,
                        Discount = row.ItemDiscount,
                        TaxPer = row.ItemTaxPer,
                        TaxAmt = row.ItemTaxAmt,
                        Image = imageList.Count > 0 ? imageList[0].FilePath : null
                    });
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message, Data = transactionList };
            }

            return new { Status = 1, Msg = "Success", Data = transactionList };
        }

        public Task<dynamic> DeleteTransactionDetAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<dynamic> GetTransactionsReportAsync(int vchType, string? startDate, string? endDate, string? customer, int? status)
         {
            var tran = new List<TransactionReportDto>();
            try
            {
                // Frontend se aayi string date ko DateTime? me convert karna safe way
                DateTime? parsedDate1 = null; DateTime? parsedDate2 = null;
                if (!string.IsNullOrEmpty(startDate))
                {
                    // Expected format: "12-May-2025" (dd-MMM-yyyy)
                    if (DateTime.TryParseExact(startDate, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                    {
                        parsedDate1 = dt;
                    }
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    // Expected format: "12-May-2025" (dd-MMM-yyyy)
                    if (DateTime.TryParseExact(endDate, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                    {
                        parsedDate2 = dt;
                    }
                }

                string sql = @"SELECT ISNULL(A.[OrderId], '') as OrderId, ISNULL(A.VchCode, 0) as VchCode, ISNULL(A.[VchNo], '') as InvNo, ISNULL(CONVERT(VARCHAR, A.[Date], 106), '') as InvDt, ISNULL(A.[MasterCode1], 0) as AccCode,CASE WHEN ISNULL(A.[MasterCode1], 0) = 101 THEN 'Symbiosis Canteen Buyer' ELSE ISNULL(M.[Name], '') END AS AccName,ISNULL(CASE WHEN P.[PaymentStatus] = 'SUCCESS' THEN 'PAID' ELSE 'UNPAID' END, '') as PaymentStatus, ISNULL(A.[D5], 0) AS Amount FROM RJTran1 A LEFT JOIN RJMaster1 M ON A.MasterCode1 = M.Code LEFT JOIN PaymentTransaction P ON A.VchCode = P.VchCode WHERE A.VchType = @vchType AND (@startDate IS NULL OR CONVERT(date, A.[Date]) >= @startDate) AND (@endDate IS NULL OR CONVERT(date, A.[Date]) <= @endDate) AND (@status IS NULL OR @status = 0 OR (@status = 1 AND P.PaymentStatus = 'SUCCESS') OR (@status = 2 AND (P.[PaymentStatus] IS NOT NULL AND P.[PaymentStatus] <> 'SUCCESS'))) Order By A.VchCode Desc ";
                tran = await _db.TransactionReportDtos.FromSqlRaw(sql,new SqlParameter("@vchType", vchType), new SqlParameter("startDate", (object)parsedDate1 ?? DBNull.Value), new SqlParameter("endDate", (object)parsedDate2 ?? DBNull.Value), new SqlParameter("@status", (object)status ?? DBNull.Value)).ToListAsync();

                if (tran != null && tran.Count > 0)
                {
                    var summaryResult = await GetInvoiceReportSummaryAsync(vchType, parsedDate1, parsedDate2, customer, status);
                    return new
                    {
                        Status = 1,
                        Msg = "Success",
                        Data = new
                        {
                            Transactions = tran,
                            Summary = summaryResult
                        }
                    };
                }
                else
                {
                    return new { Status = 0, Msg = "Data Not Found !!!"};
                }
            } 
            catch (Exception ex) 
            {
                return new { Status = 0, Msg = ex.Message.ToString() };
            }
        }

        public async Task<ReportSummaryDto?> GetInvoiceReportSummaryAsync(int vchType, DateTime? startDate, DateTime? endDate, string? customer, int? status)
        {
            try
            {
                string sql = @"SELECT ISNULL(CAST(SUM(A.D5) AS DECIMAL(18,2)), 0) AS TotalAmount, ISNULL(CAST(SUM(CASE WHEN P.PaymentStatus = 'SUCCESS' THEN A.D5 ELSE 0 END) AS DECIMAL(18,2)), 0) AS TotalPaid, ISNULL(CAST(SUM(CASE WHEN (P.PaymentStatus IS NULL OR P.PaymentStatus <> 'SUCCESS') THEN A.D5 ELSE 0 END) AS DECIMAL(18,2)), 0) AS TotalUnpaid, ISNULL(CAST(SUM(CASE WHEN (P.PaymentStatus IS NULL OR P.PaymentStatus <> 'SUCCESS') AND A.[Date] < GETDATE() THEN A.D5 ELSE 0 END) AS DECIMAL(18,2)), 0) AS Overdue FROM RJTran1 A LEFT JOIN PaymentTransaction P ON A.VchCode = P.VchCode LEFT JOIN RJMaster1 M ON A.MasterCode1 = M.Code WHERE A.VchType = @vchType AND (@startDate IS NULL OR CONVERT(date, A.[Date]) >= @startDate) AND (@endDate IS NULL OR CONVERT(date, A.[Date]) <= @endDate) AND (@status IS NULL OR @status = 0 OR (@status = 1 AND P.PaymentStatus = 'SUCCESS') OR (@status = 2 AND (P.PaymentStatus IS NOT NULL AND P.PaymentStatus <> 'SUCCESS')))";
                
                var result = await _db.ReportSummaryDtos.FromSqlRaw(sql, new SqlParameter("@vchType", vchType), new SqlParameter("@startDate", (object)startDate ?? DBNull.Value), new SqlParameter("@endDate", (object)endDate ?? DBNull.Value), new SqlParameter("@customer", (object)customer ?? DBNull.Value), new SqlParameter("@status", (object)status ?? DBNull.Value) ).FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                return null;
            }

        }

        //public async Task<dynamic> GetInvoiceReportSummaryAsync(int vchType, string? startDate, string? endDate, string? customer, int? status)
        //{
        //    try
        //    {
        //        DateTime? parsedDate1 = null; DateTime? parsedDate2 = null;
        //        if (!string.IsNullOrEmpty(startDate))
        //        {
        //            // Expected format: "12-May-2025" (dd-MMM-yyyy)
        //            if (DateTime.TryParseExact(startDate, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
        //            {
        //                parsedDate1 = dt;
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(endDate))
        //        {
        //            // Expected format: "12-May-2025" (dd-MMM-yyyy)
        //            if (DateTime.TryParseExact(endDate, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
        //            {
        //                parsedDate2 = dt;
        //            }
        //        }

        //        string sql = @"SELECT CAST(SUM(A.D5) AS DECIMAL(18,2)) AS TotalAmount, CAST(SUM(CASE WHEN P.PaymentStatus = 'SUCCESS' THEN A.D5 ELSE 0 END) AS DECIMAL(18,2)) AS TotalPaid, CAST(SUM(CASE WHEN (P.PaymentStatus IS NULL OR P.PaymentStatus <> 'SUCCESS') THEN A.D5 ELSE 0 END) AS DECIMAL(18,2)) AS TotalUnpaid, CAST(SUM(CASE WHEN (P.PaymentStatus IS NULL OR P.PaymentStatus <> 'SUCCESS') AND A.[Date] < GETDATE() THEN A.D5 ELSE 0 END) AS DECIMAL(18,2)) AS Overdue FROM RJTran1 A LEFT JOIN PaymentTransaction P ON A.VchCode = P.VchCode LEFT JOIN RJMaster1 M ON A.MasterCode1 = M.Code WHERE A.VchType = @vchType AND (@startDate IS NULL OR CONVERT(date, A.[Date]) >= @startDate) AND (@endDate IS NULL OR CONVERT(date, A.[Date]) <= @endDate) AND (@customer IS NULL OR M.Name = @customer) AND (@status IS NULL OR @status = 0 OR (@status = 1 AND P.PaymentStatus = 'SUCCESS') OR (@status = 2 AND (P.PaymentStatus IS NOT NULL AND P.PaymentStatus <> 'SUCCESS')))";

        //        var result = await _db.ReportSummaryDtos
        //            .FromSqlRaw(sql,
        //                new SqlParameter("@vchType", vchType),
        //                new SqlParameter("@startDate", (object)parsedDate1 ?? DBNull.Value),
        //                new SqlParameter("@endDate", (object)parsedDate2 ?? DBNull.Value),
        //                new SqlParameter("@customer", (object)customer ?? DBNull.Value),
        //                new SqlParameter("@status", (object)status ?? DBNull.Value)
        //            ).FirstOrDefaultAsync();

        //        return new { Status = 1, Msg = "Success", Data = result };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { Status = 0, Msg = ex.Message };
        //    }
        //}

        public async Task<dynamic> GetVchNo(int tranType, int vchType)
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

            string sql = $"SELECT * FROM RJVchConfig WHERE TranType = {tranType}";
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

        public async Task<dynamic> GetCardSummaryAsync()
        {
            try
            {
                string sql = $"SELECT CAST(ISNULL(COUNT(CASE WHEN A.VchType = 9 THEN 1 END), 0) as DECIMAL(18,2)) as TotalSale, CAST(ISNULL(COUNT(CASE WHEN A.VchType = 2 THEN 1 END), 0) as DECIMAL(18,2)) as TotalPurchase, CAST(ISNULL(COUNT(CASE WHEN A.VchType = 9 AND B.PaymentStatus = 'Success' THEN 1 END), 0) as DECIMAL(18,2)) as TotalPaid, CAST(ISNULL(COUNT(CASE WHEN A.VchType = 9 AND (B.PaymentStatus <> 'Success' OR B.PaymentStatus IS NULL) THEN 1 END), 0) as DECIMAL(18,2)) as TotalUnpaid FROM RJTran1 A LEFT JOIN PaymentTransaction B ON A.VchCode = B.VchCode";
                var dt1 = await _db.CardSummaryDtos.FromSqlRaw(sql).ToListAsync();

                if (dt1 == null)
                {
                    return new { Status = 0, Msg = "Dashboard card summary data not found!", Data = dt1 };
                }
                else
                {
                    return new { Status = 1, Msg = "Success", Data = dt1 };
                }
            }
            catch (Exception ex) 
            { 
                return new {Status = 0, Msg = ex.Message};
            }
        }

        public async Task<dynamic> GetSalesDayChartAsync(string range)
        {
            try
            {
                // Range Filter Logic (same as before)
                string dateFilter = range switch
                {
                    "1D" => "WHERE A.CreationTime >= CAST(GETDATE() AS DATE)",
                    "1W" => "WHERE A.CreationTime >= DATEADD(DAY, -7, GETDATE())",
                    "1M" => "WHERE A.CreationTime >= DATEADD(MONTH, -1, GETDATE())",
                    "3M" => "WHERE A.CreationTime >= DATEADD(MONTH, -3, GETDATE())",
                    "6M" => "WHERE A.CreationTime >= DATEADD(MONTH, -6, GETDATE())",
                    "1Y" => "WHERE A.CreationTime >= DATEADD(YEAR, -1, GETDATE())",
                    _ => ""
                };

                //string sql = $@" SELECT FORMAT(A.CreationTime, 'HH') AS HourLabel,CAST(SUM(CASE WHEN A.VchType = 9 THEN ISNULL(A.D5, 0) ELSE 0 END) as DECIMAL(18,2)) AS Sales, CAST(SUM(CASE WHEN A.VchType = 2 THEN ISNULL(A.D5, 0) ELSE 0 END) as DECIMAL(18,2)) AS Purchase FROM RJTran1 A {dateFilter} GROUP BY FORMAT(A.CreationTime, 'HH') ORDER BY HourLabel";
                string sql = $@" SELECT FORMAT(A.CreationTime, 'hh tt') AS HourLabel,CAST(SUM(CASE WHEN A.VchType = 9 THEN ISNULL(A.D5, 0) ELSE 0 END) as DECIMAL(18,2)) AS Sales, CAST(SUM(CASE WHEN A.VchType = 2 THEN ISNULL(A.D5, 0) ELSE 0 END) as DECIMAL(18,2)) AS Purchase FROM RJTran1 A {dateFilter} GROUP BY FORMAT(A.CreationTime, 'hh tt') ORDER BY MIN(A.CreationTime)";

                var dt = await _db.SalesPurchaseChartDtos.FromSqlRaw(sql).ToListAsync();

                if (dt == null || !dt.Any())
                {
                    return new { Status = 0, Msg = "No chart data found!", Data = new { chart = new { } } };
                }

                var chart = new
                {
                    series = new[]
                    {
                        new { name = "Sales", data = dt.Select(x => x.Sales).ToList() },
                        new { name = "Purchase", data = dt.Select(x => x.Purchase).ToList() }
                    },
                    xaxis = new
                    {
                        //categories = dt.Select(x => $"{x.HourLabel}:00").ToList()
                        categories = dt.Select(x => x.HourLabel).ToList()
                    }
                };

                return new { Status = 1, Msg = "Success", Data = new { chart } };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message };
            }
        }

        public async Task<dynamic> GetRecentTransactionsAsync()
        {
            var list = new List<RecentTransaction>();
            try
            {
                string sql = string.Empty;
                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";


                sql = $"select TOP 10 ISNULL(B.[MasterCode1], 0) as ItemCode, ISNULL(CONVERT(VARCHAR,A.CreationTime, 100), '') as Date, ISNULL(M.[Name], '') as Items, ISNULL(C.[PaymentStatus], '') as PaymentStatus, ISNULL(C.[PaymentMode], '') as PaymentMode, ISNULL(C.[TxnId], '') as TxnId, CAST(ISNULL(A.[D5], 0) as DECIMAL(18,2)) as Amount, '' as Image From RJTran1 A INNER JOIN RJTran2 B ON A.VchCode = B.VchCode LEFT JOIN PaymentTransaction C ON A.VchCode = C.VchCode INNER JOIN RJMaster1 M ON B.MasterCode1 = M.Code Where A.[VchType] = 9 And A.CreationTime >= DATEADD(MINUTE, -15, GETDATE())  Order By A.VchCode Desc";
                list = await _db.RecentTransactions.FromSqlRaw(sql).ToListAsync();

                if (list == null || list.Count == 0)
                {
                    sql = "select TOP 10 ISNULL(B.[MasterCode1], 0) as ItemCode, ISNULL(CONVERT(VARCHAR,A.Date, 106), '') as Date, ISNULL(M.[Name], '') as Items, ISNULL(C.[PaymentStatus], '') as PaymentStatus, ISNULL(C.[PaymentMode], '') as PaymentMode, ISNULL(C.[TxnId], '') as TxnId, CAST(ISNULL(A.[D5], 0) as DECIMAL(18,2)) as Amount, '' as Image From RJTran1 A INNER JOIN RJTran2 B ON A.VchCode = B.VchCode LEFT JOIN PaymentTransaction C ON A.VchCode = C.VchCode INNER JOIN RJMaster1 M ON B.MasterCode1 = M.Code Where A.[VchType] = 9 Order By A.VchCode Desc";
                    list = await _db.RecentTransactions.FromSqlRaw(sql).ToListAsync();

                    if (list == null || list.Count == 0)
                    {
                        return new { Status = 0, Msg = "Data not found!!!", Data = list };
                    }
                }

                // 🔹 3️⃣ Attach images (safe async)
                foreach (var row in list)
                {
                    try
                    {
                        var imageList = await MapProductWithFullImagePathsAsync(row.ItemCode, baseUrl);

                        if (imageList != null && imageList.Count > 0)
                            row.Image = imageList[0]?.FilePath;
                        else
                            row.Image = null;
                    }
                    catch
                    {
                        // Prevent crash if any image fetch fails
                        row.Image = null;
                    }
                }
            }
            catch (Exception ex) 
            { 
                return new { Status = 0, Msg = ex.Message };
            }
            return new { Status = 1, Msg = "Success" , Data = list };
        }

        public async Task<int> GetOrderIdByTxnId(string txnId)
        {
            try
            {
                string sql = @"SELECT TOP 1 VchCode as Value FROM PaymentTransaction WHERE TxnId = @txnId";

                var result = await _db.Database
                    .SqlQueryRaw<int>(sql, new SqlParameter("@txnId", txnId))
                    .FirstOrDefaultAsync();

                return result;
            }
            catch
            {
                return 0;
            }
        }

    }
}
