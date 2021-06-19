using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DoxmandAPI.Repos;
using DoxmandBackend.Common;
using Microsoft.IdentityModel.Tokens;

namespace DoxmandAPI
{
    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        private readonly string _key;
        private readonly DoxmandRepo _repo = new DoxmandRepo();

        public JwtAuthenticationManager(string key)
        {
            _key = key;
        }
        
        public string Authenticate(string email, string password)
        {
            var users = _repo.GetAllUsers();

            if (users == null)
            {
                return null;
            }
            
            var user = users.FirstOrDefault(u => u.Email == email && u.Password == CommonMethods.EncryptPassword(password));

            if (user == null)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim("Email", user.Email),
                    new Claim("Name", user.Name),
                    new Claim("User_ID", user.User_ID)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature
                    )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}