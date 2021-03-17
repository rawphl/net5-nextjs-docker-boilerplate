using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Cors;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using api.Entities;

namespace api.Controllers
{
    public class Credentials
    {
        public string? name { get; set; }
        public string? password { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class AuthenticationController : ControllerBase
    {
        private readonly JwtService JwtService;
        private readonly IConfiguration Configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _appDbContext;
        public AuthenticationController(JwtService service, IConfiguration configuration, UserManager<ApplicationUser> userManager, ApplicationDbContext appDbContext)
        {
            JwtService = service;
            Configuration = configuration;
            _userManager = userManager;
            _appDbContext = appDbContext;
        }

        [HttpGet("users")]
        public async Task<ActionResult<dynamic>> Users()
        {
            return Ok(_appDbContext.Users);
        }

        [HttpPost("login")]
        public async Task<ActionResult<dynamic>> Login([FromBody] Credentials credentials)
        {
            if (!ModelState.IsValid) return BadRequest();
            var existingUser = await _userManager.FindByNameAsync(credentials.name);
            if (existingUser == null) return Unauthorized();
            var isCorrect = await _userManager.CheckPasswordAsync(existingUser, credentials.password);
            if (!isCorrect) return Unauthorized();

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.NameId, existingUser.Id),
                new Claim("name", existingUser.UserName)
            };

            var (token, refreshToken) = JwtService.GenerateTokens(claims);
            return Ok(new { token, refreshToken });
        }

        [HttpPost("register")]
        public async Task<ActionResult<dynamic>> Register([FromBody] Credentials credentials)
        {
            if (!ModelState.IsValid) return BadRequest();
            var existingUser = await _userManager.FindByNameAsync(credentials.name);
            if (existingUser != null) return BadRequest();

            var created = await _userManager.CreateAsync(new ApplicationUser()
            {
                UserName = credentials.name
            }, credentials.password);

            if(!created.Succeeded) {
                return BadRequest(created.Errors.Select(x => x.Description).ToList());
            }

            return Ok(new { message = "User created" });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            var token = Request.Cookies["token"] ?? "";
            var refreshToken = Request.Cookies["refresh_token"];

            var principal = JwtService.GetPrincipalFromExpiredToken(token);
            var username = "rawphl";
            var savedRefreshToken = refreshToken; //retrieve the refresh token from a data store
            if (savedRefreshToken != refreshToken)
                throw new SecurityTokenException("Invalid refresh token");

            var claims = new[] {
                new Claim("name", username)
            };

            //DeleteRefreshToken(username, refreshToken);
            //SaveRefreshToken(username, newRefreshToken);

            var (newToken, newRefreshToken) = JwtService.GenerateTokens(claims);

            return Ok(newToken);
        }
    }
}
