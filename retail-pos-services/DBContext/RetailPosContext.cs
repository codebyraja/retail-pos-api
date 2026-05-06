using Easebuzz.Models;
using Location.Models;
using Master.Models;
using Microsoft.EntityFrameworkCore;
using Pos.Models;
using QSRAPIServices.Models;
using Razorpay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RetailPosContext.DBContext;

public partial class RetailPosDBContext : DbContext
{
    public RetailPosDBContext(DbContextOptions<RetailPosDBContext> options) : base(options){ }
    public virtual DbSet<Response> Responses { get; set; }
    public virtual DbSet<TempCatalog> TempCatalogs { get; set; }
    public virtual DbSet<PosProductDto> PosProductDtos { get; set; }
    public virtual DbSet<PosCategoryDto> PosCategoryDtos { get; set; }

    //public DbSet<SyncLocationsEntity> Locations { get; set; }
    public virtual DbSet<MasterList> MasterLists { get; set; }
    public virtual DbSet<MasterFieldConfig> MasterFieldConfigs { get; set; }
    public virtual DbSet<MasterLookup> MasterLookups { get; set; }
    public virtual DbSet<Master1> Masters1 { get; set; }
    public virtual DbSet<Master2> Masters2 { get; set; }
    public virtual DbSet<ProductDto> ProductDtos { get; set; }

