using Hr_System_Demo_3.lookups;
using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hr_System_Demo_3.Controllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize(Roles = "SuperHr")]
    [EnableCors("AllowConfiguredOrigins")]
      [AllowAnonymous]
    public class LookupController(AppDbContext DbContext) : ControllerBase
    {
        
        [HttpGet("positions")]
        public async Task<ActionResult<IEnumerable<Position>>> GetPositions()
        {
            return Ok(await DbContext.Positions.ToListAsync());
        }

        [HttpPost("positions")]
        [Authorize (Roles= "Admin")]
        public async Task<ActionResult> AddPosition([FromBody] Position position)
        {
            if (string.IsNullOrWhiteSpace(position.Name))
                return BadRequest("Position name is required.");

            DbContext.Positions.Add(position);
            await DbContext.SaveChangesAsync();
            return Ok("Position added successfully.");
        }

        [HttpPut("positions/{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> UpdatePosition(int id, [FromBody] Position position)
        {
            var existing = await DbContext.Positions.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = position.Name;
            await DbContext.SaveChangesAsync();
            return Ok("Position updated successfully.");
        }

        [HttpDelete("positions/{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> DeletePosition(int id)
        {
            var existing = await DbContext.Positions.FindAsync(id);
            if (existing == null) return NotFound();

            DbContext.Positions.Remove(existing);
            await DbContext.SaveChangesAsync();
            return Ok("Position deleted successfully.");
        }
        
        [HttpGet("shift-types")]
        public async Task<ActionResult<IEnumerable<ShiftType>>> GetShiftTypes()
        {
            return Ok(await DbContext.ShiftTypes.ToListAsync());
        }

        [HttpPost("shift-types")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> AddShiftType([FromBody] ShiftType shiftType)
        {
            if (string.IsNullOrWhiteSpace(shiftType.Name))
                return BadRequest("Shift type name is required.");

            DbContext.ShiftTypes.Add(shiftType);
            await DbContext.SaveChangesAsync();
            return Ok("Shift type added successfully.");
        }

        [HttpPut("shift-types/{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> UpdateShiftType(int id, [FromBody] ShiftType shiftType)
        {
            var existing = await DbContext.ShiftTypes.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = shiftType.Name;
            existing.StartTime = shiftType.StartTime;
            existing.EndTime = shiftType.EndTime;
            await DbContext.SaveChangesAsync();
            return Ok("Shift type updated successfully.");
        }

        [HttpDelete("shift-types/{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> DeleteShiftType(int id)
        {
            var existing = await DbContext.ShiftTypes.FindAsync(id);
            if (existing == null) return NotFound();

            DbContext.ShiftTypes.Remove(existing);
            await DbContext.SaveChangesAsync();
            return Ok("Shift type deleted successfully.");
        }
        
        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            var departments = await DbContext.Departments
                .Select(d => new Department { deptId = d.deptId, deptName = d.deptName })
                .ToListAsync();
            return Ok(departments);
        }


        [HttpPost("add-department")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddDepartment([FromBody] Department department)
        {
            if (string.IsNullOrEmpty(department.deptName))
                return BadRequest("Department name is required.");

            var newDepartment = new Department
            {
                deptId = Guid.NewGuid(),
                deptName = department.deptName,
                Employees = null  // Ensure employees are not required
            };

            DbContext.Departments.Add(newDepartment);
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Department added successfully", DepartmentId = newDepartment.deptId });
        }


        [HttpPut("update-department/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateDepartment(Guid id, [FromBody] Department department)
        {
            var existingDepartment = await DbContext.Departments.FindAsync(id);
            if (existingDepartment == null)
                return NotFound("Department not found.");

            if (string.IsNullOrEmpty(department.deptName))
                return BadRequest("Department name is required.");

            existingDepartment.deptName = department.deptName;
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Department updated successfully", DepartmentId = existingDepartment.deptId });
        }


        [HttpDelete("delete-department/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDepartment(Guid id)
        {
            var department = await DbContext.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.deptId == id);
            if (department == null)
                return NotFound("Department not found.");

            if (department.Employees.Any())
                return BadRequest("Cannot delete department with assigned employees.");

            DbContext.Departments.Remove(department);
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Department deleted successfully", DepartmentId = id });
        }

        [HttpGet("contract-types")]
        public async Task<ActionResult<IEnumerable<ContractType>>> GetContractTypes()
        {
            return Ok(await DbContext.ContractTypes.ToListAsync());
        }

        [HttpPost("contract-types")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> AddContractType([FromBody] ContractType contractType)
        {
            if (string.IsNullOrWhiteSpace(contractType.Name))
                return BadRequest("Contract type name is required.");

            DbContext.ContractTypes.Add(contractType);
            await DbContext.SaveChangesAsync();
            return Ok("Contract type added successfully.");
        }

        [HttpPut("contract-types/{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> UpdateContractType(int id, [FromBody] ContractType contractType)
        {
            var existing = await DbContext.ContractTypes.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = contractType.Name;
            await DbContext.SaveChangesAsync();
            return Ok("Contract type updated successfully.");
        }

        [HttpDelete("contract-types/{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> DeleteContractType(int id)
        {
            var existing = await DbContext.ContractTypes.FindAsync(id);
            if (existing == null) return NotFound();

            DbContext.ContractTypes.Remove(existing);
            await DbContext.SaveChangesAsync();
            return Ok("Contract type deleted successfully.");
        }

        [HttpGet("leave-types")]
        public async Task<ActionResult<IEnumerable<LeaveType>>> GetLeaveTypes()
        {
            return Ok(await DbContext.LeaveTypes.ToListAsync());
        }

        [HttpPost("leave-types")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> AddLeaveType([FromBody] LeaveType leaveType)
        {
            if (string.IsNullOrWhiteSpace(leaveType.Name))
                return BadRequest("Leave type name is required.");

            DbContext.LeaveTypes.Add(leaveType);
            await DbContext.SaveChangesAsync();
            return Ok("Leave type added successfully.");
        }

        [HttpPut("leave-types/{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> UpdateLeaveType(int id, [FromBody] LeaveType leaveType)
        {
            var existing = await DbContext.LeaveTypes.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = leaveType.Name;
            await DbContext.SaveChangesAsync();
            return Ok("Leave type updated successfully.");
        }

        [HttpDelete("leave-types/{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> DeleteLeaveType(int id)
        {
            var existing = await DbContext.LeaveTypes.FindAsync(id);
            if (existing == null) return NotFound();

            DbContext.LeaveTypes.Remove(existing);
            await DbContext.SaveChangesAsync();
            return Ok("Leave type deleted successfully.");
        }
    }
}
