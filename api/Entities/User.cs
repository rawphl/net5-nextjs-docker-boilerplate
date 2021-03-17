using Microsoft.AspNetCore.Identity;

namespace api.Entities
{
  public class ApplicationUser : IdentityUser
  {
    		public string? refreshToken { get; set; }
  }
}