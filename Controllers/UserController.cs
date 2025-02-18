using Hr_System_Demo_3.Authentication;
using Hr_System_Demo_3.Day_off_requests;
using Hr_System_Demo_3.DTOs.Auth;
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

        [HttpPost("add-employee")]
        [Authorize]
        public async Task<ActionResult> AddEmployee(HrRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Invalid request data");
            }

            // Extract Hr_Id from the token
            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials");
            }

            // Calculate salary deductions
            decimal totalDeductions = request.Salary * (request.InsuranceRate + request.TaxRate + request.MedicalInsuranceRate) / 100;
            decimal netSalary = request.Salary - totalDeductions;

            // Ensure required properties exist
            if (request.ShiftTypereq == 0)
            {
                return BadRequest("Invalid ShiftTypeId");
            }

            var hrId = Guid.Parse(hrIdClaim);
            var isHr = DbContext.Employees
            .Any(e => e.empId == hrId && e.Position.Name == "Hr");

            if (!isHr)
            {
                return Unauthorized("Invalid HR credentials");
            }

            var newEmployee = new Employee
            {
                empId = Guid.NewGuid(),
                empName = request.UserName,
                empEmail = request.Email,
                empPassword = _passwordHasher.HashPassword(null, request.Password),
                deptId = request.deptId,
                Hr_Id = hrId, // Use Hr_Id from token
                Role = "User",
                ManagerId = request.ManagerId,
                ShiftTypeId = request.ShiftTypereq,
                PhoneNumber = request.PhoneNumber,

                PositionId = request.PositionId, // Ensure PositionId is assigned correctly
                ContractTypeId = 1,
                LeaveTypeId = 1,
                ContractStart = DateTime.UtcNow,
                ContractEnd = DateTime.UtcNow.AddYears(1),
                ContractDuration = 12,
                salary = (double)request.Salary,
                WorkHours = 40,
                WorkingDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }
            };

            DbContext.Employees.Add(newEmployee);
            await DbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Employee added successfully",
                EmployeeId = newEmployee.empId
            });
        }
        [HttpPost("add-manager")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddManager([FromBody] ManagerDto managerDto)
        {
            if (managerDto == null)
            {
                return BadRequest("Invalid manager data.");
            }

            // Extract Hr_Id from the token
            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials");
            }

            // Validate DepartmentId if provided
            if (managerDto.DepartmentId.HasValue)
            {
                bool departmentExists = await DbContext.Departments.AnyAsync(d => d.deptId == managerDto.DepartmentId);
                if (!departmentExists) return BadRequest("Invalid DepartmentId.");
            }

            // Ensure DepartmentId is not null
            if (!managerDto.DepartmentId.HasValue)
            {
                return BadRequest("DepartmentId is required.");
            }

            // Generate a single GUID for both Manager and Employee
            var managerId = Guid.NewGuid();

            // Create Manager Entity
            var newManager = new Manager
            {
                ManagerId = managerId, // Use the same GUID for ManagerId
                Name = managerDto.Name,
                Address = managerDto.Address,
                PhoneNumber = managerDto.PhoneNumber,
                DepartmentId = managerDto.DepartmentId.Value, // Use .Value to unwrap the nullable Guid
            };

            // Add the manager to the database
            DbContext.Managers.Add(newManager);

            // Create a corresponding Employee record for the manager
            var newEmployee = new Employee
            {
                empId = managerId, // Use the same GUID for empId
                empName = managerDto.Name,
                empEmail = managerDto.Email, // Ensure Email is provided in ManagerDto
                empPassword = _passwordHasher.HashPassword(null, managerDto.Password), // Ensure Password is provided in ManagerDto
                deptId = managerDto.DepartmentId.Value, // Use .Value to unwrap the nullable Guid
                Hr_Id = Guid.Parse(hrIdClaim), // Use Hr_Id from token
                Role = "Manager", // Set role to Manager
                ManagerId = null, // Managers typically don't have a manager
                ShiftTypeId = 1, // Assign a default shift type (adjust as needed)
                PhoneNumber = managerDto.PhoneNumber,
                PositionId = 5, // Static PositionId for Manager
                ContractTypeId = 1, // Default contract type (adjust as needed)
                LeaveTypeId = 1, // Default leave type (adjust as needed)
                ContractStart = DateTime.UtcNow,
                ContractEnd = DateTime.UtcNow.AddYears(1),
                ContractDuration = 12,
                salary = 0, // Set a default salary or allow it to be provided in ManagerDto
                WorkHours = 40,
                WorkingDays = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }
            };

            // Add the employee to the database
            DbContext.Employees.Add(newEmployee);

            // Save changes to the database
            await DbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetManagerById), new { id = newManager.ManagerId }, newManager);
        }
        public class ManagerDto
        {
            public string Name { get; set; }
            public string Email { get; set; } // Required for login
            public string Password { get; set; } // Required for login
            public string? Address { get; set; }
            public string? PhoneNumber { get; set; }
            public Guid? DepartmentId { get; set; }
        }
        [HttpGet("manager/{id}")]

        public async Task<IActionResult> GetManagerById(Guid id)
        {
            var manager = await DbContext.Managers.FindAsync(id);
            if (manager == null) return NotFound("Manager not found.");

            return Ok(manager);
        }

        [HttpGet("get-managers")]
        [Authorize]
        public async Task<IActionResult> GetManagers()
        {
            var managers = await DbContext.Managers.ToListAsync();
            if (managers == null) return NotFound("there is no managers yet.");

            return Ok(managers);
        }
        [HttpDelete("delete-manager/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteManager(Guid id)
        {
            var manager = await DbContext.Managers.FindAsync(id);
            if (manager == null)
            {
                return NotFound("Manager not found.");
            }

            // Check if there are any employees linked to this manager
            bool hasEmployees = await DbContext.Employees.AnyAsync(e => e.ManagerId == id);
            if (hasEmployees)
            {
                return BadRequest("Cannot delete this manager because they are assigned to employees.");
            }

            DbContext.Managers.Remove(manager);
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Manager deleted successfully", ManagerId = id });
        }

        [HttpPut("edit-manager/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditManager(Guid id, [FromBody] ManagerDto managerDto)
        {
            if (managerDto == null)
            {
                return BadRequest("Invalid manager data.");
            }

            var manager = await DbContext.Managers.FindAsync(id);
            if (manager == null)
            {
                return NotFound("Manager not found.");
            }

            // Update manager properties
            manager.Name = managerDto.Name;
            manager.Address = managerDto.Address;
            manager.PhoneNumber = managerDto.PhoneNumber;
            manager.DepartmentId = managerDto.DepartmentId;

            // Save changes to the database
            DbContext.Managers.Update(manager);
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Manager updated successfully", ManagerId = manager.ManagerId });
        }
    




        [HttpPost("add-employee-application")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddEmployeeApplication(HrRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Invalid request data");
            }

            // Extract Hr_Id from the token
            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials");
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
                HrId = Guid.Parse(hrIdClaim), // Use Hr_Id from token
                Role = "User",
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<EmployeeApplication>>> GetEmployeeApplications()
        {
            var applications = await DbContext.EmployeeApplications
                .Where(e => e.Status == "Pending")
                .ToListAsync();

            return Ok(applications);

        }

        [HttpPost("employee-application-action")]
        [Authorize(Roles = "Admin")]
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
        //  [Authorize(Roles ="Admin,User")]
        public ActionResult<string> Login(AuthenticateRequest request)
        {

            // Include the Position when querying the Employee
            var user = DbContext.Employees
                .Include(e => e.Position)
                .FirstOrDefault(x => x.empEmail == request.Email);

            if (user == null) return Unauthorized("Invalid email or password");

            var passwordVerification = _passwordHasher.VerifyHashedPassword(null, user.empPassword, request.Password);
            if (passwordVerification == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password");

            if (string.IsNullOrEmpty(user.Role))
                return Unauthorized("User does not have a role assigned.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.empId.ToString()),
            new Claim(ClaimTypes.Name, user.empName),
            new Claim(ClaimTypes.Email, user.empEmail),
            new Claim(ClaimTypes.Role, user.Role) // Ensure role is included
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
                Position = user.Position?.Name // Assuming Position has a Name property
            });
        }

        [HttpPost("apply-leave-request")]
        [Authorize(Roles = "Admin,User")]
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
                Action = "Pending",
                comment = request.comment,
            };

            DbContext.LeaveRequests.Add(leaveRequest);
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Leave request submitted successfully", LeaveRequestId = leaveRequest.Id });
        }

        [HttpGet("leave-requests")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetAllLeaveRequests()
        {
            var leaveRequests = await DbContext.LeaveRequests
                .Include(lr => lr.Employee) // Include related employee data
                .ToListAsync();

            return Ok(leaveRequests);
        }

        [HttpGet("my-leave-requests")]
        [Authorize(Roles = "Admin,User")]
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

        [HttpGet("hr-employee-leave-requests")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetHrEmployeeLeaveRequests()
        {
            // Extract HR ID from the token
            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials");
            }

            var hrId = Guid.Parse(hrIdClaim);

            // Find all employees added by this HR
            var employeeIds = await DbContext.Employees
                .Where(e => e.ManagerId == hrId)
                .Select(e => e.empId)
                .ToListAsync();

            if (!employeeIds.Any())
            {
                return NotFound("No employees found for this HR.");
            }

            // Get all leave requests for these employees
            var leaveRequests = await DbContext.LeaveRequests
                .Where(lr => employeeIds.Contains(lr.EmployeeId))
                .ToListAsync();

            if (!leaveRequests.Any())
            {
                return NotFound("No leave requests found for employees assigned by this HR.");
            }

            return Ok(leaveRequests);
        }

        [HttpPost("leave-request-action")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> LeaveRequestAction([FromBody] LeaveRequestActionModel model)
        {
            var leaveRequest = await DbContext.LeaveRequests.FindAsync(model.LeaveRequestId);
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

            if (model.IsApproved)
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
                leaveRequest.RejectReason = model.RejectReason ?? "No reason provided";
            }

            await DbContext.SaveChangesAsync();
            return Ok(new { Message = "Leave request processed successfully", LeaveRequestId = leaveRequest.Id, Status = leaveRequest.Status });
        }

        public class LeaveRequestActionModel
        {
            public int LeaveRequestId { get; set; }
            public bool IsApproved { get; set; }
            public string? RejectReason { get; set; }
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
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult> Scan()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Invalid user credentials");
            }

            var userId = Guid.Parse(userIdClaim);
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

        public class DeductionActionRequest
        {
            public int DeductionId { get; set; }
            public DeductionState Action { get; set; }
        }

        [HttpPost("take-deduction-action")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> TakeDeductionAction([FromBody] DeductionActionRequest request)
        {
            var deduction = await DbContext.Deductions.FindAsync(request.DeductionId);
            if (deduction == null) return NotFound("Deduction not found");

            if (deduction.state != DeductionState.submited)
                return BadRequest("Deduction has already been processed");

            if (request.Action != DeductionState.Approved && request.Action != DeductionState.Rejected)
                return BadRequest("Invalid action. Only 'Approved' or 'Rejected' are allowed.");

            deduction.state = request.Action;
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = $"Deduction {request.Action} successfully" });
        }

        //[HttpPost("finalize-deduction")]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult> FinalizeDeduction(int deductionId, bool isApproved, string? rejectReason = null)
        //{
        //    var deduction = await DbContext.Deductions.FindAsync(deductionId);
        //    if (deduction == null) return NotFound("Deduction not found");

        //    if (deduction.state != DeductionState.Approved)
        //        return BadRequest("Deduction has not been approved by the Manager yet");

        //    if (isApproved)
        //    {
        //        deduction.state = DeductionState.Approved;
        //        var salaryAfterDeduction = new SalaryAfterDeductions
        //        {
        //            empId = deduction.EmployeeId,
        //            Salaryafterchange = (double)deduction.PenaltyAmount
        //        };
        //        //DbContext.SalaryAfterDeductions.Add(salaryAfterDeduction);
        //    }
        //    else
        //    {
        //        deduction.state = DeductionState.Rejected;
        //        deduction.Reason = rejectReason ?? "No reason provided";
        //    }

        //    await DbContext.SaveChangesAsync();
        //    return Ok(new { Message = "Deduction finalized", Status = deduction.state });
        //}

        [HttpGet("deductions/{employeeId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Deduction>>> GetEmployeeDeductions(Guid employeeId)
        {
            var deductions = await DbContext.Deductions
                .Where(d => d.EmployeeId == employeeId)
                .ToListAsync();

            if (!deductions.Any()) return NotFound("No deductions found.");

            return Ok(deductions);
        }

        [HttpGet("employees-by-hr")]
        [Authorize(Roles = "User, Admin")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByHr()
        {

            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials");
            }


            var hrId = Guid.Parse(hrIdClaim);
            var isHr = DbContext.Employees
                 .Any(e => e.empId == hrId && e.Position.Name == "Hr");

            if (!isHr)
            {
                return Unauthorized("Invalid HR credentials");

            }
            var employees = await DbContext.Employees
                .Where(e => e.Hr_Id == hrId)
                .ToListAsync();

            if (!employees.Any())
            {
                return NotFound("No employees found for this HR.");
            }

            return Ok(employees);
        }

        [HttpGet("employees-by-manager-department")]
        [Authorize(Roles = "Manager, Admin")]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployeesByManagerDepartment()
        {
            var managerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(managerIdClaim))
            {
                return Unauthorized("Invalid manager credentials");
            }

            var managerId = Guid.Parse(managerIdClaim);


           

            // Get employees in the manager's department
            var employees = await DbContext.Employees
                .Where(e => e.ManagerId == managerId)
                .ToListAsync();

            if (!employees.Any())
            {
                return NotFound("No employees found in this manager's department.");
            }

            return Ok(employees);
        }

        public class AssignDeductionDto
        {
            public Guid EmployeeId { get; set; } // The employee to whom the deduction is assigned
            public string Reason { get; set; } // Reason for the deduction (e.g., "Late Arrival", "Early Leave")
            public decimal PenaltyAmount { get; set; } // The amount to deduct
        }

        [HttpPost("assign-deduction")]
        [Authorize]
        public async Task<ActionResult> AssignDeduction([FromBody] AssignDeductionDto request)
        {
            // Validate the request
            if (request == null || request.EmployeeId == Guid.Empty || request.PenaltyAmount <= 0)
            {
                return BadRequest("Invalid deduction data.");
            }

            // Check if the employee exists
            var employee = await DbContext.Employees.FindAsync(request.EmployeeId);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            // Extract HR ID from the token
            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials.");
            }

            var userId = Guid.Parse(hrIdClaim);

            // Check if the user is a Manager
            var isManager = await DbContext.Employees
                .Where(us => us.empId == userId && us.Role != null)
                .AnyAsync(us => us.Role.ToLower() == "manager");

            // Create the deduction record
            var deduction = new Deduction
            {
                EmployeeId = request.EmployeeId,
                DeptId = employee.deptId, // Assign the employee's department
                Date = DateTime.UtcNow,
                Reason = request.Reason,
                PenaltyAmount = request.PenaltyAmount,
                state = isManager? DeductionState.Approved : DeductionState.submited, // Initial state
                HrId = userId, // Track which HR assigned the deduction
                isFinalized = false // Finalize if user is a Manager
            };

            // Add the deduction to the database
            DbContext.Deductions.Add(deduction);
            await DbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Deduction assigned successfully",
                IsFinalized = deduction.isFinalized
            });
        }

    }
}