    public virtual DbSet<Addon> Addons { get; set; }
    public virtual DbSet<AddonList> AddonLists { get; set; }
    public virtual DbSet<RestaurantTable> RestaurantTables { get; set; }
    public virtual DbSet<ResponseNew> ResponseNew { get; set; }
    public virtual DbSet<ApiResponse> ApiResponses { get; set; }
    public virtual DbSet<LoginResult> LoginResults { get; set; }
    public virtual DbSet<UnknowList> UnknowLists { get; set; }
    public virtual DbSet<VchConfig> VchConfigs { get; set; }
    public virtual DbSet<AutoVchNo> AutoVchNos { get; set; }
    public virtual DbSet<SignupRequest> SignupRequests { get; set; }
    public virtual DbSet<LoginRequest> LoginRequests { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<UserModel> Users { get; set; }
    public virtual DbSet<ResetPasswordRequestWithOtp> ResetPasswordRequestWithOtps { get; set; }
    public virtual DbSet<SaveMasterRequest> SaveMasterRequests { get; set; }    
    public virtual DbSet<SaveProductMasterRequest> SaveProductMasterRequests { get; set; }
    public virtual DbSet<GetMasterRequest> GetMasterRequests { get; set; }  
    public virtual DbSet<GetProductMasterRequest> GetProductMasterRequests { get; set; }
    public virtual DbSet<ProductImage> ProductImages { get; set; }
    public virtual DbSet<ProductImageDto> ProductImageDtos { get; set; }
    public virtual DbSet<SaveUserMasterRequest> SaveUserMasterRequests { get; set; }
    public virtual DbSet<GetUserMasterDetailRequest> GetUserMasterDetailRequests { get; set; }
    public virtual DbSet<ImportProductRequest> ImportProductRequests { get; set; }
    public virtual DbSet<ImportStockRequest> ImportStockRequests { get; set; }
    public virtual DbSet<UpiTransaction> UpiTransactions { get; set; }
    public virtual DbSet<Feedback> Feedbacks { get; set; }
    public virtual DbSet<Razorpay.Models.Razorpay> Payments { get; set; }
    public virtual DbSet<RawCategoryProduct> RawCategoryProducts { get; set; }
    public virtual DbSet<RawCategorie> RawCategories { get; set; }
    public virtual DbSet<RawProduct> RawProducts { get; set; }
    public virtual DbSet<SvCustomerDet> SvCustomerDets { get; set; }
    public virtual DbSet<GetCustomerDet> GetCustomerDets { get; set; }
    public virtual DbSet<TransactionReportDto> TransactionReportDtos { get; set; }

    // Location related entities
    public virtual DbSet<SaveLocationRequest> SaveLocationRequests { get; set; }
    public virtual DbSet<RJMaster1> RJMasters { get; set; }

    // Transaction related entities
    public virtual DbSet<RawTProductData> RawTProductDatas { get; set; }
    public virtual DbSet<ReportSummaryDto> ReportSummaryDtos { get; set; }
    public virtual DbSet<CardSummaryDto> CardSummaryDtos { get; set; }
    public virtual DbSet<SalesPurchaseChartDto> SalesPurchaseChartDtos { get; set; }
    public virtual DbSet<RecentTransaction> RecentTransactions { get; set; }

    // Easebuzz related entities
    public virtual DbSet<EasebuzzWebhookRequest> EasebuzzWebhookRequests { get; set; }
    public virtual DbSet<PaymentStatusDto> PaymentStatusDtos { get; set; }

    //Mobile apk api entities
    public virtual DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Response>(entity => entity.HasNoKey());
        modelBuilder.Entity<Response>(entity => entity.HasNoKey());
        modelBuilder.Entity<MasterFieldConfig>(entity => entity.HasNoKey());
        modelBuilder.Entity<MasterLookup>(entity => entity.HasNoKey());
        modelBuilder.Entity<MasterList>(entity => entity.HasNoKey());
        //modelBuilder.Entity<Master1>().HasKey(x => x.Code);
        modelBuilder.Entity<Master1>(entity => entity.HasNoKey());
        modelBuilder.Entity<Master2>(entity => entity.HasNoKey());
        modelBuilder.Entity<ProductDto>(entity => entity.HasNoKey());


        modelBuilder.Entity<Addon>(entity => entity.HasNoKey());
        modelBuilder.Entity<AddonList>(entity => entity.HasNoKey());
        modelBuilder.Entity<RestaurantTable>(entity => entity.HasNoKey());

        modelBuilder.Entity<ResponseNew>(entity => entity.HasNoKey());
        modelBuilder.Entity<ApiResponse>(entity => entity.HasNoKey());
        modelBuilder.Entity<LoginResult>(entity => entity.HasNoKey());
        modelBuilder.Entity<UnknowList>(entity => entity.HasNoKey());
        modelBuilder.Entity<VchConfig>(entity => entity.HasNoKey());
        modelBuilder.Entity<AutoVchNo>(entity => entity.HasNoKey());
        modelBuilder.Entity<SignupRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<LoginRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<ResetPasswordRequestWithOtp>(entity => entity.HasNoKey());
        modelBuilder.Entity<SaveMasterRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<SaveProductMasterRequest>(entity => entity.HasNoKey()); 
        modelBuilder.Entity<GetMasterRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<GetProductMasterRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<ProductImageDto>(entity => entity.HasNoKey());
        modelBuilder.Entity<SaveUserMasterRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<GetUserMasterDetailRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<ImportProductRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<ImportStockRequest>(entity => entity.HasNoKey());
        //modelBuilder.Entity<UpiTransaction>(entity => entity.HasNoKey());
        modelBuilder.Entity<RawCategoryProduct>(entity=> entity.HasNoKey());
        modelBuilder.Entity<RawCategorie>(entity => entity.HasNoKey());
        modelBuilder.Entity<RawProduct>(entity => entity.HasNoKey());
        modelBuilder.Entity<SaveLocationRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<RJMaster1>(entity => entity.HasNoKey());
        modelBuilder.Entity<SvCustomerDet>(entity=> entity.HasNoKey());
        modelBuilder.Entity<GetCustomerDet>(entity=> entity.HasNoKey());
        modelBuilder.Entity<RawTProductData>(entity=> entity.HasNoKey());
        modelBuilder.Entity<EasebuzzWebhookRequest>(entity => entity.HasNoKey());
        modelBuilder.Entity<PaymentStatusDto>(entity => entity.HasNoKey());
        modelBuilder.Entity<TransactionReportDto>(entity=> entity.HasNoKey());
        modelBuilder.Entity<ReportSummaryDto>(entity => entity.HasNoKey());
        modelBuilder.Entity<CardSummaryDto>(entity=> entity.HasNoKey());
        modelBuilder.Entity<SalesPurchaseChartDto>(entity=> entity.HasNoKey());
        modelBuilder.Entity<RecentTransaction>(entity=>  entity.HasNoKey());
        modelBuilder.Entity<Category>(entity => entity.HasNoKey());
        modelBuilder.Entity<PosProductDto>(entity => entity.HasNoKey());
        modelBuilder.Entity<PosCategoryDto>(entity => entity.HasNoKey());
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

}

