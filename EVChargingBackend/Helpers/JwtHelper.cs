namespace EVChargingBackend.Helpers
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;

    public static class JwtHelper
    {
        public static string GenerateJwtToken(string username, string role, string secretKey)
        {
            // Create claims based on user data
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            // Create a signing key using the provided secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: "http://localhost:5033",
                audience: "http://localhost:3000",
                claims: claims,
                expires: DateTime.Now.AddHours(1),  // Token expiration time
                signingCredentials: creds
            );

            // Return the token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
