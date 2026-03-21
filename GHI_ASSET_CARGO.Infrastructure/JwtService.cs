using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GHI_ASSET_CARGO.Infrastructure
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        //public string GenerateToken(AppUser user, IList<string> roles)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();

        //    var key = Encoding.UTF8.GetBytes(_config.GetSection("JWT:Key").Value!);


        //    var claimList = new List<Claim>
        //    {
        //        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        //        new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
        //        new(JwtRegisteredClaimNames.Email, user.Email!),
        //        new Claim("AirlineId", user.AirlineId.ToString())
        //    };
        //    claimList.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));


        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Audience = _config.GetSection("JWT:Audience").Value,
        //        Issuer = _config.GetSection("JWT:Issuer").Value,
        //        Subject = new ClaimsIdentity(claimList),
        //        Expires = DateTime.UtcNow.AddDays(7),
        //        SigningCredentials =
        //            new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };

        //    return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        //}

        public string GenerateToken(AppUser user, IList<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWT:Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("AirlineId", user.AirlineId.ToString())
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"],
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
