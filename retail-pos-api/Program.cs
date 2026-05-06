using EasebuzzPayment.Services.V1;
using EasebuzzPayment.Services.V2;
using General.Services.Repository;
using LocationRepository.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Payments.RealTime;
using Pos.Services.Repository;
using QSRAPIServices.Models;
using Razorpay.Models;
using RetailPos.Licensing;
using RetailPosContext.DBContext;
using RetailPosEmail.Services.Email;
using RetailPosRepository.Services.Repository;
using RetailPosToken.Services.Token;
using System.Text;

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
builder.Services.AddDbContext<RetailPosDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr")));

// Custom Services
//builder.Services.AddTransient<IRepository, Repository>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IPosRepository, PosRepository>();
builder.Services.AddScoped<IGeneralRepository, GeneralRepository>();


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
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

// LICENSE CHECK – SABSE PEHLE
//app.UseMiddleware<LicenseMiddleware>();

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

// Static files from wwwroot 
app.UseStaticFiles();

// SignalR Hubs
app.MapHub<PaymentsHub>("/hubs/payments");

// Controllers
app.MapControllers();
app.Run();
