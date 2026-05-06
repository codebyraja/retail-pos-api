using Master.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Models
{

    [Keyless]
    public class TempCatalog
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryImage { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }

        public decimal Price { get; set; }
        public decimal Stock { get; set; }
    }

    public class CatalogDto
    {
        public List<PosCategoryDto> Categories { get; set; }
    }

    public class PosCategoryDto
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }

        public List<ProductDto> Products { get; set; }
    }

    public class PosProductDto
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }

        public int Stock { get; set; }
        public string? SKU { get; set; }

        // Future ready fields
        public bool IsVariant { get; set; }
    }
}
