using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hr_System_Demo_3.DTOs.Deduction;
using static Hr_System_Demo_3.Controllers.UserController;
using System.Security.Claims;

namespace Hr_System_Demo_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeductionController(AppDbContext DbContext) : ControllerBase
    {

        [HttpPost("assign-deduction")]
        public async Task<ActionResult> AssignDeduction([FromBody] DTOs.Deduction.AssignDeductionDto request)
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
                state = isManager ? DeductionState.Approved : DeductionState.submited, // Initial state
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



        [HttpPost("take-deduction-action")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> TakeDeductionAction([FromBody] DTOs.Deduction.DeductionActionRequest request)
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

        [HttpGet("deductions/{employeeId}")]
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