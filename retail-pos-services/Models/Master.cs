using System;
using MimeKit.Encodings;
using System.ComponentModel.DataAnnotations.Schema;

namespace Master.Models
{
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
        public decimal D1 { get; set; } = 0;
        public decimal D2 { get; set; } = 0;
        public decimal D3 { get; set; } = 0;
        public decimal D4 { get; set; } = 0;
        public decimal D5 { get; set; } = 0;
        public string? Remark { get; set; } = string.Empty;
        public bool BlockedMaster { get; set; } = false;
        public bool DeactiveMaster { get; set; } = false;
        public string? Image { get; set; } = string.Empty;
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? CreationTime { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModificationTime { get; set; } = DateTime.Now;
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