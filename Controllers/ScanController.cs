using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportSalaryStatements(int year, int month)
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var salaryStatements = await DbContext.SalaryStatements
                        .Include(s => s.Employee)
                            .ThenInclude(e => e.Manager)
                        .Include(s => s.Deductions)
                        .Where(s => s.StatementDate.Year == year && s.StatementDate.Month == month)
                        .ToListAsync();

                    if (!salaryStatements.Any())
                    {
                        return BadRequest(new
                        {
                            Status = "Error",
                            Message = "No salary statements found for the given year and month.",
                            Year = year,
                            Month = month
                        });
                    }

                    var groupedStatements = salaryStatements.GroupBy(s => s.EmployeeId);

                    if (!groupedStatements.Any())
                    {
                        return BadRequest(new
                        {
                            Status = "Error",
                            Message = "No valid employee data available for export.",
                            EmployeeCount = salaryStatements.Select(s => s.EmployeeId).Distinct().Count()
                        });
                    }

                    foreach (var group in groupedStatements)
                    {
                        var employee = group.First().Employee;
                        var employeeName = employee?.empName ?? "Unknown";
                        var worksheet = package.Workbook.Worksheets.Add(employeeName);

                        worksheet.Cells[1, 1].Value = "Deduction Date";
                        worksheet.Cells[1, 2].Value = "Reason";
                        worksheet.Cells[1, 3].Value = "Penalty Amount";
                        worksheet.Cells[1, 4].Value = "Assigned by";

                        int row = 2;
                        foreach (var statement in group)
                        {
                            var approvedDeductions = statement.Deductions?
                                .Where(d => d.state == DeductionState.Approved)
                                .ToList();

                            if (approvedDeductions != null && approvedDeductions.Any())
                            {
                                foreach (var deduction in approvedDeductions)
                                {
                                    var hr = await DbContext.Employees.FirstOrDefaultAsync(e => e.empId == statement.HrId);
                                    worksheet.Cells[row, 1].Value = deduction.Date.ToString("yyyy-MM-dd");
                                    worksheet.Cells[row, 2].Value = deduction.Reason;
                                    worksheet.Cells[row, 3].Value = deduction.PenaltyAmount;
                                    worksheet.Cells[row, 4].Value = hr?.empName ?? "N/A";
                                    row++;
                                }
                            }
                        }

                        row++;
                        worksheet.Cells[row, 1].Value = "Statement Details";
                        worksheet.Cells[row, 1].Style.Font.Bold = true;
                        worksheet.Cells[row + 1, 1].Value = "Salary";
                        worksheet.Cells[row + 1, 2].Value = group.First().NetSalary;
                        worksheet.Cells[row + 1, 2].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row + 2, 1].Value = "Total Deductions";
                        worksheet.Cells[row + 2, 2].Value = group.First().TotalDeductions;
                        worksheet.Cells[row + 2, 2].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row + 3, 1].Value = "Final";
                        worksheet.Cells[row + 3, 2].Value = group.First().NetSalary - group.First().TotalDeductions;
                        worksheet.Cells[row + 3, 2].Style.Numberformat.Format = "#,##0.00";

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                        worksheet.View.FreezePanes(2, 1);
                    }

                    if (package.Workbook.Worksheets.Count == 0)
                    {
                        return BadRequest(new
                        {
                            Status = "Error",
                            Message = "No valid data to export.",
                            EmployeeCount = salaryStatements.Count
                        });
                    }

                    var fileName = $"Salary_Statements_{year}_{month:00}_Report.xlsx";
                    var stream = new MemoryStream(package.GetAsByteArray());
                    stream.Position = 0;
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An unexpected error occurred while generating the salary report.",
                    ErrorDetails = ex.Message
                });
            }
        }

    }
}



