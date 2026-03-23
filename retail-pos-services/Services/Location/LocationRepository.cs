using Location.Models;
using Microsoft.EntityFrameworkCore;
using QSRAPIServices.Models;
using QSRHelperApiServices.Helper;
using RetailPosContext.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LocationRepository.Services
{
    public class LocationService : ILocationService
    {
        private readonly RetailPosDBContext _dbContext;
        private readonly HttpClient _httpClient;

        public LocationService(RetailPosDBContext dbContext, HttpClient httpClient)
        {
            _dbContext = dbContext;
            _httpClient = httpClient;
        }

        //public async Task<List<string>> GetCountriesAsync()
        //{
        //    try
        //    {
        //        var response = await _httpClient.GetAsync("https://countriesnow.space/api/v0.1/countries/positions");
        //        response.EnsureSuccessStatusCode();

        //        var json = await response.Content.ReadAsStringAsync();
        //        //var countries = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);

        //        var doc = JsonDocument.Parse(json);
        //        var countries = doc.RootElement.GetProperty("data").EnumerateArray().Select(c => c.GetProperty("name").GetString()).ToList();

        //        return countries ?? new List<string>();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error fetching countries: {ex.Message}");
        //        return new List<string>();
        //    }
        //}
        //public async Task<List<string>> GetCitiesAsync(string country, string state)
        //{
        //    try
        //    {
        //        var requestObj = new { country, state };
        //        var jsonContent = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");

        //        var response = await _httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/state/cities", jsonContent);
        //        response.EnsureSuccessStatusCode();
        //        var json = await response.Content.ReadAsStringAsync();

        //        var doc = JsonDocument.Parse(json);
        //        var cities = doc.RootElement.GetProperty("data").EnumerateArray().Select(c => c.GetString()).ToList();
        //        return cities ?? new List<string>();

        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        Console.WriteLine($"Error fetching cities: {ex.Message}");
        //        return new List<string>();
        //    }
        //}
        //public async Task<List<string>> GetStatesAsync(string country)
        //{
        //    try
        //    {
        //        var requestObj = new { country };
        //        var jsonContent = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");

        //        var response = await _httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/states", jsonContent);

        //        var json = await response.Content.ReadAsStringAsync();
        //        var doc = JsonDocument.Parse(json);

        //        var states = doc.RootElement.GetProperty("data").GetProperty("states").EnumerateArray().Select(s => s.GetProperty("name").GetString()).ToList();

        //        return states ?? new List<string>();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        Console.WriteLine($"Error fetching states: {ex.Message}");
        //        return new List<string>();
        //    }
        //}

        public async Task<dynamic> GetCountriesWithCodeAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://countriesnow.space/api/v0.1/countries/positions");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var countriesWithCode = doc.RootElement.GetProperty("data").EnumerateArray()
                    .Select(c => new CountryWithCode
                    {
                        Name = c.GetProperty("name").GetString(),
                        Iso2 = c.GetProperty("iso2").GetString(),
                        Long = Helper.ParseToDouble(c.GetProperty("long")),
                        Lat = Helper.ParseToDouble(c.GetProperty("lat"))
                    })
                    .ToList();

                return new { Status = 1, Msg = doc.RootElement.GetProperty("msg"), Data = countriesWithCode };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching countries with code: {ex.Message}");
                return new { Status = 0, Msg = ex.Message, Data = new List<CountryWithCode>() };
            }
        }
        public async Task<dynamic> GetStateWithCodeAsync(string country)
        {
            try
            {
                var requestObj = new { country };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/states", jsonContent);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var statesWithCode = doc.RootElement.GetProperty("data").GetProperty("states").EnumerateArray()
                    .Select(s => new StateWithCode
                    {
                        Name = s.GetProperty("name").GetString(),
                        State_code = s.GetProperty("state_code").GetString()
                    }).ToList();

                return new { Status = 1, Msg = doc.RootElement.GetProperty("msg"), Data = statesWithCode };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching states with code");
                return new { Status = 0, Msg = ex.Message.ToString(), Data = new List<StateWithCode>() };
            }
        }
        public async Task<dynamic> GetCitiesWithCodeAsync(string country, string state)
        {
            try
            {
                var requestObj = new { country, state };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestObj), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/state/cities", jsonContent);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var citiesWithCode = doc.RootElement.GetProperty("data").EnumerateArray()
                    .Select(c => new CityWithCode
                    {
                        Name = c.GetString(),
                    }).ToList();

                return new { Status = 1, Msg = doc.RootElement.GetProperty("msg"), Data = citiesWithCode };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching cities with code");
                return new { Status = 0, Msg = ex.Message.ToString(), Data = new List<CityWithCode>() };
            }
        }
        public async Task<dynamic> SeedCountriesToDatabaseAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://countriesnow.space/api/v0.1/countries/positions");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                var countries = doc.RootElement.GetProperty("data").EnumerateArray()
                    .Select(c => new SaveLocationRequest
                    {
                        Name = c.GetProperty("name").GetString(),
                        Iso2 = c.GetProperty("iso2").GetString(),
                        Longitude = Helper.ParseToDouble(c.GetProperty("long")),
                        Latitude = Helper.ParseToDouble(c.GetProperty("lat"))
                    })
                    .ToList();

                //foreach (var item in countries)
                //{
                //    // Check if this country already exists
                //    var exists = await _dbContext.RJMasters.AnyAsync(x => x.Name == item.Name && x.ParentGrp == null);

                //    if (!exists)
                //    {
                //        var entity = new RJMaster1
                //        {
                //            Name = item.Name,
                //            Alias = item.Iso2,
                //            ParentGrp = null,       // No parent for countries
                //            D1 = item.Latitude,
                //            D2 = item.Longitude
                //        };

                //        _dbContext.RJMasters.Add(entity);
                //    }
                //    else
                //    {
                //        // Optional update logic
                //        var existing = await _dbContext.RJMasters.FirstOrDefaultAsync(x => x.Name == item.Name && x.ParentGrp == null);

                //        if (existing != null)
                //        {
                //            existing.Alias = item.Iso2;
                //            existing.D1 = item.Latitude;
                //            existing.D2 = item.Longitude;
                //        }
                //    }
                //}
                //await _dbContext.SaveChangesAsync();

                foreach (var item in countries)
                {
                    // Check if this country already exists using raw SQL
                    var existing = await _dbContext.RJMasters.FromSqlRaw("SELECT * FROM RJMaster1 WHERE Name = {0} AND (ParentGrp IS NULL OR ParentGrp = 0)", item.Name).AsNoTracking().FirstOrDefaultAsync();

                    if (existing == null)
                    {
                        // Insert new country using raw SQL
                        await _dbContext.Database.ExecuteSqlRawAsync(@"INSERT INTO RJMaster1 ([Name], [Alias], [D1], [D2], [ParentGrp], [MasterType], [PrintName], [CreatedBy], [CreationTime]) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})", item.Name, item.Iso2, item.Latitude, item.Longitude, 0, 55, "", "Raja", DateTime.UtcNow);
                    }
                    else
                    {
                        // Update existing country using raw SQL
                        await _dbContext.Database.ExecuteSqlRawAsync("UPDATE RJMaster1 SET [Alias] = {1}, [D1] = {2}, [D2] = {3}, [PrintName] = {4}, [ModifiedBy] = {5}, [ModificationTime] = {6} WHERE [Name] = {0} AND ([ParentGrp] IS NULL OR [ParentGrp] = 0)", item.Name, item.Iso2, item.Latitude, item.Longitude,"", "Raja", DateTime.UtcNow);
                    }
                }

                return new { Status = 1, Msg = "Countries seeded successfully", Data = countries };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding countries to database: {ex.Message}");
                return new { Status = 0, Msg = ex.Message, Data = new List<SaveLocationRequest>() };
            }
        }
        public async Task<dynamic> SeedStatesToDatabaseAsync()
        {
            try
            {
                var countries = await _dbContext.RJMasters.FromSqlRaw("SELECT * FROM RJMaster1 WHERE (ParentGrp = 0 OR ParentGrp IS NULL) AND MasterType = 55").AsNoTracking().ToListAsync();

                foreach (var country in countries)
                {
                    var requestBody = new { country = country.Name };
                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/states", content);
                    if (!response.IsSuccessStatusCode) continue;

                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);

                    var states = doc.RootElement.GetProperty("data").GetProperty("states").EnumerateArray().Select(s => s.GetProperty("name").GetString()).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();

                    foreach (var stateName in states)
                    {
                        var exists = await _dbContext.RJMasters.FromSqlRaw("SELECT * FROM RJMaster1 WHERE Name = {0} AND ParentGrp = {1}", stateName, country?.Code).AsNoTracking().FirstOrDefaultAsync();

                        if (exists == null)
                        {
                            await _dbContext.Database.ExecuteSqlRawAsync(@"INSERT INTO RJMaster1 ([Name], [Alias], [ParentGrp], [MasterType], [PrintName], [CreatedBy], [CreationTime]) VALUES ({0}, {1}, {2}, 56, {3}, {4}, {5})", stateName, stateName, country.Code, stateName, "Raja", DateTime.UtcNow);
                        }
                        else
                        {
                            await _dbContext.Database.ExecuteSqlRawAsync("UPDATE RJMaster1 SET [Name] = {0}, [Alias] = {1}, [PrintName] = {2}, [ModifiedBy] = {3}, [ModificationTime] = {4} WHERE [Name] = {0} AND [ParentGrp] = {5}", stateName, stateName, stateName, "Raja", DateTime.UtcNow, country.Code);
                        }
                    }
                }

                return new { Status = 1, Msg = "States seeded successfully."};
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message };
            }
        }
        public async Task<dynamic> SeedCitiesToDatabaseAsync()
        {
            try
            {
                var states = await _dbContext.RJMasters.FromSqlRaw("SELECT * FROM RJMaster1 WHERE MasterType = 56").AsNoTracking().ToListAsync();

                foreach (var state in states)
                {
                    var country = await _dbContext.RJMasters.FromSqlRaw("SELECT * FROM RJMaster1 WHERE Code = {0} And MasterType = 55", state.ParentGrp).AsNoTracking().FirstOrDefaultAsync();

                    if (country == null) continue;

                    var requestBody = new { country = country.Name, state = state.Name };
                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/state/cities", content);
                    if (!response.IsSuccessStatusCode) continue;

                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);

                    var cities = doc.RootElement.GetProperty("data").EnumerateArray().Select(c => c.GetString()).Where(name => !string.IsNullOrWhiteSpace(name)).ToList();

                    foreach (var cityName in cities)
                    {
                        var exists = await _dbContext.RJMasters.FromSqlRaw("SELECT * FROM RJMaster1 WHERE Name = {0} AND ParentGrp = {1}", cityName, state.Code).AsNoTracking().FirstOrDefaultAsync();

                        if (exists == null)
                        {
                            await _dbContext.Database.ExecuteSqlRawAsync(@"INSERT INTO RJMaster1 ([Name], [Alias], [ParentGrp], [MasterType], [PrintName], [CreatedBy], [CreationTime]) VALUES ({0}, {1}, {2}, 57, {3}, {4}, {5})", cityName, "", state.Code, cityName, "Raja", DateTime.UtcNow);
                        }
                        else
                        {
                            await _dbContext.Database.ExecuteSqlRawAsync("UPDATE RJMaster1 SET [Name] = {0}, [Alias] = {1}, [PrintName] = {2}, [ModifiedBy] = {3}, [ModificationTime] = {4} WHERE [Name] = {0} AND [ParentGrp] = {5}", cityName, cityName, cityName, "Raja", DateTime.UtcNow, state.Code);
                        }
                    }
                }
                return new { Status = 1, Msg = "Cities seeded successfully." };
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message };
            }
        }

        public async Task<dynamic> GetCountriesAsync()
        {
            try
            {
                string sql = "SELECT ISNULL([Code], 0) as Value, ISNULL([Name], '') as Label FROM RJMaster1 Where MasterType = 55 Group By [Code], [Name] Order By [Name]";
                var DT1 = await _dbContext.UnknowLists.FromSqlRaw(sql).ToListAsync();

                if (DT1.Count == 0)
                {
                    return new { Status = 0, Msg = "No countries found", Data = new List<CountryWithCode>() };
                }
                else
                {
                    var countries = DT1.Select(c => new UnknowList
                    {
                        Label = c.Label,
                        Value = c.Value,
                    }).ToList();

                    return new { Status = 1, Msg = "Countries retrieved successfully", Data = countries };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message, Data = new List<UnknowList>() };
            }
        }

        public async Task<dynamic> GetStatesAsync(string country)
        {
            try
            {
                string sql = "SELECT ISNULL([Code], 0) as Value, ISNULL([Name], '') as Label FROM RJMaster1 Where MasterType = 56 AND ParentGrp = (SELECT Code FROM RJMaster1 WHERE Name = {0} AND MasterType = 55) Group By [Code], [Name] Order By [Name]";
                var DT1 = await _dbContext.UnknowLists.FromSqlRaw(sql, country).ToListAsync();

                if (DT1.Count == 0)
                {
                    return new { Status = 0, Msg = "No states found for the specified country", Data = new List<UnknowList>() };
                }
                else
                {
                    var states = DT1.Select(s => new UnknowList
                    {
                        Value = s.Value,
                        Label = s.Label.ToString()
                    }).ToList();

                    return new { Status = 1, Msg = "States retrieved successfully", Data = states };
                }
            }
            catch (Exception ex)
            {
                return new { Status = 0, Msg = ex.Message, Data = new List<UnknowList>() };
            }
        }

        public async Task<dynamic> GetCitiesAsync(string country, string state)
        {
            try
            {
                string sql = "Select ISNULL(Code, 0) as Value, ISNULL(Name, '') as Label from RJMaster1 Where MasterType = 57 And ParentGrp IN (Select Code From RJMaster1 Where MasterType = 56 And [Name] = {1} And ParentGrp IN (select Code From RJMaster1 Where MasterType = 55 And Name = {0})) Group By Code, [Name] Order By [Name]";
                var DT1 = await _dbContext.UnknowLists.FromSqlRaw(sql, country, state).ToListAsync();

                if (DT1.Count == 0)
                {
                    return new { Status = 0, Msg = "No cities found for the specified country and state", Data = new List<UnknowList>() };
                }
                else
                {
                    var states = DT1.Select(s => new UnknowList
                    {
                        Value = s.Value,
                        Label = s.Label.ToString()
                    }).ToList();

                    return new { Status = 1, Msg = "Cities retrieved successfully", Data = states };
                }
            }
            catch (Exception ex) 
            { 
                return new { Status = 0, Msg = ex.Message, Data = new List<UnknowList>() };
            }
        }
    }
}
