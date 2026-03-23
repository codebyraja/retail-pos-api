using Microsoft.EntityFrameworkCore;
using QSRAPIServices.Models;
using QSRTokenService.Services.Token;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QSRTokenServices.Services.Token;
using QSREmailServices.Services.Email;
using QSRDBContextServices.DBContext;
using QSRRepositoryServices.Services.Repository;
using Microsoft.Extensions.FileProviders;
using Razorpay.Models;
using EasebuzzPayment.Services.V2;
using EasebuzzPayment.Services.V1;
using LocationRepository.Services;
using Payments.RealTime;
using QsrAdminWebApi.Licensing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddControllers().AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.PropertyNamingPolicy = null;
//});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// JWT Auth Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];

// Add services to the container.
//builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Database Context
builder.Services.AddDbContext<QSRDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr")));

// Custom Services
//builder.Services.AddTransient<IRepository, Repository>();
builder.Services.AddScoped<IRepository, Repository>();

// Razorpay Payment Services
builder.Services.Configure<RazorpayOptions>(builder.Configuration.GetSection("Razorpay"));
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Token Service
builder.Services.AddTransient<ITokenService, TokenService>();

// Easebuzz Payment Services  
builder.Services.AddScoped<IEasebuzzRepositoryV1, EasebuzzRepositoryV1>();
builder.Services.AddScoped<IEasebuzzRepositoryV2, EasebuzzRepositoryV2>();

// Location Repository Services
builder.Services.AddHttpClient<ILocationService, LocationService>();
builder.Services.AddHttpClient();

// LICENSE SERVICES
builder.Services.AddMemoryCache();
builder.Services.AddScoped<LicenseService>();

// Email Repository Services
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailHelper>();

// SignalR (REALTIME)
builder.Services.AddSignalR();

// CORS Policy
//builder.Services.AddCors(opt =>
//{
//    opt.AddPolicy("CorsPolicy", policy =>
//    {
//        //policy.AllowAnyOrigin()
//        policy.WithOrigins("http://localhost:3000")
//        //policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});


builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://103.172.92.157:2026", "http://103.172.92.157:12001", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();

// LICENSE CHECK – SABSE PEHLE
app.UseMiddleware<LicenseMiddleware>();

// Dev tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS
app.UseCors("CorsPolicy");

// WebSockets (SignalR)
app.UseWebSockets();

// Security
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// // Static files from wwwroot 
app.UseStaticFiles();

// SignalR Hubs
app.MapHub<PaymentsHub>("/hubs/payments");

// Controllers
app.MapControllers();

app.Run();
