using Hr_System_Demo_3.Authentication;
using Hr_System_Demo_3.Day_off_requests;
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
        [HttpPost("apply-leave-request")]
        [Authorize(Roles = "User, HrEmp, SuperHr")]
        public async Task<ActionResult> ApplyLeaveRequest([FromBody] ApplyLeaveRequestDto request)
        {
            
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employeeNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim) || string.IsNullOrEmpty(employeeNameClaim))
            {
                return Unauthorized("Invalid user credentials");
            }

            
            var leaveRequest = new LeaveRequest
            {
                EmployeeId = Guid.Parse(employeeIdClaim),
                EmployeeName = employeeNameClaim,
                LeaveType = request.LeaveType,
                LeaveFrom = request.LeaveFrom,
                LeaveTo = request.LeaveTo,
                Status = LeaveStatus.Pending, 
                Action = "Pending" 
            };

            DbContext.LeaveRequests.Add(leaveRequest);
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Leave request submitted successfully", LeaveRequestId = leaveRequest.Id });
        }

        [HttpGet("leave-requests")]
        [Authorize(Roles = "HrEmp, SuperHr")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetAllLeaveRequests()
        {
            var leaveRequests = await DbContext.LeaveRequests.ToListAsync();
            return Ok(leaveRequests);
        }

        [HttpGet("my-leave-requests")]
        [Authorize(Roles = "User, HrEmp, SuperHr")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetMyLeaveRequests()
        {
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim))
            {
                return Unauthorized("Invalid user credentials");
            }

            var employeeId = Guid.Parse(employeeIdClaim);
            var myLeaveRequests = await DbContext.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId)
                .ToListAsync();

            return Ok(myLeaveRequests);
        }

        [HttpGet("employee-leave-requests/{employeeId}")]
        [Authorize(Roles = "HrEmp, SuperHr")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetEmployeeLeaveRequests(Guid employeeId)
        {
            var leaveRequests = await DbContext.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId && lr.Status == LeaveStatus.Pending)
                .ToListAsync();

            if (!leaveRequests.Any())
            {
                return NotFound("No pending leave requests found for this employee.");
            }

            return Ok(leaveRequests);
        }

        [HttpPost("leave-request-action")]
        [Authorize(Roles = "HrEmp, SuperHr")]
        public async Task<ActionResult> LeaveRequestAction(int leaveRequestId, bool isApproved, string? rejectReason = null)
        {
            var leaveRequest = await DbContext.LeaveRequests.FindAsync(leaveRequestId);
            if (leaveRequest == null)
            {
                return NotFound("Leave request not found");
            }

            var employee = await DbContext.Employees.FindAsync(leaveRequest.EmployeeId);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            if (leaveRequest.Status != LeaveStatus.Pending)
            {
                return BadRequest("Leave request has already been processed");
            }

            if (isApproved)
            {
                leaveRequest.Status = LeaveStatus.Accepted;
                leaveRequest.Action = "Approved";

                int leaveDays = (leaveRequest.LeaveTo - leaveRequest.LeaveFrom).Days + 1;
                leaveRequest.workingDaysOff = leaveDays.ToString();
                leaveRequest.TotalDaysOff = leaveDays.ToString();
            }
            else
            {
                leaveRequest.Status = LeaveStatus.Refused;
                leaveRequest.Action = "Rejected";
                leaveRequest.RejectReason = rejectReason;
            }

            await DbContext.SaveChangesAsync();
            return Ok(new { Message = "Leave request processed successfully", LeaveRequestId = leaveRequest.Id, Status = leaveRequest.Status });
        }



    }
}
