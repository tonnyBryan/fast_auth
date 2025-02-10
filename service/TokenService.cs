using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace fast_auth.service
{
    public class TokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(string secretKey, string issuer, string audience)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
        }

        public string GenerateToken(string username)
        {
            var expirationDate = DateTime.UtcNow.AddHours(1);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, username), // "sub" est généralement l'identifiant de l'utilisateur
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // "jti" est un identifiant unique du token
            };

            // Créer la clé secrète pour signer le token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            // Créer les informations du token (header + claims + signature)
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expirationDate,
                signingCredentials: credentials
            );

            // Générer le token en format string
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }
    }

}
