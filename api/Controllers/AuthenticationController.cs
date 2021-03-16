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

namespace api.Controllers
{
    public class Credentials
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class AuthenticationController : ControllerBase
    {
        private readonly JwtService JwtService;
        private readonly IConfiguration Configuration;

        public AuthenticationController(JwtService service,  IConfiguration configuration)
        {
            JwtService = service;
            Configuration = configuration;
        }
        
        [HttpPost("login")]
        public ActionResult<dynamic> Login([FromBody] Credentials credentials)
        {
            if(string.IsNullOrEmpty(credentials.Username) || string.IsNullOrEmpty(credentials.Password))
            {
                return BadRequest();
            }

            //var result = AuthenticationService.Authenticate(credentials.Username, credentials.Password);
       
            if (!IsValidCredentials(credentials)) return Unauthorized();

            var claims = new[] {
                new Claim("name", credentials.Username)
            };

            var (token, refreshToken) = JwtService.GenerateTokens(claims);
            return Ok(new { token, refreshToken });
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

        private bool IsValidCredentials(Credentials credentials)
        {
            return credentials.Username == "rawphl" && credentials.Password == "lol";
        }

    }
}
