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

        public StatsController(AAPZ_BackendContext context, IServiceProvider provider)
        {
            _context = context;
            _provider = provider;
        }

        [HttpGet("driver-stats/{id}")]
        [Authorize(Roles = "Driver,Manager")]
        public ActionResult<DriverStats> GetDriverStats(int id)
        {
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string currentUserRoles = User.FindFirst(ClaimTypes.Role).Value;
            User currentUser = _context.Users.Single(x => x.Id == currentUserId);

            if(User.HasClaim(ClaimTypes.Role, "Manager"))
            {
                if(currentUser.Company)
                    // TODO
                    //check driver/manager company match IF MANAGER
                    //check your Id vs request Id IF DRIVER
            }
        }

    }
}