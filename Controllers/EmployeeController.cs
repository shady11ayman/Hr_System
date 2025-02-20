using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hr_System_Demo_3.DTOs.Employee;
using Hr_System_Demo_3.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Hr_System_Demo_3.DTOs.Auth;

namespace Hr_System_Demo_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeController(AppDbContext DbContext) : ControllerBase
    {
        private readonly PasswordHasher<Employee> _passwordHasher = new();

        [HttpPost("add-employee")]
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

            var hrId = Guid.Parse(hrIdClaim);
            var isHr = DbContext.Employees.Any(e => e.empId == hrId && e.Position.Name == "Hr");

            if (!isHr)
            {
                return Unauthorized("Invalid HR credentials");
            }

            // Calculate salary deductions
            decimal totalDeductions = request.Salary * (request.InsuranceRate + request.TaxRate + request.MedicalInsuranceRate) / 100;
            decimal netSalary = request.Salary - totalDeductions;

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
                PositionId = request.PositionId,
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

            // ✅ **Create Default First Salary Statement**
            var firstSalaryStatement = new SalaryStatement
            {
                EmployeeId = newEmployee.empId,
                StatementDate = DateTime.UtcNow,
                TotalSalary = request.Salary,
                TotalDeductions = totalDeductions,
                NetSalary = netSalary,
                State = SalaryStatementState.Pending, // Set default state to Pending
                HrId = hrId,
                ManagerId = request.ManagerId,
                
            };

            DbContext.SalaryStatements.Add(firstSalaryStatement);
            await DbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Employee added successfully",
                EmployeeId = newEmployee.empId,
                SalaryStatementId = firstSalaryStatement.Id
            });
        }

        [HttpPut("edit-employee/{id}")]
        public async Task<ActionResult> EditEmployee(Guid id, HrEditRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data");
            }

            // Extract Hr_Id from the token
            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials");
            }

            var hrId = Guid.Parse(hrIdClaim);
            var isHr = DbContext.Employees.Any(e => e.empId == hrId && e.Position.Name == "Hr");

            if (!isHr)
            {
                return Unauthorized("Invalid HR credentials");
            }

            var employee = await DbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            // Update fields if provided
            employee.empName = string.IsNullOrEmpty(request.UserName) ? employee.empName : request.UserName;
            employee.empEmail = string.IsNullOrEmpty(request.Email) ? employee.empEmail : request.Email;
            employee.deptId = request.deptId ?? employee.deptId;
            employee.ManagerId = request.ManagerId ?? employee.ManagerId; 
            employee.ShiftTypeId = request.ShiftTypereq ?? employee.ShiftTypeId;
            employee.PhoneNumber = string.IsNullOrEmpty(request.PhoneNumber) ? employee.PhoneNumber : request.PhoneNumber;
            employee.PositionId = request.PositionId ?? employee.PositionId;
            employee.salary = request.Salary ?? employee.salary;
            employee.WorkHours = request.WorkHours ?? employee.WorkHours;
            employee.ContractStart = request.ContractStart ?? employee.ContractStart;
            employee.ContractEnd = request.ContractEnd ?? employee.ContractEnd;

            if (!string.IsNullOrEmpty(request.Password))
            {
                employee.empPassword = _passwordHasher.HashPassword(employee, request.Password);
            }

            await DbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Employee updated successfully",
                EmployeeId = employee.empId
            });
        }


        [HttpGet("employees-by-hr")]
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

        [HttpGet("attendance/{userId}")]
        [Authorize(Roles = "Admin, User, Manager")]
        public async Task<ActionResult> GetAttendance(Guid userId, [FromQuery] int page = 1)
        {
            if (page < 1)
            {
                return BadRequest("Page number must be 1 or greater.");
            }

            // Check if the user exists
            var employee = await DbContext.Employees
                .Include(e => e.ShiftType)
                .FirstOrDefaultAsync(e => e.empId == userId);

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            // Get total count of attendance records for pagination
            var totalRecords = await DbContext.ScanRecords
                .Where(s => s.EmployeeId == userId)
                .CountAsync();

            if (totalRecords == 0)
            {
                return NotFound("No attendance records found for this employee.");
            }

            // Retrieve paginated attendance records (latest first)
            var attendanceRecords = await DbContext.ScanRecords
                .Where(s => s.EmployeeId == userId)
                .OrderByDescending(s => s.Date)  // Newest records first
                .Skip((page - 1) * 10)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                EmployeeId = userId,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalRecords / 10.0),
                AttendanceRecords = attendanceRecords.Select(s => new
                {
                    s.Date,
                    EntryTime = s.EntryTime,
                    ExitTime = s.ExitTime,
                    s.ipAddress
                })
            });
        }


    }
}