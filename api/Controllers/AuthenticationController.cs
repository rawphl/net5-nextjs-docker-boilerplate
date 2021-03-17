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
using Microsoft.AspNetCore.Identity;
using api.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    public class Credentials
    {
        public string? email { get; set; }
        public string? name { get; set; }
        public string? password { get; set; }
        public bool? rememberMe { get; set; }
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
        private readonly ILogger _logger;
        
        public AuthenticationController(ILogger<AuthenticationController> logger, JwtService service, IConfiguration configuration, UserManager<ApplicationUser> userManager, ApplicationDbContext appDbContext)
        {
            JwtService = service;
            Configuration = configuration;
            _userManager = userManager;
            _appDbContext = appDbContext;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<dynamic>> Login([FromBody] Credentials credentials)
        {
            _logger.LogInformation("login", credentials.email);
            if (!ModelState.IsValid) return BadRequest();
            var existingUser = await _userManager.FindByEmailAsync(credentials.email);
            if (existingUser == null) return Unauthorized();
            var isCorrect = await _userManager.CheckPasswordAsync(existingUser, credentials.password);
            if (!isCorrect) return Unauthorized();

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.NameId, existingUser.Id),
                new Claim("email", existingUser.Email)
            };

            var (token, refreshToken) = JwtService.GenerateTokens(claims);
            var rt = new RefreshToken() {
                UserId = existingUser.Id,
                Token = refreshToken
            };

            await _appDbContext.RefreshTokens.AddAsync(rt);
            await _appDbContext.SaveChangesAsync();

            return Ok(new { token, refreshToken });
        }

        [HttpPost("register")]
        public async Task<ActionResult<dynamic>> Register([FromBody] Credentials credentials)
        {
            if (!ModelState.IsValid) return BadRequest();

            var existingUser = await _userManager.FindByEmailAsync(credentials.email);
            if (existingUser != null) return BadRequest();

            var created = await _userManager.CreateAsync(new ApplicationUser()
            {
                Email = credentials.email,
                UserName = credentials.name
            }, credentials.password);

            var errors = created.Errors.Select(x => x.Description).ToList();

            if(!created.Succeeded) {
                return BadRequest(errors);
            }
            
            return Ok("Registration succesful");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var token = Request.Headers["Authorization"].ToString().Split(' ')[1];
            var refreshToken = Request.Headers["RefreshToken"];
            var principal = JwtService.GetPrincipalFromExpiredToken(token);
            //var userId = principal.Claims
            var userId = "rfl";
            var storedToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId);
            if (storedToken?.Token != refreshToken) {
                //throw new SecurityTokenException("Invalid refresh token");
                return Unauthorized("Invalid refresh token");
            }
            _logger.LogInformation(storedToken.Token);
            var (newToken, newRefreshToken) = JwtService.GenerateTokens(principal.Claims);
                        _logger.LogInformation(newRefreshToken);
            storedToken.Token = newRefreshToken;
            _appDbContext.RefreshTokens.Update(storedToken);
            await _appDbContext.SaveChangesAsync();
            return Ok(new { newToken, refreshToken });
        }
    }
}
