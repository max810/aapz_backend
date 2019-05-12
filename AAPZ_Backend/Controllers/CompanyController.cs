using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BLL.Models;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AAPZ_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AAPZ_BackendContext _context;

        public CompanyController(AAPZ_BackendContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(Company), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Company), StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(typeof(Company[]), StatusCodes.Status200OK)]
        public IEnumerable<Company> GetAllCompanies()
        {
            return _context.Companies;
        }

        // GET: api/Company/5
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(Company), StatusCodes.Status200OK)]
        public async Task<Company> GetCompany(string name)
        {
            return await _context.Companies.FindAsync(name);
        }

        [HttpGet("drivers/{name}")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(Driver[]), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Driver>>> GetCompanyDrivers(string name)
        {
            User currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var manager = await _context.Managers.FindAsync(currentUser.Id);

            if (manager is null)
            {
                return StatusCode(403);
            }

            var company = await _context.Companies.FindAsync(name);

            if (company is null)
            {
                return BadRequest(name);
            }

            _context.Entry(company).Collection(x => x.Drivers).Load();
            return Ok(JsonConvert.SerializeObject(company.Drivers.Select(x =>
            {
                x.IdentifierHashB64 = null;
                x.User = null;
                x.Rides = null;
                x.Company = null;

                return x;
            })));
        }

        // GET: api/Company/5
        [HttpGet("user-info")]
        [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
        public async Task<UserInfo> GetUserInfo(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            return new UserInfo()
            {
                Id = id,
                Email = user.Email,
                UserName = user.UserName,
            };
        }
    }
}
