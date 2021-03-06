﻿using Authenticate.Api.Entities;
using Authenticate.Api.Helpers;
using Authenticate.Api.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authenticate.Api.Data
{
    public class TokenRepository : IToken
    {
        private readonly IConfiguration _configuration;
        private readonly IBase _firebase;
        public TokenRepository (IConfiguration configuration, IBase firebase)
        {
            _configuration = configuration;
            _firebase = firebase;
        }
        public TokenResponse RequestToken(TokenRequest request)
        {
            var users = _firebase.GetUsers();
            var user = users.FirstOrDefault(u => u.Email.ToLower() == request.Username.ToLower());
            if(user != null)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, request.Username),
                    new Claim("CompleteHRProcess",""),
                    new Claim(CustomClaimTypes.EmploymentCommenced,
                               new DateTime(2017,10,04).ToString(),
                               ClaimValueTypes.DateTime)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecurityKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "https://github.com/freedomkhanyile",
                    audience: "https://github.com/freedomkhanyile",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: creds
                    );

                var response = new TokenResponse();
                response.Token =  new JwtSecurityTokenHandler().WriteToken(token);
                return response;
            }

            return null;
        }
    }
}
