using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Projet4.Classes;
using Projet4.Models;

namespace Projet4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class UserController : ControllerBase
    {

        private readonly IConfiguration _config;

        public UserController(IConfiguration config)
        {
            _config = config;
        }

        [Route("login")]
        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            var jwtToken = GenerateJSONWebToken(user);
            if (jwtToken == null)
            {
                return Unauthorized();
            }

            return Ok(jwtToken);
        }

        
        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<string>> Get()
        {
            List<Claim> userClaimList = HttpContext.User.Claims.ToList();
            List<string> list = new List<string>();

            string id = userClaimList[0].Value.ToString();
            string username = userClaimList[1].Value.ToString();
            string role = userClaimList[2].Value.ToString();

            list.Add(id);
            list.Add(username);
            list.Add(role);

            return list;
            //return Ok(list);
        }

        [Route("gestion")]
        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public IActionResult Gestion()
        {
            List<Claim> userClaimList = HttpContext.User.Claims.ToList();

            string id = userClaimList[0].Value.ToString();
            string username = userClaimList[1].Value.ToString();
            string role = userClaimList[2].Value.ToString();

            string msg = "Id : '" + id + "' -- Username : '" + username + "' -- Role : '" + role + "' : accés autorisé.";
            return Ok(msg);

        }


        private string GenerateJSONWebToken(User userInfo)
        {
            var user = _users.Where(x => x.Username == userInfo.Username && x.Password == userInfo.Password).SingleOrDefault();
            if (user != null)
            {
                var signingKey = Convert_base64String.ConvertFromBase64String(_config["Jwt:Key"]);
                var expiryDuration = int.Parse(_config["Jwt:ExpiryDuration"]);
                var tokenDiscriptor = new SecurityTokenDescriptor
                {
                    Issuer = null,
                    Audience = null,
                    IssuedAt = DateTime.UtcNow,
                    NotBefore = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddMinutes(expiryDuration),
                    Subject = new ClaimsIdentity(new List<Claim> {
                        new Claim("id", user.Id.ToString()),
                        new Claim("username", user.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256Signature)
                };

                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtTokenHandler.CreateJwtSecurityToken(tokenDiscriptor);
                var token = jwtTokenHandler.WriteToken(jwtToken);

                return token;
            }

            return null;
        }

        //********************************* DATA **************************

        private readonly IEnumerable<User> _users = new List<User>
        {
            new User {Id=1, Username = "nouha", Password="test", Role="Admin"},
            new User {Id=2, Username = "derbal", Password="test", Role="invité"},
            new User {Id=3, Username = "derbaaal", Password="test", Role="autre"}
        };
    }
}