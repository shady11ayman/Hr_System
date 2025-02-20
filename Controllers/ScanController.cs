using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel;
using System.Security.Claims;

namespace Hr_System_Demo_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScanController(AppDbContext DbContext) : ControllerBase
    {
        [HttpPost("scan")]
        [Authorize(Roles = "Admin,User")]
       // [AllowAnonymous]
        public async Task<ActionResult> Scan( string ipAddress)
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
            var existingScan = await DbContext.ScanRecords
                .FirstOrDefaultAsync(s => s.EmployeeId == userId && s.Date == today);

            
            var ipAlreadyUsed = await DbContext.ScanRecords
                .AnyAsync(s => s.EmployeeId == userId && s.Date == today && s.ipAddress != ipAddress);

            var ipAlreadyUsedWithDifferentAccount = await DbContext.ScanRecords
               .AnyAsync(s => s.EmployeeId != userId && s.Date == today && s.ipAddress == ipAddress);


            if (ipAlreadyUsed)
            {
                return BadRequest("You must scan from the same device per day.");

                
            }

            if (ipAlreadyUsedWithDifferentAccount) 
            {
                return BadRequest("This IP address is used for another user , stop cheating");
            }
            

            DateTime currentTime = DateTime.UtcNow;
            TimeSpan shiftStart = employee.ShiftType.StartTime;
            TimeSpan shiftEnd = employee.ShiftType.EndTime;

            if (existingScan == null)
            {
                var scanRecord = new ScanRecord
                {
                    EmployeeId = userId,
                    Date = today,
                    EntryTime = currentTime,
                    ExitTime = null,
                    ipAddress = ipAddress
                };

                DbContext.ScanRecords.Add(scanRecord);
                await DbContext.SaveChangesAsync();

                return Ok(new { Message = "Entry scan recorded successfully", EntryTime = scanRecord.EntryTime });
            }
            else
            {
                if (existingScan.ExitTime == null)
                {
                    existingScan.ExitTime = currentTime;
                    await DbContext.SaveChangesAsync();
                    return Ok(new { Message = "Exit scan recorded successfully", ExitTime = existingScan.ExitTime });
                }
                else
                {
                    return BadRequest("You have already scanned for exit today.");
                }
            }
        }

        [HttpPost("process-absences")]
        [Authorize(Roles = "Admin")]
        //[AllowAnonymous]
        public async Task<ActionResult> ProcessAbsences()
        {
            DateTime today = DateTime.UtcNow.Date;
            var employees = await DbContext.Employees.ToListAsync();

            foreach (var employee in employees)
            {
                var scanExists = await DbContext.ScanRecords.AnyAsync(s => s.EmployeeId == employee.empId && s.Date == today);
                if (!scanExists)
                {
                    var latestStatement = await DbContext.SalaryStatements
                        .Where(s => s.EmployeeId == employee.empId && s.State != SalaryStatementState.Paid)
                        .OrderByDescending(s => s.StatementDate)
                        .FirstOrDefaultAsync();

                    if (latestStatement == null) continue;

                    var deduction = new Deduction
                    {
                        EmployeeId = employee.empId,
                        DeptId = employee.deptId,
                        Date = today,
                        Reason = "Full Day Absence",
                        PenaltyAmount = (decimal)(employee.salary / 21),
                        state = DeductionState.submited,
                        SalaryStatementId = latestStatement.Id
                    };

                    DbContext.Deductions.Add(deduction);
                }
            }
            await DbContext.SaveChangesAsync();
            return Ok("Absences processed successfully.");
        }

        [HttpGet("export-salary-statements")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportSalaryStatements()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var salaryStatements = await DbContext.SalaryStatements
                    .Include(s => s.Employee)
                        .ThenInclude(e => e.Manager)
                    .Include(s => s.Deductions) 
                    .ToListAsync();

                var groupedStatements = salaryStatements.GroupBy(s => s.EmployeeId);

                foreach (var group in groupedStatements)
                {
                    var employee = group.First().Employee;
                    var employeeName = employee?.empName ?? "Unknown";
                    var worksheet = package.Workbook.Worksheets.Add($"{employeeName}");

                    // Headers
                    worksheet.Cells[1, 1].Value = "Employee Name";
                    worksheet.Cells[1, 2].Value = "Total Salary";
                    worksheet.Cells[1, 3].Value = "Total Deductions";
                    worksheet.Cells[1, 4].Value = "Net Salary";
                    worksheet.Cells[1, 5].Value = "Manager Name";
                    worksheet.Cells[1, 6].Value = "HR Name";
                    worksheet.Cells[1, 7].Value = "Deduction Date"; 

                    int row = 2;
                    foreach (var statement in group)
                    {
                        var managerName = statement.Employee?.Manager?.Name ?? "N/A";
                        var hr = await DbContext.Employees.FirstOrDefaultAsync(e => e.empId == statement.Employee.Hr_Id);
                        var hrName = hr?.empName ?? "N/A";

                        // Get the most recent deduction date (or "N/A" if none exist)
                        var latestDeductionDate = statement.Deductions?.OrderByDescending(d => d.Date)
                                                   .FirstOrDefault()?.Date.ToString("yyyy-MM-dd") ?? "N/A";

                        worksheet.Cells[row, 1].Value = employeeName;
                        worksheet.Cells[row, 2].Value = statement.TotalSalary;
                        worksheet.Cells[row, 3].Value = statement.TotalDeductions;
                        worksheet.Cells[row, 4].Value = statement.NetSalary;
                        worksheet.Cells[row, 5].Value = managerName;
                        worksheet.Cells[row, 6].Value = hrName;
                        worksheet.Cells[row, 7].Value = latestDeductionDate;

                        row++;
                    }
                }

                var stream = new MemoryStream(package.GetAsByteArray());
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalaryStatementsReport.xlsx");
            }
        }




    }
}



