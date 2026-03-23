using EasebuzzPayment.Services.V1;
using EasebuzzPayment.Services.V2;
using LocationRepository.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QsrAdminWebApi.Controllers;
using QSRTokenService.Services.Token;
using Location.Models;

namespace QsrAdminWebApi.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[action]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        public readonly ILocationService _location;
        public LocationController(ILocationService location)
        {
            _location = location;
        }

        [HttpGet("countries")]
        public async Task<IActionResult> GetCountries()
        {
            var countries = await _location.GetCountriesAsync();
            return Ok(countries);
        }

        [HttpPost("states")]
        public async Task<IActionResult> GetStates([FromBody] State request)
        {
            var states = await _location.GetStatesAsync(request.Country);
            return Ok(states);
        }

        [HttpPost("cities")]
        public async Task<IActionResult> GetCities([FromBody] City request)
        {
            var cities = await _location.GetCitiesAsync(request.Country.Trim(), request.State.Trim());
            return Ok(cities);
        }

        [HttpGet("countries")]
        public async Task<IActionResult> GetCountriesWithCode()
        {
            var countries = await _location.GetCountriesWithCodeAsync();
            return Ok(countries);
        }

        [HttpPost("states")]
        public async Task<IActionResult> GetStatesWithCode([FromBody] State request)
        {
            var states = await _location.GetStateWithCodeAsync(request.Country.Trim());
            return Ok(states);
        }

        [HttpPost("cities")]
        public async Task<IActionResult> GetCitiesWithCode([FromBody] City request)
        {
            var cities = await _location.GetCitiesWithCodeAsync(request.Country.Trim(), request.State.Trim());
            return Ok(cities);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshCountries()
        {
            return Ok(await _location.SeedCountriesToDatabaseAsync());
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshStates()
        {
            return Ok(await _location.SeedStatesToDatabaseAsync());
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshCities()
        {
            return Ok(await _location.SeedCitiesToDatabaseAsync());
        }
    }
}
