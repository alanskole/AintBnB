using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AintBnB.BusinessLogic.Helpers.AllCountriesAndCities;

namespace AintBnB.BlazorWASM.Server.Controllers
{
    [ApiController]
    [Authorize]
    public class WorldController : Controller
    {

        /// <summary>API GET request that returns all the countries from the SQLite database with all the cities and countries.</summary>
        /// <returns>Status code 200 and all the countires if successful otherwise status code 404 if an error occured</returns>
        [HttpGet]
        [Route("api/[controller]/countries")]
        public async Task<ActionResult<List<string>>> GetAllCountriesInTheWorldAsync()
        {
            try
            {
                return await GetAllTheCountriesAsync();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>API GET request that returns all the cities of a country from the SQLite database with all the cities and countries.</summary>
        /// <param name="country">The country name to return all the cities of.</param>
        /// <returns>Status code 200 and all the cities of a country if successful otherwise status code 404 if an error occured</returns>
        [HttpGet]
        [Route("api/[controller]/cities/{country}")]
        public async Task<ActionResult<List<string>>> GetAllCitiesOfAllTheCountriesAsync([FromRoute] string country)
        {
            try
            {
                return await GetCitiesOfACountryAsync(country);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
