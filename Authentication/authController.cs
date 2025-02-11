using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hr_System_Demo_3.Authentication
{/*
    [Route("")]
    [ApiController]
    public class authController (AppDbContext DbContext , JwtOptions jwtOptions ) : ControllerBase
    {

        [HttpPost]
        [Route("auth")]
        [AllowAnonymous]
        public ActionResult<string> AuthenticateUser(AuthenticateRequest request)
        {
            var user = DbContext.Set<Employee>().FirstOrDefault(x => x.empName == request.UserName && x.empPassword == request.Password && x.deptId == request.deptId);
           
            if (user == null) return Unauthorized();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescribtor = new SecurityTokenDescriptor
            {
                Issuer = jwtOptions.Issuer,
                Audience = jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
               , SecurityAlgorithms.HmacSha256),

                Subject = new ClaimsIdentity(new Claim[]
             {
                new (ClaimTypes.NameIdentifier , request.UserName),
                new (ClaimTypes.NameIdentifier , request.Password),
                
               // new (ClaimTypes.NameIdentifier , user.Id.ToString()),
                //new (ClaimTypes.Name , user.Name),
                new (ClaimTypes.Role, "User"),
                new (ClaimTypes.Role , "SuperHr"),
                new (ClaimTypes.Role , "HrEmp"),

             })
            };

            var secuirityToken = tokenHandler.CreateToken(tokenDescribtor);
            var accessToken = tokenHandler.WriteToken(secuirityToken);
            return Ok(new
            {
                Token = accessToken,
                UserId = user.empId
            });
        }
    }*/
}
