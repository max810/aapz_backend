using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AAPZ_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly AAPZ_BackendContext _context;

        public CompanyController(AAPZ_BackendContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCompany([FromBody] Company company)
        {
            if (ModelState.IsValid)
            {
                await _context.Companies.AddAsync(company);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetCompany), new { name = company.Name }, company);
            }
            else
            {
                return BadRequest(company);
            }
        }

        [HttpGet("all")]
        public IEnumerable<Company> GetAllCompanies()
        {
            return _context.Companies;
        }

        // GET: api/Company/5
        [HttpGet("{name}")]
        public async Task<Company> GetCompany(string name)
        {
            return await _context.Companies.FindAsync(name);
        }
    }
}
