namespace poc_caching_api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("poc-caching-api/logs")]
    public class ValuesController : Controller
    {
        // GET poc-caching-api/logs/index
        [HttpGet("{index}")]
        public IEnumerable<string> Get(string index)
        {
            return new string[] { "value1", "value2" };
        }

        // GET poc-caching-api/logs/id
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
    }
}
