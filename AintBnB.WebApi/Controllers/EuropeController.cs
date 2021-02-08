using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Services.AllCountiresAndCitiesEurope;

namespace AintBnB.WebApi.Controllers
{
    public class EuropeController : Controller
    {
        [HttpGet]
        [Route("api/[controller]/countries")]
        public IActionResult GetAllCountriesInEurope()
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
        public IActionResult GetAllCitiesOfAEuropeanCountry([FromRoute] string country)
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
