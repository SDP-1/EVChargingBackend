namespace EVChargingBackend.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;

    public static class JwtHelper
    {
        // Updated method: now optionally accepts NIC for EVOwner
        public static string GenerateJwtToken(string username, string role, string userId, string nic, string secretKey)
        {
            var claims = new List<Claim>
             {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("userId", userId)
              };

            if (!string.IsNullOrEmpty(nic) && role == "EVOwner")
            {
                claims.Add(new Claim("nic", nic));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "http://localhost:5033",
                audience: "http://localhost:3000",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
