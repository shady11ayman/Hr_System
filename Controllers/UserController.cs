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

        [HttpPost("add-manager")]
        public async Task<IActionResult> AddManager([FromBody] ManagerDto managerDto)
        {
            if (managerDto == null)
            {
                return BadRequest("Invalid manager data.");
            }

            // Validate Foreign Keys
            bool positionExists = await DbContext.Positions.AnyAsync(p => p.Id == managerDto.PositionId);
            if (!positionExists) return BadRequest("Invalid PositionId.");

            bool shiftTypeExists = await DbContext.ShiftTypes.AnyAsync(s => s.ShiftTypeId == managerDto.ShiftTypeId);
            if (!shiftTypeExists) return BadRequest("Invalid ShiftTypeId.");

            bool contractTypeExists = await DbContext.ContractTypes.AnyAsync(c => c.Id == managerDto.ContractTypeId);
            if (!contractTypeExists) return BadRequest("Invalid ContractTypeId.");

            bool leaveTypeExists = await DbContext.LeaveTypes.AnyAsync(l => l.Id == managerDto.LeaveTypeId);
            if (!leaveTypeExists) return BadRequest("Invalid LeaveTypeId.");

            if (managerDto.DepartmentId.HasValue)
            {
                bool departmentExists = await DbContext.Departments.AnyAsync(d => d.deptId == managerDto.DepartmentId);
                if (!departmentExists) return BadRequest("Invalid DepartmentId.");
            }

            if (managerDto.DirectManagerId.HasValue)
            {
                bool directManagerExists = await DbContext.Managers.AnyAsync(m => m.ManagerId == managerDto.DirectManagerId);
                if (!directManagerExists) return BadRequest("Invalid DirectManagerId.");
            }

            // Create Manager Entity
            var newManager = new Manager
            {
                ManagerId = Guid.NewGuid(),
                Name = managerDto.Name,
                Address = managerDto.Address,
                Hr_Id = managerDto.Hr_Id,
                PhoneNumber = managerDto.PhoneNumber,
                PositionId = managerDto.PositionId,
                DepartmentId = managerDto.DepartmentId,
                WorkHours = managerDto.WorkHours,
                WorkingDays = managerDto.WorkingDays ?? new List<string>(),
                ShiftTypeId = managerDto.ShiftTypeId,
                ContractTypeId = managerDto.ContractTypeId,
                DirectManagerId = managerDto.DirectManagerId,
                LeaveTypeId = managerDto.LeaveTypeId,
                ContractStart = managerDto.ContractStart,
                ContractEnd = managerDto.ContractEnd
            };

            DbContext.Managers.Add(newManager);
            await DbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetManagerById), new { id = newManager.ManagerId }, newManager);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetManagerById(Guid id)
        {
            var manager = await DbContext.Managers.FindAsync(id);
            if (manager == null) return NotFound("Manager not found.");

            return Ok(manager);
        }


        public class ManagerDto
        {
            public string Name { get; set; }
            public string? Address { get; set; }
            public Guid Hr_Id { get; set; }
            public int? PhoneNumber { get; set; }
            public int PositionId { get; set; }
            public Guid? DepartmentId { get; set; }
            public double? WorkHours { get; set; }
            public List<string>? WorkingDays { get; set; }
            public int ShiftTypeId { get; set; }
            public int ContractTypeId { get; set; }
            public Guid? DirectManagerId { get; set; }
            public int LeaveTypeId { get; set; }
            public DateTime? ContractStart { get; set; }
            public DateTime? ContractEnd { get; set; }
        }




        [HttpPost("add-employee-application")]
       // [Authorize(Roles = "HrEmp, SuperHr")]
        [AllowAnonymous]
        public async Task<ActionResult> AddEmployeeApplication(HrRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Invalid request data");
            }

            // Calculate salary deductions
            decimal totalDeductions = request.Salary * (request.InsuranceRate + request.TaxRate + request.MedicalInsuranceRate) / 100;
            decimal netSalary = request.Salary - totalDeductions;

            var newApplication = new EmployeeApplication
            {
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(null, request.Password),
                deptId = request.deptId,
                HrId = request.Hr_Id,
                Role = request.Role,
                Status = "Pending",
                ShiftTypeId = request.ShiftTypereq,
                Salary = request.Salary,
                InsuranceRate = request.InsuranceRate,
                TaxRate = request.TaxRate,
                MedicalInsuranceRate = request.MedicalInsuranceRate,
                ApprovalStatus = "PendingApproval",
                ManagerId = request.ManagerId,
                PhoneNumber = request.PhoneNumber,
                
            };

            DbContext.EmployeeApplications.Add(newApplication);
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Employee application submitted successfully and awaiting General Manager approval", ApplicationId = newApplication.Id });
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
        // [Authorize(Roles = "Manager")]
           [AllowAnonymous]
            public async Task<ActionResult> EmployeeApplicationAction(int applicationId, bool isApproved, string PhoneNumber, string? rejectReason = null)
            {
                var application = await DbContext.Set<EmployeeApplication>().FindAsync(applicationId);
                if (application == null) return NotFound("Employee application not found");

                if (application.Status != "Pending")
                    return BadRequest("Application has already been processed");

                if (isApproved)
                {
                    // Ensure all required properties exist
                    if (application.ShiftTypeId == 0 || application.HrId == Guid.Empty)
                        return BadRequest("Invalid ShiftTypeId or HrId");

                var newEmployee = new Employee
                {
                    empId = Guid.NewGuid(),
                    empName = application.UserName,
                    empEmail = application.Email,
                    empPassword = application.PasswordHash,
                    deptId = application.deptId,
                    Hr_Id = application.HrId,
                    Role = application.Role,
                    ManagerId = application.ManagerId ?? Guid.Empty,
                    ShiftTypeId = application.ShiftTypeId,
                    PhoneNumber = application.PhoneNumber,

                    PositionId = 1, // ⚠️ FIXED: Ensure PositionId is assigned correctly

                    ContractTypeId = 1,
                    LeaveTypeId = 1,
                    ContractStart = DateTime.UtcNow,
                    ContractEnd = DateTime.UtcNow.AddYears(1),
                    ContractDuration = 12,

                    salary = (double)application.Salary,
                    WorkHours = 40,

                    WorkingDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }
                };

                DbContext.Employees.Add(newEmployee);
                    application.Status = "Approved by Manager";
                }
                else
                {
                    application.Status = "Rejected";
                    application.RejectReason = rejectReason ?? "No reason provided";
                }

                await DbContext.SaveChangesAsync();
                return Ok(new
                {
                    Message = "Application processed successfully",
                    ApplicationId = application.Id,
                    Status = application.Status
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

          /*  bool isSuperHr = user.Role == "SuperHr";
            if (!isSuperHr) return Unauthorized();*/

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
               // IsSuperHr = isSuperHr
            });
        }
        [HttpPost("apply-leave-request")]
        // [Authorize(Roles = "User, HrEmp, SuperHr")]
        [AllowAnonymous]
        public async Task<ActionResult> ApplyLeaveRequest([FromBody] ApplyLeaveRequestDto request)
        {
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employeeNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim) || string.IsNullOrEmpty(employeeNameClaim))
            {
                return Unauthorized("Invalid user credentials");
            }

            // Calculate total days off
            int totalDaysOff = (request.LeaveTo - request.LeaveFrom).Days + 1;

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = Guid.Parse(employeeIdClaim),
                EmployeeName = employeeNameClaim,
                LeaveType = request.LeaveType,
                LeaveFrom = request.LeaveFrom,
                LeaveTo = request.LeaveTo,
                workingDaysOff = totalDaysOff.ToString(), // Assuming all days are working days
                TotalDaysOff = totalDaysOff.ToString(),
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
            var leaveRequests = await DbContext.LeaveRequests
                .Include(lr => lr.Employee) // Include related employee data
                .ToListAsync();

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
                leaveRequest.RejectReason = rejectReason ?? "No reason provided";
            }

            await DbContext.SaveChangesAsync();
            return Ok(new { Message = "Leave request processed successfully", LeaveRequestId = leaveRequest.Id, Status = leaveRequest.Status });
        }

        public class UserIdDto
        {
            public Guid UserId { get; set; }
        }

        private double CalculateDeduction(TimeSpan timeDifference, double salary)
        {
            double minutesLate = timeDifference.TotalMinutes;

            if (minutesLate < 15) return 0;  // No deduction for < 15 mins
            if (minutesLate <= 90) return salary / 21 / 2; // Half-day deduction
            return salary / 21; // Full-day deduction
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

            if (employee.ShiftType == null) return BadRequest("Shift type not assigned to employee");

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
                    var delay = currentTime.TimeOfDay - shiftStart;
                    var deductionAmount = CalculateDeduction(delay, employee.salary);

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
                            PenaltyAmount = (decimal)deductionAmount,
                            state = DeductionState.submited
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
                        var earlyLeaveDuration = shiftEnd - currentTime.TimeOfDay;
                        var deductionAmount = CalculateDeduction(earlyLeaveDuration, employee.salary);

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
                                PenaltyAmount = (decimal)deductionAmount,
                                state = DeductionState.submited,
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

        [HttpPost("approve-deduction")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> ApproveDeduction(int deductionId)
        {
            var deduction = await DbContext.Deductions.FindAsync(deductionId);
            if (deduction == null) return NotFound("Deduction not found");

            if (deduction.state != DeductionState.submited)
                return BadRequest("Deduction has already been processed");

            deduction.state = DeductionState.Approved; // Correct state transition
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Deduction approved by Manager" });
        }

        [HttpPost("finalize-deduction")]
        [Authorize(Roles = "SuperHr")]
        public async Task<ActionResult> FinalizeDeduction(int deductionId, bool isApproved, string? rejectReason = null)
        {
            var deduction = await DbContext.Deductions.FindAsync(deductionId);
            if (deduction == null) return NotFound("Deduction not found");

            if (deduction.state != DeductionState.Approved)
                return BadRequest("Deduction has not been approved by the Manager yet");

            if (isApproved)
            {
                deduction.state = DeductionState.Approved;
                var salaryAfterDeduction = new SalaryAfterDeductions
                {
                    empId = deduction.EmployeeId,
                    Salaryafterchange = (double)deduction.PenaltyAmount
                };
                DbContext.SalaryAfterDeductions.Add(salaryAfterDeduction);
            }
            else
            {
                deduction.state = DeductionState.Rejected;
                deduction.Reason = rejectReason ?? "No reason provided";
            }

            await DbContext.SaveChangesAsync();
            return Ok(new { Message = "Deduction finalized", Status = deduction.state });
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
