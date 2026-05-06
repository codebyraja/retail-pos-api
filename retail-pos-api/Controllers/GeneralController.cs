using General.Services.Repository;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;

namespace retail_pos_api.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[action]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        public readonly IGeneralRepository _services;

        public GeneralController(IGeneralRepository services)
        {
            this._services = services;
        }

        [HttpGet()]
        public async Task<IActionResult> GetVchNoAsync(int tranType, int vchType)
        {
            if (vchType == 0)
                return BadRequest(new { Status = 0, Msg = "VchType is required" });
            return Ok(await _services.GetVchNoAsync(tranType, vchType));
        }
    }
}
