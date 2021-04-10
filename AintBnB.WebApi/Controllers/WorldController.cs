using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

using static AintBnB.BusinessLogic.Helpers.AllCountiresAndCities;

namespace AintBnB.WebApi.Controllers
{
    public class WorldController : Controller
    {
        [HttpGet]
        [Route("api/[controller]/countries")]
        public async Task<IActionResult> GetAllCountriesInTheWorldAsync()
        {
            try
            {
                return Ok(await GetAllTheCountriesAsync());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/cities/{country}")]
        public async Task<IActionResult> GetAllCitiesOfAllTheCountriesAsync([FromRoute] string country)
        {
            try
            {
                return Ok(await GetCitiesOfACountryAsync(country));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
