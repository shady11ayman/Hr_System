using Hr_System_Demo_3.lookups;
using Hr_System_Demo_3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hr_System_Demo_3.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "superAdmin")]  
    public class LookupController(AppDbContext DbContext) : ControllerBase
    {
       
        [HttpGet("positions")]
        public async Task<ActionResult<IEnumerable<Position>>> GetPositions()
        {
            return Ok(await DbContext.Positions.ToListAsync());
        }

        
        [HttpPost("positions")]
        public async Task<ActionResult> AddPosition([FromBody] Position position)
        {
            if (string.IsNullOrWhiteSpace(position.Name))
                return BadRequest("Position name is required.");

            DbContext.Positions.Add(position);
            await DbContext.SaveChangesAsync();
            return Ok("Position added successfully.");
        }

        [HttpGet("shift-types")]
        public async Task<ActionResult<IEnumerable<ShiftType>>> GetShiftTypes()
        {
            return Ok(await DbContext.ShiftTypes.ToListAsync());
        }

        [HttpPost("shift-types")]
        public async Task<ActionResult> AddShiftType([FromBody] ShiftType shiftType)
        {
            if (string.IsNullOrWhiteSpace(shiftType.Name))
                return BadRequest("Shift type name is required.");

            DbContext.ShiftTypes.Add(shiftType);
            await DbContext.SaveChangesAsync();
            return Ok("Shift type added successfully.");
        }

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            return Ok(await DbContext.Departments.ToListAsync());
        }

   

        [HttpGet("contract-types")]
        public async Task<ActionResult<IEnumerable<ContractType>>> GetContractTypes()
        {
            return Ok(await DbContext.ContractTypes.ToListAsync());
        }

        [HttpPost("contract-types")]
        public async Task<ActionResult> AddContractType([FromBody] ContractType contractType)
        {
            if (string.IsNullOrWhiteSpace(contractType.Name))
                return BadRequest("Contract type name is required.");

            DbContext.ContractTypes.Add(contractType);
            await DbContext.SaveChangesAsync();
            return Ok("Contract type added successfully.");
        }

        [HttpGet("leave-types")]
        public async Task<ActionResult<IEnumerable<LeaveType>>> GetLeaveTypes()
        {
            return Ok(await DbContext.LeaveTypes.ToListAsync());
        }

        [HttpPost("leave-types")]
        public async Task<ActionResult> AddLeaveType([FromBody] LeaveType leaveType)
        {
            if (string.IsNullOrWhiteSpace(leaveType.Name))
                return BadRequest("Leave type name is required.");

            DbContext.LeaveTypes.Add(leaveType);
            await DbContext.SaveChangesAsync();
            return Ok("Leave type added successfully.");
        }
    }
}
