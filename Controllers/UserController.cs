using Hr_System_Demo_3.Authentication;
using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hr_System_Demo_3.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController (AppDbContext DbContext , JwtOptions jwtOptions) : ControllerBase
    {

        [HttpPost("add-employee")]
        [Authorize(Roles = "HrEmp, SuperHr")]
        public async Task<ActionResult> AddEmployee(AuthenticateRequest request)
        {
            var hrId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (hrId == null) return Unauthorized("Invalid HR credentials");

           var newEmployee = new Employee
            {
                empName = request.UserName,
                empEmail = request.Email,
                empPassword = request.Password,
                deptId = request.deptId,
                Hr_Id = Guid.Parse(hrId)
            };

            DbContext.Employees.Add(newEmployee);
            await DbContext.SaveChangesAsync();
           // return Ok(hrId);
            return Ok(new
            {
                Message = "Employee added successfully",
                EmployeeId = newEmployee.empId,
                HrId = hrId
            });

        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<string> Login(AuthenticateRequest request)
        {
            var user = DbContext.Set<Employee>().FirstOrDefault(x => x.empName == request.UserName && x.empPassword == request.Password && x.deptId == request.deptId );
           
            if (user == null) return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = jwtOptions.Issuer,
                Audience = jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)), SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.empId.ToString()),
                    new Claim(ClaimTypes.Name, user.empName),
                    new Claim(ClaimTypes.Email, user.empEmail),
                 //   new Claim(ClaimTypes.NameIdentifier , user.empId),
                    new Claim(ClaimTypes.Role, "User")
                })
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);
            return Ok(new
            {
                Token = accessToken,
                UserId = user.empId
            });
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<ActionResult> SignUp(AuthenticateRequest request)
        {
            if (DbContext.Set<Employee>().Any(x => x.empEmail == request.Email))
                return BadRequest("User with this email already exists.");

            var newUser = new Employee
            {
                empName = request.UserName,
                empEmail = request.Email,
                empPassword = request.Password, 
             //   deptId = request.deptId
            };

            DbContext.Employees.Add(newUser);
            await DbContext.SaveChangesAsync();

            return Ok("User registered successfully");
        }
    }
}

    
