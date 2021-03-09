using Microsoft.AspNetCore.Mvc;
using System;
using static AintBnB.BusinessLogic.Helpers.AllCountiresAndCities;

namespace AintBnB.WebApi.Controllers
{
    public class WorldController : Controller
    {
        [HttpGet]
        [Route("api/[controller]/countries")]
        public IActionResult GetAllCountriesInTheWorld()
        {
            try
            {
                return Ok(GetAllTheCountries());
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/[controller]/cities/{country}")]
        public IActionResult GetAllCitiesOfAllTheCountries([FromRoute] string country)
        {
            try
            {
                return Ok(GetCitiesOfACountry(country));
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
