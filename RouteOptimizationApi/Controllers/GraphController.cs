using Microsoft.AspNetCore.Mvc;
using RouteOptimizationApi.Data;

namespace RouteOptimizationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GraphController : ControllerBase
    {
        private readonly ApiDatabaseService databaseService;

        public GraphController(ApiDatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }

        [HttpGet("nodes")]
        public IActionResult GetNodes()
        {
            return Ok(databaseService.GetNodes());
        }

        [HttpGet("edges")]
        public IActionResult GetEdges()
        {
            return Ok(databaseService.GetEdges());
        }

        [HttpGet("db-path")]
        public IActionResult GetDatabasePath()
        {
            return Ok(databaseService.GetDatabasePath());
        }
    }
}
