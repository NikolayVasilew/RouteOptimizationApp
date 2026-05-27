using Microsoft.AspNetCore.Mvc;
using RouteOptimizationApi.Data;
using RouteOptimizationApi.Models;

namespace RouteOptimizationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DispatcherController : ControllerBase
    {
        private readonly ApiDatabaseService databaseService;

        public DispatcherController(ApiDatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }

        [HttpGet("requests")]
        public IActionResult GetRequests()
        {
            return Ok(databaseService.GetDispatchRequests());
        }

        [HttpPost("requests")]
        public IActionResult CreateRequest([FromBody] DispatchRequestDto request)
        {
            databaseService.CreateDispatchRequest(request);

            return Ok(new
            {
                message = "Dispatch request created successfully"
            });
        }

        [HttpPut("requests/{id}/status")]
        public IActionResult UpdateStatus(int id, [FromQuery] string status)
        {
            databaseService.UpdateDispatchStatus(id, status);

            return Ok(new
            {
                message = "Dispatch request status updated successfully"
            });
        }
    }
}
