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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            //PopulateRandomRides(context, "4caa4591-5ffb-4d79-a705-c7cf0cb60375", 150);
        }

        private void PopulateRandomRides(AAPZ_BackendContext context, string driverId, int count)
        {
            var driver = context.Drivers.Find(driverId);
            Random rnd = new Random(1337);
            DateTime now = DateTime.Now;
            DateTime startDate = now - TimeSpan.FromDays(count);
            DateTime currentDate = startDate.Date + TimeSpan.FromHours(rnd.Next(1, 18)); // .Date leaves time at 00:00:00

            for (int i = 0; i < count; i++)
            {
                double s0 = TimeSpan.FromMinutes(58 + rnd.Next(-5, 5)).TotalSeconds;
                double sn() => TimeSpan.FromSeconds(10 + rnd.Next(-5, 25)).TotalSeconds;
                Ride ride = new Ride()
                {
                    StartTime = currentDate,
                    NormalDrivingSeconds = s0,
                    MobileRightSeconds = sn(),
                    MobileRightHeadSeconds = sn(),
                    MobileLeftSeconds = sn(),
                    MobileLeftHeadSeconds = sn(),
                    RadioSeconds = sn(),
                    DrinkSeconds = sn(),
                    SearchSeconds = sn(),
                    MakeupSeconds = sn(),
                    TalkingSeconds = sn(),
                };
                ride.EndTime = ride.StartTime
                    + TimeSpan.FromSeconds(ride.NormalDrivingSeconds)
                    + TimeSpan.FromSeconds(ride.MobileRightSeconds)
                    + TimeSpan.FromSeconds(ride.MobileRightHeadSeconds)
                    + TimeSpan.FromSeconds(ride.MobileLeftSeconds)
                    + TimeSpan.FromSeconds(ride.MobileLeftHeadSeconds)
                    + TimeSpan.FromSeconds(ride.RadioSeconds)
                    + TimeSpan.FromSeconds(ride.DrinkSeconds)
                    + TimeSpan.FromSeconds(ride.SearchSeconds)
                    + TimeSpan.FromSeconds(ride.MakeupSeconds)
                    + TimeSpan.FromSeconds(ride.TalkingSeconds);

                ride.InProgress = false;
                ride.Driver = driver;
                ride.DriverId = driver.Id;
                context.Rides.Add(ride);

                currentDate += TimeSpan.FromDays(1);
                currentDate = currentDate.Date + TimeSpan.FromHours(rnd.Next(1, 18));
            }
            context.SaveChanges();

        }

        [HttpGet("driver-stats/my")]
        [Authorize(Roles = "Driver")]
        [ProducesResponseType(typeof(DriverStats), StatusCodes.Status200OK)]
        public async Task<string> GetMyDriverStats(DateTime? from = null, DateTime? to = null)
        {
            User currentUser = await _userManager.GetUserAsync(HttpContext.User);
            DateTime periodBegin = from ?? DateTime.MinValue;
            DateTime periodEnd = to ?? DateTime.Now;
            var stats = await GetDriverStatsPeriod(currentUser, currentUser.Id, periodBegin, periodEnd);

            return stats.Value;
        }

        [HttpGet("rating/my")]
        [Authorize(Roles = "Driver")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetMyRatingPlace()
        {
            User currentUser = await _userManager.GetUserAsync(HttpContext.User);
            Driver currentDriver = await _context.Drivers.FindAsync(currentUser.Id);
            var drivers = _context.Drivers.Where(x => x.CompanyName == currentDriver.CompanyName).Include(x => x.Rides);
            var stats = _statistics.GetRatingList(drivers);

            return stats.Select(x => x.DriverId).ToList().BinarySearch(currentDriver.Id) + 1;
        }

        [HttpGet("rating/all")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetAllDriversRating()
        {
            User currentUser = await _userManager.GetUserAsync(HttpContext.User);
            Manager currentManager = await _context.Managers.FindAsync(currentUser.Id);
            var stats = _statistics.GetRatingList(_context.Drivers.Where(x => x.CompanyName == currentManager.CompanyName).Include(x => x.Rides));
            return Ok(stats.Select(x => x.DriverId).ToList());
        }

        [HttpGet("driver-stats/{id}")]
        [Authorize(Roles = "Driver,Manager")]
        [ProducesResponseType(typeof(DriverStats), StatusCodes.Status200OK)]
        public async Task<string> GetDriverStats(string id, DateTime? from = null, DateTime? to = null)
        {
            User currentUser = await _userManager.GetUserAsync(HttpContext.User);
            DateTime periodBegin = from ?? DateTime.MinValue;
            DateTime periodEnd = to ?? DateTime.Now;
            var stats = await GetDriverStatsPeriod(currentUser, id, periodBegin, periodEnd);

            return stats.Value;
        }


        [HttpGet("driver-stats")]
        [Authorize(Roles = "Driver,Manager")]
        [ProducesResponseType(typeof(DriverStats), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetAllDriversStats(DateTime? from = null, DateTime? to = null)
        {
            User currentUser = await _userManager.GetUserAsync(HttpContext.User);
            DateTime periodBegin = from ?? DateTime.MinValue;
            DateTime periodEnd = to ?? DateTime.Now;
            var stats = GetAllDriversStatsPeriod(currentUser, periodBegin, periodEnd);

            return Ok(JsonConvert.SerializeObject(stats, new StringEnumConverter()));
        }

        private async Task<ActionResult<string>> GetDriverStatsPeriod(
                User currentUser,
                string requestedDriverId,
                DateTime from, DateTime to)
        {
            Driver requestedDriver = _context.Drivers.Include(x => x.Rides).SingleOrDefault(x => x.Id == requestedDriverId);

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

            // TODO: return Json WITHOUT circular loop
            return JsonConvert.SerializeObject(_statistics.GetDriverInfo(requestedDriver), new StringEnumConverter());
        }

        private IEnumerable<DriverStats> GetAllDriversStatsPeriod(
            User currentUser,
            DateTime from, DateTime to)
        {
            var manager = _context.Managers.Single(x => x.Id == currentUser.Id);
            foreach (var driver in _context.Drivers.Include(x => x.Rides).Where(x => x.CompanyName == manager.CompanyName))
            {
                yield return _statistics.GetDriverInfo(driver);
            }
        }
    }
}