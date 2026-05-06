using System;
using MimeKit.Encodings;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Master.Models
{

    public partial class MasterList
    {
        public int Code { get; set; } = 0;
        public string? Name { get; set; } = string.Empty;
        public int ParentGrpCode { get; set; } = 0;
    }

    public class MasterLookup
    {
        public int Code { get; set; }
        public string Name { get; set; } = "";
    }

    public class SaveProductRequest
    {
        public Master1? Master { get; set; }

        public List<VariantDto>? Variants { get; set; }

        public CustomFieldsDto? CustomFields { get; set; }

        public List<string>? Images { get; set; }
    }

    public class MasterFieldConfig
    {
        public int MasterType { get; set; }
        public string Field { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    [Table("Master1")]
    public partial class Master1
    {
        public int Code { get; set; } = 0;
        public short MasterType { get; set; } = 0;
        public string? Name { get; set; } = string.Empty;
        public string? Alias { get; set; } = string.Empty;
        public string? PrintName { get; set; } = string.Empty;
        public int ParentGrp { get; set; } = 0;
        public string? HSNCode { get; set; } = string.Empty;
        public int CM1 { get; set; } = 0;
        public int CM2 { get; set; } = 0;
        public int CM3 { get; set; } = 0;
        public int CM4 { get; set; } = 0;
        public int CM5 { get; set; } = 0;
        public int CM6 { get; set; } = 0;
        public int CM7 { get; set; } = 0;
        public int CM8 { get; set; } = 0;
        public int CM9 { get; set; } = 0;
        public int CM10 { get; set; } = 0;
        public decimal D1 { get; set; } = 0;
        public decimal D2 { get; set; } = 0;
        public decimal D3 { get; set; } = 0;
        public decimal D4 { get; set; } = 0;
        public decimal D5 { get; set; } = 0;
        public string? C1 { get; set; } = string.Empty;
        public string? C2 { get; set; } = string.Empty;
        public string? C3 { get; set; } = string.Empty;
        public string? C4 { get; set; } = string.Empty;
        public string? C5 { get; set; } = string.Empty;
        public string? Remark { get; set; } = string.Empty;
        public bool BlockedMaster { get; set; } = false;
        public bool DeactiveMaster { get; set; } = true; // 1 Active 2 Deactive
        public string? Image { get; set; } = string.Empty;
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? CreationTime { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModificationTime { get; set; } = DateTime.Now;
    }

    public partial class Master2
    {
        [JsonIgnore]
        public short MasterType { get; set; } = 0;
        public int Code { get; set; } = 0;
        public string? Name { get; set; } = string.Empty;
        public string? Alias { get; set; } = string.Empty;
        public string? PrintName { get; set; } = string.Empty;
        public int ParentGrpCode { get; set; } = 0;
        public string ParentGrpName { get; set; } = string.Empty;
        public string? HSNCode { get; set; } = string.Empty;

        [JsonIgnore]
        public int CM1 { get; set; } = 0;

        [JsonIgnore]
        public int CM2 { get; set; } = 0;

        [JsonIgnore]
        public int CM3 { get; set; } = 0;

        [JsonIgnore]
        public int CM4 { get; set; } = 0;

        [JsonIgnore]
        public int CM5 { get; set; } = 0;

        [JsonIgnore]
        public int CM6 { get; set; } = 0;

        [JsonIgnore]
        public int CM7 { get; set; } = 0;

        [JsonIgnore]
        public int CM8 { get; set; } = 0;

        [JsonIgnore]
        public int CM9 { get; set; } = 0;

        [JsonIgnore]
        public int CM10 { get; set; } = 0;
        public decimal D1 { get; set; } = 0;
        public decimal D2 { get; set; } = 0;
        public decimal D3 { get; set; } = 0;
        public decimal D4 { get; set; } = 0;
        public decimal D5 { get; set; } = 0;
        public string C1 { get; set; } = string.Empty;
        public string C2 { get; set; } = string.Empty;
        public string C3 { get; set; } = string.Empty;
        public string C4 { get; set; } = string.Empty;
        public string C5 { get; set; } = string.Empty;
        public string Values { get; set; } = string.Empty;
        public string? Remark { get; set; } = string.Empty;
        public int NoOfProducts { get; set; } = 0;
        public bool Blocked { get; set; } = false;
        public bool Deactive { get; set; } = false; // 1 Active 2 Deactive
        public string? Image { get; set; } = string.Empty;
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; } = DateTime.Now;
        public Dictionary<string, object> DynamicFields { get; set; } = new();
    }

    public class ProductDto
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public string? PrintName { get; set; }
        public int ParentGrpCode { get; set; }
        public string? ParentGrpName { get; set; }
        public string? HSNCode { get; set; }

        // 🔹 CM Fields + NAMING
        public int StoreId { get; set; }
        public string? StoreName { get; set; }

        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }

        public int BrandId { get; set; }
        public string? BrandName { get; set; }

        public int UnitId { get; set; }
        public string? UnitName { get; set; }

        public int SellingTypeId { get; set; }
        public string? SellingTypeName { get; set; }

        public int BarcodeSymbolId { get; set; }
        public string? BarcodeSymbolName { get; set; }

        public int TaxTypeId { get; set; }
        public string? TaxTypeName { get; set; }

        public int DiscountTypeId { get; set; }
        public string? DiscountTypeName { get; set; }
        public int ProductType { get; set; } // 1=single,2=variant

        // 🔹 C Fields
        public string? C1 { get; set; }  // SKU
        public string? C2 { get; set; }
        public string? C3 { get; set; }
        public string? C4 { get; set; }
        public string? C5 { get; set; }

        // 🔹 D Fields
        public decimal D1 { get; set; }
        public decimal D2 { get; set; }
        public decimal D3 { get; set; }
        public decimal D4 { get; set; }
        public decimal D5 { get; set; }

        public string? Remark { get; set; }

        // 🔥 JSON Fields (IMPORTANT)
        public string? Variants { get; set; }
        public string? Images { get; set; }
        public string? CustomFields { get; set; }
    }

    public class ItemSaveDto
    {
        public Master1? Item { get; set; }

        public List<ItemVariantDto>? Variants { get; set; }
    }

    public class ItemVariantDto
    {
        public string VariantName { get; set; }

        public decimal Price { get; set; }

        public bool IsDefault { get; set; }
    }

    public class ItemMaster
    {
        public int Code { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public decimal NetPrice { get; set; }

        public int CategoryId { get; set; }

        public int TaxId { get; set; }

        public string? Image { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreationTime { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModificationTime { get; set; }

        public bool IsDeleted { get; set; }

        public virtual List<ItemVariant>? Variants { get; set; }
    }

    public class ItemVariant
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public string VariantName { get; set; }

        public decimal Price { get; set; }

        public bool IsDefault { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreationTime { get; set; }

        public virtual ItemMaster Item { get; set; }
    }

    public class VariantDto
    {
        public string? Attribute { get; set; }
        public string? Value { get; set; }
        public string SKU { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }

    public class CustomFieldsDto
    {
        public string? Warranty { get; set; }
        public string? Manufacturer { get; set; }
        public DateTime? ManufacturedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class ImageDto
    {
        public string? ImagePath { get; set; }
    }

    public class Addon
    {
        public int Code { get; set; }

        public int ItemCode { get; set; }   // mapping ke liye

        public string? Name { get; set; }

        public decimal Price { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        public bool IsActive { get; set; } = true;

        public string? CreatedBy { get; set; }

        public string? ModifiedBy { get; set; }
    }

    public class AddonList
    {
        public int Code { get; set; }

        public int ItemCode { get; set; }

        public string ItemName { get; set; }

        public int AddonCode { get; set; }

        public string AddonName { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public bool IsActive { get; set; }
    }

    public class RestaurantTable
    {
        public int Code { get; set; }

        public string Name { get; set; }

        public string Floor { get; set; }

        public int TableSize { get; set; }

        public int NoOfGuests { get; set; }

        public int Status { get; set; }

        public string Remark { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }
    }
}