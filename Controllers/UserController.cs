using Hr_System_Demo_3.Authentication;
using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    public class UserController(AppDbContext DbContext, JwtOptions jwtOptions) : ControllerBase
    {
        private readonly PasswordHasher<Employee> _passwordHasher = new();

        [HttpPost("add-employee")]
        [Authorize(Roles = "HrEmp, SuperHr")]
        public async Task<ActionResult> AddEmployee(HrRequest request)
        {
            var hrId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (hrId == null) return Unauthorized("Invalid HR credentials");

            var newEmployee = new Employee
            {
                empName = request.UserName,
                empEmail = request.Email,
                empPassword = _passwordHasher.HashPassword(null, request.Password),
                deptId = request.deptId,
                Hr_Id = Guid.Parse(hrId),
                Role = request.Role
            };

            DbContext.Employees.Add(newEmployee);
            await DbContext.SaveChangesAsync();
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
            var user = DbContext.Set<Employee>().FirstOrDefault(x => x.empEmail == request.Email);
            if (user == null) return Unauthorized("Invalid email or password");

            var passwordVerification = _passwordHasher.VerifyHashedPassword(null, user.empPassword, request.Password);
            if (passwordVerification == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password");

            if (string.IsNullOrEmpty(user.Role))
                return Unauthorized("User does not have a role assigned.");

            bool isSuperHr = user.Role == "SuperHr";
            if (!isSuperHr) return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.empId.ToString()),
                    new Claim(ClaimTypes.Name, user.empName),
                    new Claim(ClaimTypes.Email, user.empEmail),
                    new Claim(ClaimTypes.Role, user.Role)
                }, authenticationType: "Bearer"),
                Issuer = jwtOptions.Issuer,
                Audience = jwtOptions.Audience,
                Expires = DateTime.UtcNow.AddMinutes(jwtOptions.Lifetime),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    SecurityAlgorithms.HmacSha256)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return Ok(new
            {
                Token = accessToken,
                UserId = user.empId,
                Role = user.Role,
                IsSuperHr = isSuperHr
            });
        }
        
    }
}
