using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
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
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ARKPZ_CourseWork_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AAPZ_BackendContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(AAPZ_BackendContext context, UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register-driver")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DriverRegisterModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterDriverAsync([FromBody] DriverRegisterModel registerModel)
        {
            if (ModelState.IsValid)
            {
                var user = new User()
                {
                    UserName = registerModel.UserName,
                    Email = registerModel.Email,
                };
                var result = await _userManager.CreateAsync(user, registerModel.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(registerModel);
                }
                await _userManager.AddToRoleAsync(user, "Driver");

                var driver = registerModel.Driver;
                driver.User = user;
                driver.Id = user.Id;
                driver.IdentifierHashB64 = Misc.GetMD5HashB64(driver.IdentifierHashB64);
                _context.Drivers.Add(driver);
                await _context.SaveChangesAsync();

                return Ok();
            }

            return BadRequest(registerModel);
        }

        [HttpPost("register-manager")]
        [ProducesResponseType(typeof(Manager), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ManagerRegisterModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterManagerAsync([FromBody] ManagerRegisterModel registerModel)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Email = registerModel.Email,
                    UserName = registerModel.Email,
                };
                var result = await _userManager.CreateAsync(user, registerModel.Password);
                if (!result.Succeeded)
                {
                    return BadRequest();
                }
                await _userManager.AddToRoleAsync(user, "Manager");

                var manager = registerModel.Manager;
                manager.User = user;
                manager.Id = user.Id;
                _context.Managers.Add(manager);
                await _context.SaveChangesAsync();

                return Ok();
            }
            else
            {
                return BadRequest(registerModel);
            }
        }

        [HttpPost("login-driver")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthModel), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> LoginDriver([FromBody] AuthModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var driver = await _context.Drivers.SingleOrDefaultAsync(x => x.User.Email == model.Email);

                if (user is null)
                {
                    return NotFound(model.Email);
                }

                if (driver is null)
                {
                    return Forbid();
                }

                Microsoft.AspNetCore.Identity.SignInResult res = await _signInManager.PasswordSignInAsync(
                    user.UserName, model.Password, true, false);

                if (!res.Succeeded)
                {
                    return BadRequest(model);
                }

                string jwt = await CreateDefaultJWT(user);

                return Ok(jwt);
            }

            return BadRequest(model);
        }

        [HttpPost("login-manager")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthModel), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> LoginManager([FromBody] AuthModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var manager = await _context.Managers.SingleOrDefaultAsync(x => x.User.Email == model.Email);

                if (user is null)
                {
                    return NotFound(model.Email);
                }

                if (manager is null)
                {
                    return Forbid();
                }

                Microsoft.AspNetCore.Identity.SignInResult res = await _signInManager.PasswordSignInAsync(
                    user?.UserName, model.Password, true, false);

                if (!res.Succeeded)
                {
                    return BadRequest(model);
                }
                string jwt = await CreateDefaultJWT(user);

                return Ok(jwt);
            }

            return BadRequest(model);
        }

        private async Task<string> CreateDefaultJWT(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };
            foreach (string role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: JWTOptions.ISSUER,
                audience: JWTOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(JWTOptions.LIFETIME),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTOptions.KEY)),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return encodedJwt;
        }

        [HttpPost("logout")]
        public async Task LogOff()
        {
            await _signInManager.SignOutAsync();
        }
    }
}