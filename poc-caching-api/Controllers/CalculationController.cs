namespace pocCachingApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using pocCachingApi.BusinessLayer;

    [Route("poc-caching-api/calculation")]
    public class CalculationController : Controller
    {
        // GET poc-caching-api/calculation
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var service = new CalculatorCachingService();
            var result = await service.GetResults(id);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine($"Process time: {elapsedMs.ToString()} ms");
            Console.WriteLine("----------------------------------------------");
            
            if (result.isValid)
            {
                if (result.Result != null)
                {
                    return Ok(result.Result);
                }
                else
                {
                    return Ok($"{result.Message}");
                }
            }

            return StatusCode(500, result.Message);
        }
    }
}
