using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private double CalculateDeduction(TimeSpan timeDifference, double salary)
        {
            double minutesLate = timeDifference.TotalMinutes;

            if (minutesLate < 15) return 0;  // No deduction for < 15 mins
            if (minutesLate <= 90) return salary / 21 / 2; // Half-day deduction
            return salary / 21; // Full-day deduction
        }
    }
}