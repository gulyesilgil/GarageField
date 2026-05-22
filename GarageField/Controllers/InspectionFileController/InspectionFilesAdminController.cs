using GarageField.Services.InspectionFileServices;
using Microsoft.AspNetCore.Mvc;

namespace GarageField.Controllers.InspectionFileController
{
    [ApiController]
    [Route("api/inspection-files")]
    public class InspectionFilesAdminController : ControllerBase
    {
        private readonly InspectionFileService _service;

        public InspectionFilesAdminController(InspectionFileService service)
        {
            _service = service;
        }

        // GET ALL FILES
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllFilesAsync();
            return Ok(result);
        }
    }
}