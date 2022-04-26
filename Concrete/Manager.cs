using System;
using System.Collections.Generic;
using System.Text;
using SuvillianceSystem.RabbitMQ_Models.Concrete;
using SuvillianceSystem.RabbitMQ_Models.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System.Security.Principal;

namespace SuvillianceSystem.RabbitMQAuthorizationService
{
    public class Manager
    {
        public Dictionary<OperationType, Func<AuthDTO, AuthDTO>> Factory { get; private set; }
        public IAuthManagerInfo Configuration { get; }

        public Manager(IAuthManagerInfo configuration)
        {
            this.Factory = new Dictionary<OperationType, Func<AuthDTO, AuthDTO>>();
            this.Factory.Add(OperationType.Authorization, this.Authenticate);
            this.Factory.Add(OperationType.Validation, this.Validate);
            this.Configuration = configuration;
        }

        private AuthDTO Authenticate(AuthDTO authInfo)
        {
            AuthDTO response = new AuthDTO()
            {
                Operation = authInfo.Operation,
                Status = OperationStatus.Authorized,
                Token = new AuthToken() { Token = GenerateToken(authInfo.UserName) },
                UserName = authInfo.UserName
            };
            return response;
        }
        private AuthDTO Validate(AuthDTO authInfo)
        {
            OperationStatus status = ValidateTokenHelper(authInfo.Token.Token)?
                                    OperationStatus.Authorized:OperationStatus.Unauthorized;
            AuthDTO response = new AuthDTO()
            {
                Operation = authInfo.Operation,
                Status = status,
                Token = authInfo.Token,
                UserName = authInfo.UserName
            };
            return response;
        }
        private string GenerateToken(string username)
        {

            string secret = this.Configuration.Secret != null ?
                            this.Configuration.Secret : "CustomSecret123";

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(secret); // <--- Taken from appsettings.json
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
              {
             new Claim(ClaimTypes.Name, username)
              }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private bool ValidateTokenHelper(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            SecurityToken validatedToken;
            IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            return true;
        }

        private TokenValidationParameters GetValidationParameters()
        {
            string secret = this.Configuration.Secret != null ?
                            this.Configuration.Secret : "CustomSecret123";
            var encodedKey = Encoding.UTF8.GetBytes(secret);

            return new TokenValidationParameters()
            {
                ValidateLifetime = false, // Because there is no expiration in the generated token
                ValidateAudience = false, // Because there is no audiance in the generated token
                ValidateIssuer = false,   // Because there is no issuer in the generated token
                ValidIssuer = this.Configuration.Issuer,
                ValidAudience = this.Configuration.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(encodedKey) // The same key as the one that generate the token
            };
        }

    }
}
