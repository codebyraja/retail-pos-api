using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Location.Models;

namespace LocationRepository.Services
{
    public interface ILocationService
    {
        //Task<List<string>> GetCountriesAsync();
        //Task<List<string>> GetStatesAsync(string country);
        //Task<List<string>> GetCitiesAsync(string country, string state);

        Task<dynamic> GetCountriesWithCodeAsync();
        Task<dynamic> GetStateWithCodeAsync(string country);
        Task<dynamic> GetCitiesWithCodeAsync(string country, string state);
        Task<dynamic> SeedCountriesToDatabaseAsync();
        Task<dynamic> SeedStatesToDatabaseAsync();
        Task<dynamic> SeedCitiesToDatabaseAsync();
        Task<dynamic> GetStatesAsync(string country);
        Task<dynamic> GetCitiesAsync(string country, string state);
        Task<dynamic> GetCountriesAsync();
    }
}
