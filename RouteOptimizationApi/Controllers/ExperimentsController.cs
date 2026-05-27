using Microsoft.AspNetCore.Mvc;
using RouteOptimizationApi.Data;
using RouteOptimizationApi.Models;

namespace RouteOptimizationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExperimentsController : ControllerBase
    {
        private readonly ApiDatabaseService databaseService;

        public ExperimentsController(ApiDatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }

        [HttpGet]
        public IActionResult GetExperiments()
        {
            return Ok(databaseService.GetExperiments());
        }

        [HttpPost]
        public IActionResult SaveExperiment([FromBody] ExperimentDto experiment)
        {
            databaseService.SaveExperiment(experiment);

            return Ok(new
            {
                message = "Experiment saved successfully"
            });
        }
    }
}
