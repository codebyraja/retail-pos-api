using Master.Models;

public static class MasterFieldConfigProvider
{
    private static readonly List<MasterFieldConfig> _config = new()
    {
        new() { MasterType = 2, Field = "CM7", Label = "City" },
        new() { MasterType = 2, Field = "CM8", Label = "State" },
        new() { MasterType = 2, Field = "CM9", Label = "Country" },

        new() { MasterType = 6, Field = "CM1", Label = "Store" },
        new() { MasterType = 6, Field = "CM2", Label = "Warehouse" },
        new() { MasterType = 6, Field = "CM3", Label = "Brand" },
        new() { MasterType = 6, Field = "CM4", Label = "Unit" },
        new() { MasterType = 6, Field = "CM5", Label = "Tax Type" },

        new() { MasterType = 12, Field = "CM7", Label = "City" },
        new() { MasterType = 12, Field = "CM8", Label = "State" },
        new() { MasterType = 12, Field = "CM9", Label = "Country" }
    };

    public static List<MasterFieldConfig> GetFieldConfig(int masterType)
    {
        return _config.Where(x => x.MasterType == masterType).ToList();
    }
}