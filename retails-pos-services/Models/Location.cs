using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Models
{
    public class Country
    {
        public string Name { get; set; }
    }

    public class State
    {
        public string Country { get; set; }
    }

    public class City
    {
        public string Country { get; set; }
        public string State { get; set; }
    }

    public class CountryWithCode
    {
        public string? Name { get; set; }
        public string? Iso2 { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }
    }

    public class StateWithCode
    {
        public string Name { get; set; }
        public string State_code { get; set; }
    }

    public class CityWithCode
    {
        public string Name { get; set; }
    }

    
    public class SaveLocationRequest
    {
        public string? Name { get; set; }
        public string? Iso2 { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }

    [Table("RJMaster1")]
    public class RJMaster1
    {
        public int Code { get; set; }
        public string? Name { get; set; }
        public string? Alias { get; set; }
        public int? ParentGrp { get; set; }
        public double? D1 { get; set; } // Latitude
        public double? D2 { get; set; } // Longitude
    }

}
