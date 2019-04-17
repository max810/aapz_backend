using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL;
using BLL.Models;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AAPZ_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatsController : ControllerBase
    {
        private readonly AAPZ_BackendContext _context = null;
        private readonly IServiceProvider _provider;
        private readonly UserManager<User> _userManager;
        private readonly Statistics _statistics;

        public StatsController(AAPZ_BackendContext context, IServiceProvider provider, UserManager<User> userManager,
             Statistics statistics)
        {
            _context = context;
            _provider = provider;
            _userManager = userManager;
            _statistics = statistics;
        }

        [HttpGet("driver-stats/{id}")]
        [Authorize(Roles = "Driver,Manager")]
        public async Task<ActionResult<DriverStats>> GetDriverStats(string id, DateTime? from = null, DateTime? to = null)
        {
            User currentUser = await _userManager.GetUserAsync(HttpContext.User);
            DateTime periodBegin = from ?? DateTime.MinValue;
            DateTime periodEnd = to ?? DateTime.Now;

            return await GetDriverStatsPeriod(currentUser, id, periodBegin, periodEnd);
        }

        private async Task<ActionResult<DriverStats>> GetDriverStatsPeriod(
            User currentUser,
            string requestedDriverId,
            DateTime from, DateTime to)
        {
            Driver requestedDriver = _context.Drivers.SingleOrDefault(x => x.Id == requestedDriverId);

            if (await _userManager.IsInRoleAsync(currentUser, "Manager"))
            {
                Manager currentManager = _context.Managers.Single(x => x.Id == currentUser.Id);
                if (requestedDriver.CompanyName != currentManager.CompanyName)
                {
                    return StatusCode(403, "You and your driver must belong to the same company!");
                }
            }
            else if (currentUser.Id != requestedDriverId)
            {
                return StatusCode(403, "You can only get your own statistics!");
            }

            return _statistics.GetDriverInfo(requestedDriver);
        }
    }
}