using Hr_System_Demo_3.Authentication;
using Hr_System_Demo_3.Day_off_requests;
using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
    [EnableCors("AllowConfiguredOrigins")]
    public class UserController(AppDbContext DbContext, JwtOptions jwtOptions) : ControllerBase
    {
        private readonly PasswordHasher<Employee> _passwordHasher = new();

       /* [HttpPost("add-employee")]
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
            });*/

            [HttpPost("add-employee-application")]
            [Authorize(Roles = "HrEmp, SuperHr")]
            [AllowAnonymous]
            public async Task<ActionResult> AddEmployeeApplication(HrRequest request)
            {
              /*  var hrId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (hrId == null) return Unauthorized("Invalid HR credentials");*/

                var newApplication = new EmployeeApplication
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    PasswordHash = _passwordHasher.HashPassword(null, request.Password),
                    deptId = request.deptId,
                    HrId = Guid.Parse("a8f83315-8c71-47cd-b42b-c95c4acdf7a1"), //Guid.Parse(hrId),
                    Role = request.Role,
                    Status = "Pending",
                    ShiftTypeId = request.ShiftTypereq

                };

                DbContext.EmployeeApplications.Add(newApplication);
                await DbContext.SaveChangesAsync();
                return Ok(new { Message = "Employee application submitted successfully", ApplicationId = newApplication.Id });
            }
        
        [HttpGet("employee-applications")]
        [Authorize(Roles = "SuperHr")]
        public async Task<ActionResult<IEnumerable<EmployeeApplication>>> GetEmployeeApplications()
        {
            var applications = await DbContext.EmployeeApplications
                .Where(e => e.Status == "Pending")
                .ToListAsync();

            return Ok(applications);
        }

        [HttpPost("employee-application-action")]
         [Authorize(Roles = "SuperHr")]
        [AllowAnonymous]
        public async Task<ActionResult> EmployeeApplicationAction(int applicationId, bool isApproved, string? rejectReason = null)
        {
            var application = await DbContext.EmployeeApplications.FindAsync(applicationId);
            if (application == null) return NotFound("Employee application not found");

            if (application.Status != "Pending")
                return BadRequest("Application has already been processed");

            if (isApproved)
            {
                // Move the employee from applications to Employees table
                var newEmployee = new Employee
                {
                    empName = application.UserName,
                    empEmail = application.Email,
                    empPassword = application.PasswordHash,
                    deptId = application.deptId,
                    Hr_Id = application.HrId,
                    Role = application.Role,
                    ShiftTypeId = application.ShiftTypeId
                };


                DbContext.Employees.Add(newEmployee);
                application.Status = "Approved";
            }
            else
            {
                application.Status = "Rejected";
                application.RejectReason = rejectReason;
            }

            await DbContext.SaveChangesAsync();
            return Ok(new { Message = "Application processed successfully", ApplicationId = application.Id, Status = application.Status });
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

        public class UserIdDto
        {
            public Guid UserId { get; set; }
        }

        private decimal CalculateDeduction(TimeSpan timeDifference)
        {
            double minutesLate = timeDifference.TotalMinutes;

            if (minutesLate <= 5) return 0;  // No deduction for <= 5 minutes
            if (minutesLate <= 15) return 5; // Small deduction
            if (minutesLate <= 30) return 10;
            return 20; // Large deduction
        }


        [HttpPost("scan")]
        [AllowAnonymous]
        public async Task<ActionResult> Scan(UserIdDto request)
        {
            var userId = request.UserId;
            var employee = await DbContext.Employees
                .Include(e => e.ShiftType)
                .FirstOrDefaultAsync(e => e.empId == userId);

            if (employee == null) return NotFound("User not found");

            DateTime today = DateTime.UtcNow.Date;
            var scanRecord = await DbContext.ScanRecords
                .FirstOrDefaultAsync(s => s.EmployeeId == userId && s.Date == today);

            DateTime currentTime = DateTime.UtcNow;
            TimeSpan shiftStart = employee.ShiftType.StartTime;
            TimeSpan shiftEnd = employee.ShiftType.EndTime;

            if (scanRecord == null)
            {
                scanRecord = new ScanRecord
                {
                    EmployeeId = userId,
                    Date = today,
                    EntryTime = currentTime,
                    ExitTime = null
                };

                DbContext.ScanRecords.Add(scanRecord);
                await DbContext.SaveChangesAsync();

                if (currentTime.TimeOfDay > shiftStart)
                {
                    // **Late Arrival Deduction**
                    var delay = currentTime.TimeOfDay - shiftStart;
                    var deductionAmount = CalculateDeduction(delay);  // Corrected function name

                    if (deductionAmount > 0)
                    {
                        var deduction = new Deduction
                        {
                            EmployeeId = userId,
                            DeptId = employee.deptId,
                            Date = today,
                            EntryTime = currentTime,
                            ExitTime = null,
                            Reason = "Late Arrival",
                            PenaltyAmount = deductionAmount
                        };
                        DbContext.Deductions.Add(deduction);
                        await DbContext.SaveChangesAsync();
                    }
                }

                return Ok(new { Message = "Entry scan recorded successfully", EntryTime = scanRecord.EntryTime });
            }
            else
            {
                if (scanRecord.ExitTime == null)
                {
                    scanRecord.ExitTime = currentTime;
                    await DbContext.SaveChangesAsync();

                    if (currentTime.TimeOfDay < shiftEnd)
                    {
                        // **Early Leave Deduction**
                        var earlyLeaveDuration = shiftEnd - currentTime.TimeOfDay;
                        var deductionAmount = CalculateDeduction(earlyLeaveDuration);

                        if (deductionAmount > 0)
                        {
                            var deduction = new Deduction
                            {
                                EmployeeId = userId,
                                DeptId = employee.deptId,
                                Date = today,
                                EntryTime = scanRecord.EntryTime,
                                ExitTime = currentTime,
                                Reason = "Early Leave",
                                PenaltyAmount = deductionAmount
                            };
                            DbContext.Deductions.Add(deduction);
                            await DbContext.SaveChangesAsync();
                        }
                    }

                    return Ok(new { Message = "Exit scan recorded successfully", ExitTime = scanRecord.ExitTime });
                }
                else
                {
                    return BadRequest("You have already scanned for exit today.");
                }
            }
        }

        [HttpGet("deductions/{employeeId}")]
        [Authorize(Roles = "HrEmp, SuperHr")]
        public async Task<ActionResult<IEnumerable<Deduction>>> GetEmployeeDeductions(Guid employeeId)
        {
            var deductions = await DbContext.Deductions
                .Where(d => d.EmployeeId == employeeId)
                .ToListAsync();

            if (!deductions.Any()) return NotFound("No deductions found.");

            return Ok(deductions);
        }

        
        
    }
}
