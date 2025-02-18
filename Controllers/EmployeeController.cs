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


    }
}