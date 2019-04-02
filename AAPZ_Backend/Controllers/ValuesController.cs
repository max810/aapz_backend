using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AAPZ_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private AAPZ_BackendContext _context;
        public ValuesController(AAPZ_BackendContext dbContext)
        {
            _context = dbContext;
        }

        [HttpGet("video")]
        public IActionResult GetVideoContent()
        {
            //User.Identity.Name;

            try
            {
                var fs = System.IO.File.Open("BigBuckBunny.mp4", FileMode.Open);
                return new FileStreamResult(fs, new MediaTypeHeaderValue("video/mp4").MediaType);
            }
            catch
            {
                return BadRequest();
            }
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Company x = new Company();
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
