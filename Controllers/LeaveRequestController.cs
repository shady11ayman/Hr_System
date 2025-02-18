using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hr_System_Demo_3.DTOs.LeaveRequest;
using Hr_System_Demo_3.Day_off_requests;
using System.Security.Claims;

namespace Hr_System_Demo_3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveRequestController(AppDbContext DbContext) : ControllerBase
    {
        [HttpPost("apply-leave-request")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult> ApplyLeaveRequest([FromBody] DTOs.LeaveRequest.ApplyLeaveRequestDto request)
        {
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employeeNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim) || string.IsNullOrEmpty(employeeNameClaim))
            {
                return Unauthorized("Invalid user credentials");
            }

            // Calculate total days off
            int totalDaysOff = (request.LeaveTo - request.LeaveFrom).Days + 1;

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = Guid.Parse(employeeIdClaim),
                EmployeeName = employeeNameClaim,
                LeaveType = request.LeaveType,
                LeaveFrom = request.LeaveFrom,
                LeaveTo = request.LeaveTo,
                workingDaysOff = totalDaysOff.ToString(), // Assuming all days are working days
                TotalDaysOff = totalDaysOff.ToString(),
                Status = LeaveStatus.Pending,
                Action = "Pending",
                comment = request.comment,
            };

            DbContext.LeaveRequests.Add(leaveRequest);
            await DbContext.SaveChangesAsync();

            return Ok(new { Message = "Leave request submitted successfully", LeaveRequestId = leaveRequest.Id });
        }

        [HttpGet("leave-requests")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetAllLeaveRequests()
        {
            var leaveRequests = await DbContext.LeaveRequests
                .Include(lr => lr.Employee) // Include related employee data
                .ToListAsync();

            return Ok(leaveRequests);
        }

        [HttpGet("my-leave-requests")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetMyLeaveRequests()
        {
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim))
            {
                return Unauthorized("Invalid user credentials");
            }

            var employeeId = Guid.Parse(employeeIdClaim);
            var myLeaveRequests = await DbContext.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId)
                .ToListAsync();

            return Ok(myLeaveRequests);
        }

        [HttpGet("hr-employee-leave-requests")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetHrEmployeeLeaveRequests()
        {
            // Extract HR ID from the token
            var hrIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(hrIdClaim))
            {
                return Unauthorized("Invalid HR credentials");
            }

            var hrId = Guid.Parse(hrIdClaim);

            // Find all employees added by this HR
            var employeeIds = await DbContext.Employees
                .Where(e => e.ManagerId == hrId)
                .Select(e => e.empId)
                .ToListAsync();

            if (!employeeIds.Any())
            {
                return NotFound("No employees found for this HR.");
            }

            // Get all leave requests for these employees
            var leaveRequests = await DbContext.LeaveRequests
                .Where(lr => employeeIds.Contains(lr.EmployeeId))
                .ToListAsync();

            if (!leaveRequests.Any())
            {
                return NotFound("No leave requests found for employees assigned by this HR.");
            }

            return Ok(leaveRequests);
        }

        [HttpPost("leave-request-action")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult> LeaveRequestAction([FromBody] LeaveRequestActionModel model)
        {
            var leaveRequest = await DbContext.LeaveRequests.FindAsync(model.LeaveRequestId);
            if (leaveRequest == null)
            {
                return NotFound("Leave request not found");
            }

            var employee = await DbContext.Employees.FindAsync(leaveRequest.EmployeeId);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            if (leaveRequest.Status != LeaveStatus.Pending)
            {
                return BadRequest("Leave request has already been processed");
            }

            if (model.IsApproved)
            {
                leaveRequest.Status = LeaveStatus.Accepted;
                leaveRequest.Action = "Approved";

                int leaveDays = (leaveRequest.LeaveTo - leaveRequest.LeaveFrom).Days + 1;
                leaveRequest.workingDaysOff = leaveDays.ToString();
                leaveRequest.TotalDaysOff = leaveDays.ToString();
            }
            else
            {
                leaveRequest.Status = LeaveStatus.Refused;
                leaveRequest.Action = "Rejected";
                leaveRequest.RejectReason = model.RejectReason ?? "No reason provided";
            }

            await DbContext.SaveChangesAsync();
            return Ok(new { Message = "Leave request processed successfully", LeaveRequestId = leaveRequest.Id, Status = leaveRequest.Status });
        }



    }
}