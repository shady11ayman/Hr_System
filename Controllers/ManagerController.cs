using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hr_System_Demo_3.DTOs.Manager;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Hr_System_Demo_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController(AppDbContext DbContext) : ControllerBase
    {
        private readonly PasswordHasher<Employee> _passwordHasher = new();

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

        [HttpPut("edit-manager/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditManager(Guid id, [FromBody] EditManagerDto managerDto)
        {
            var manager = await DbContext.Managers.FindAsync(id);
            if (manager == null)
            {
                return NotFound("Manager not found.");
            }

            var employee = await DbContext.Employees.FirstOrDefaultAsync(e => e.empId == id);
            if (employee == null)
            {
                return NotFound("Associated employee record not found.");
            }

            // Update fields if they are provided
            if (!string.IsNullOrWhiteSpace(managerDto.Name))
            {
                manager.Name = managerDto.Name;
                employee.empName = managerDto.Name;
            }

            if (!string.IsNullOrWhiteSpace(managerDto.Email))
            {
                employee.empEmail = managerDto.Email;
            }

            if (!string.IsNullOrWhiteSpace(managerDto.Password))
            {
                employee.empPassword = _passwordHasher.HashPassword(null, managerDto.Password);
            }

            if (!string.IsNullOrWhiteSpace(managerDto.Address))
            {
                manager.Address = managerDto.Address;
            }

            if (!string.IsNullOrWhiteSpace(managerDto.PhoneNumber))
            {
                manager.PhoneNumber = managerDto.PhoneNumber;
                employee.PhoneNumber = managerDto.PhoneNumber;
            }

            if (managerDto.DepartmentId.HasValue)
            {
                bool departmentExists = await DbContext.Departments.AnyAsync(d => d.deptId == managerDto.DepartmentId);
                if (!departmentExists) return BadRequest("Invalid DepartmentId.");

                manager.DepartmentId = managerDto.DepartmentId.Value;
                employee.deptId = managerDto.DepartmentId.Value;
            }

            await DbContext.SaveChangesAsync();
            return Ok("Manager updated successfully.");
        }

        [HttpGet("manager/{id}")]

        public async Task<IActionResult> GetManagerById(Guid id)
        {
            var manager = await DbContext.Managers.FindAsync(id);
            if (manager == null) return NotFound("Manager not found.");

            return Ok(manager);
        }

        [HttpGet("get-managers")]
        [Authorize(Roles = "Admin,User,Manager")]
        public async Task<IActionResult> GetManagers()
        {
            var managers = await DbContext.Managers
                .Join(DbContext.Employees,
                      m => m.ManagerId,
                      e => e.empId,
                      (m, e) => new
                      {
                          m.ManagerId,
                          m.Name,
                          m.Address,
                          m.PhoneNumber,
                          m.DepartmentId,
                          Email = e.empEmail
                      })
                .ToListAsync();

            if (managers == null || !managers.Any()) return NotFound("There are no managers yet.");

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


    }
}