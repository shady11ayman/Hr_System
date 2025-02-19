using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hr_System_Demo_3.DTOs.Deduction;
using System.Security.Claims;

namespace Hr_System_Demo_3.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class DeductionController(AppDbContext DbContext) : ControllerBase
    {

        [HttpPost("assign-deduction")]
        public async Task<ActionResult> AssignDeduction([FromBody] DTOs.Deduction.AssignDeductionDto request)
        {
            if (request == null || request.EmployeeId == Guid.Empty || request.PenaltyAmount <= 0)
            {
                return BadRequest("Invalid deduction data.");
            }

            var employee = await DbContext.Employees.FindAsync(request.EmployeeId);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials.");
            }

            var userId = Guid.Parse(hrIdClaim);

            var isManager = await DbContext.Employees
                .Where(us => us.empId == userId && us.Role != null)
                .AnyAsync(us => us.Role.ToLower() == "manager");

            // Get the latest unfinalized salary statement for the employee
            var latestStatement = await DbContext.SalaryStatements
                .Where(s => s.EmployeeId == request.EmployeeId && s.State != SalaryStatementState.Paid)
                .OrderByDescending(s => s.StatementDate)
                .FirstOrDefaultAsync();

            if (latestStatement == null)
            {
                return BadRequest("No active salary statement found for the employee.");
            }

            var deduction = new Deduction
            {
                EmployeeId = request.EmployeeId,
                DeptId = employee.deptId,
                Date = DateTime.UtcNow,
                Reason = request.Reason,
                PenaltyAmount = request.PenaltyAmount,
                state = isManager ? DeductionState.Approved : DeductionState.submited,
                HrId = userId,
                isFinalized = false,
                SalaryStatementId = latestStatement.Id // Associate with the latest salary statement
            };
            if (isManager)
            {
                latestStatement.TotalDeductions += request.PenaltyAmount;
            }

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

            // Find the salary statement associated with this deduction
            var salaryStatement = await DbContext.SalaryStatements.FindAsync(deduction.SalaryStatementId);
            if (salaryStatement == null)
                return NotFound("Salary statement not found.");

            // Ensure the salary statement is not already approved or paid
            if (salaryStatement.State == SalaryStatementState.Paid || salaryStatement.State == SalaryStatementState.Approved)
                return BadRequest("Cannot modify deductions for an already paid or approved salary statement.");

            // Update deduction state
            deduction.state = request.Action;

            // If the deduction is approved, update salary statement
            if (request.Action == DeductionState.Approved)
            {
                salaryStatement.TotalDeductions += deduction.PenaltyAmount;
                //salaryStatement.NetSalary -= deduction.PenaltyAmount;
            }

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
  
        [HttpGet("deductions-for-statement/{salaryStatementId}")]
        public async Task<ActionResult<IEnumerable<Deduction>>> GetDeductionsForStatement(int salaryStatementId)
        {
            var deductions = await DbContext.Deductions
                .Where(d => d.SalaryStatementId == salaryStatementId)
                .ToListAsync();

            if (!deductions.Any()) return NotFound("No deductions found for this statement.");

            return Ok(deductions);
        }


        [HttpDelete("delete-deduction/{deductionId}")]
        public async Task<ActionResult> DeleteDeduction(Guid deductionId)
        {
            var deduction = await DbContext.Deductions.FindAsync(deductionId);
            if (deduction == null)
            {
                return NotFound("Deduction not found.");
            }

            // Check if the deduction is approved
            if (deduction.state == DeductionState.Approved)
            {
                var salaryStatement = await DbContext.SalaryStatements.FindAsync(deduction.SalaryStatementId);
                if (salaryStatement == null)
                {
                    return NotFound("Salary statement not found.");
                }

                // Ensure salary statement is not finalized
                if (salaryStatement.State == SalaryStatementState.Paid)
                {
                    return BadRequest("Cannot delete deduction from a finalized salary statement.");
                }

                // Refund the deducted amount
                salaryStatement.TotalDeductions -= deduction.PenaltyAmount;
                //salaryStatement.NetSalary += deduction.PenaltyAmount;
            }

            DbContext.Deductions.Remove(deduction);
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Deduction deleted successfully, and salary statement updated if necessary." });
        }




    }
}