using Hr_System_Demo_3.DTOs.Deduction;
using Hr_System_Demo_3.DTOs.SalaryStatment;
using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hr_System_Demo_3.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class SalaryStatementController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public SalaryStatementController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet("salary-statements/{employeeId}")]

        public async Task<ActionResult<IEnumerable<SalaryStatementDto>>> GetEmployeeSalaryStatements(Guid employeeId)
        {
            var statements = await _dbContext.SalaryStatements
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.StatementDate)
                .Include(s => s.Deductions) // Make sure to include deductions
                .ToListAsync();

            if (!statements.Any()) return NotFound("No salary statements found.");

            // Map to DTO and calculate NetSalary and SalaryAfterDeductions
            var statementDtos = statements.Select(s => new SalaryStatementDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                StatementDate = s.StatementDate,
                TotalSalary = s.TotalSalary,
                TotalDeductions = s.TotalDeductions,
                NetSalary = s.NetSalary, // Calculate Net Salary
                SalaryAfterDeductions = s.NetSalary - s.TotalDeductions, 
                State = s.State,
                HrId = s.HrId,
                ManagerId = s.ManagerId,
                ApprovedDate = s.ApprovedDate,
                PaidDate = s.PaidDate,
                Deductions = s.Deductions.Select(d => new DeductionDto
                {
                    Id = d.Id,
                    EmployeeId = d.EmployeeId,
                    Date = d.Date,
                    Reason = d.Reason,
                    PenaltyAmount = d.PenaltyAmount,
                    ManagerId = d.ManagerId,
                    HrId = d.HrId,
                    State = d.state // Use correct enum
                }).ToList()
            }).ToList();

            return Ok(statementDtos);
        }

        [HttpGet("salary-statement/{salaryStatementId}")]
        public async Task<ActionResult<SalaryStatementDto>> GetSalaryStatementDetails(int salaryStatementId)
        {
            var statement = await _dbContext.SalaryStatements
                .Include(s => s.Deductions)
                .FirstOrDefaultAsync(s => s.Id == salaryStatementId);

            if (statement == null) return NotFound("Salary statement not found.");

            // Map to DTO and calculate NetSalary and SalaryAfterDeductions
            var statementDto = new SalaryStatementDto
            {
                Id = statement.Id,
                EmployeeId = statement.EmployeeId,
                StatementDate = statement.StatementDate,
                TotalSalary = statement.TotalSalary,
                TotalDeductions = statement.TotalDeductions,
                NetSalary = statement.NetSalary, // Calculate Net Salary
                SalaryAfterDeductions = (statement.NetSalary - statement.TotalDeductions), // Calculate Salary After Deductions
                State = statement.State,
                HrId = statement.HrId,
                ManagerId = statement.ManagerId,
                ApprovedDate = statement.ApprovedDate,
                PaidDate = statement.PaidDate,
                Deductions = statement.Deductions?.Select(d => new DeductionDto
                {
                    Id = d.Id,
                    EmployeeId = d.EmployeeId,
                    Date = d.Date,
                    Reason = d.Reason,
                    PenaltyAmount = d.PenaltyAmount,
                    ManagerId = d.ManagerId,
                    HrId = d.HrId,
                    State = d.state // Make sure to map the correct enum here (DeductionState)
                }).ToList() ?? new List<DeductionDto>() // Ensure we handle null deductions gracefully
            };

            return Ok(statementDto);
        }

        [HttpPost("approve-statement/{salaryStatementId}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> ApproveSalaryStatement(int salaryStatementId)
        {
            var salaryStatement = await _dbContext.SalaryStatements.FindAsync(salaryStatementId);

            if (salaryStatement == null)
            {
                return NotFound("Salary statement not found.");
            }

            if (salaryStatement.State != SalaryStatementState.Submitted)
            {
                return BadRequest("Only submitted salary statements can be approved.");
            }

            var managerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(managerIdClaim))
            {
                return Unauthorized("Invalid Manager credentials.");
            }

            salaryStatement.State = SalaryStatementState.Approved;
            salaryStatement.ManagerId = Guid.Parse(managerIdClaim);
            salaryStatement.ApprovedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Salary statement approved successfully." });
        }

        [HttpPost("submit-statement/{salaryStatementId}")]
        [Authorize]
        public async Task<ActionResult> SubmitSalaryStatement(int salaryStatementId)
        {
            var salaryStatement = await _dbContext.SalaryStatements.FindAsync(salaryStatementId);
            if (salaryStatement == null)
            {
                return NotFound("Salary statement not found.");
            }

            if (salaryStatement.State != SalaryStatementState.Pending)
            {
                return BadRequest("Only pending salary statements can be submitted.");
            }

            salaryStatement.State = SalaryStatementState.Submitted;
            salaryStatement.StatementDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Salary statement submitted successfully." });
        }

        [HttpPost("finalize-payment")]
        public async Task<ActionResult> FinalizePayment([FromBody] FinalizePaymentDto request)
        {
            var salaryStatement = await _dbContext.SalaryStatements
                .Include(s => s.Deductions)
                .FirstOrDefaultAsync(s => s.Id == request.SalaryStatementId);

            if (salaryStatement == null)
            {
                return NotFound("Salary statement not found.");
            }

            if (salaryStatement.State != SalaryStatementState.Approved)
            {
                return BadRequest("Salary statement must be approved before finalizing payment.");
            }

            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials.");
            }

            // Finalize the current salary statement
            salaryStatement.State = SalaryStatementState.Paid;
            salaryStatement.PaidDate = DateTime.UtcNow;

            // Iterate over deductions and mark non-approved ones as rejected
            foreach (var deduction in salaryStatement.Deductions)
            {
                if (deduction.state != DeductionState.Approved)
                {
                    deduction.state = DeductionState.Rejected;  // Mark as rejected if not approved
                }

                // Mark deductions as finalized
                deduction.isFinalized = true;
            }

            // Create a new salary statement for the employee
            var newSalaryStatement = new SalaryStatement
            {
                EmployeeId = salaryStatement.EmployeeId,
                StatementDate = DateTime.UtcNow,
                TotalSalary = salaryStatement.TotalSalary, // You can adjust this based on business logic
                TotalDeductions = 0, // Start with 0 deductions
                NetSalary = salaryStatement.NetSalary, // Initial net salary (before deductions)
                HrId = Guid.Parse(hrIdClaim),
                State = SalaryStatementState.Pending,
                ManagerId = salaryStatement.ManagerId,
            };

            _dbContext.SalaryStatements.Add(newSalaryStatement);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Payment finalized successfully. New salary statement created." });
        }

        [HttpGet("my-salary-statements")]
        public async Task<ActionResult<IEnumerable<SalaryStatementDto>>> GetMySalaryStatements()
        {
            // Get the employee's ID from the logged-in user's claims
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim))
            {
                return Unauthorized("User not authenticated.");
            }

            var employeeId = Guid.Parse(employeeIdClaim);

            // Fetch the salary statements for the employee
            var salaryStatements = await _dbContext.SalaryStatements
                .Include(s => s.Deductions.Where(d => d.state == DeductionState.Approved)) // Filter deductions directly in the query
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.StatementDate)
                .ToListAsync();

            if (!salaryStatements.Any())
            {
                return NotFound("No salary statements found for this employee.");
            }

            // Map to DTO and calculate NetSalary and SalaryAfterDeductions
            var statementDtos = salaryStatements.Select(s => new SalaryStatementDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                StatementDate = s.StatementDate,
                TotalSalary = s.TotalSalary,
                TotalDeductions = s.TotalDeductions,
                NetSalary = s.NetSalary, // Assuming the NetSalary is already calculated and stored
                SalaryAfterDeductions = s.NetSalary - s.TotalDeductions, // Calculate Salary After Deductions
                State = s.State,
                HrId = s.HrId,
                ManagerId = s.ManagerId,
                ApprovedDate = s.ApprovedDate,
                PaidDate = s.PaidDate,
                Deductions = s.Deductions.Select(d => new DeductionDto
                {
                    Id = d.Id,
                    EmployeeId = d.EmployeeId,
                    Date = d.Date,
                    Reason = d.Reason,
                    PenaltyAmount = d.PenaltyAmount,
                    ManagerId = d.ManagerId,
                    HrId = d.HrId,
                    State = d.state
                }).ToList() // Map the approved deductions to DTO
            }).ToList();

            return Ok(statementDtos);
        }

    }
}
