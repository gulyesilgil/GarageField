using GarageField.DTOs.Inspection;
using GarageField.Services.InspectionServices;
using Microsoft.AspNetCore.Mvc;

namespace GarageField.Controllers.Inspection
{
    [ApiController]
    [Route("api/inspections")]
    public class InspectionsController : ControllerBase
    {
        private readonly InspectionService _service;

        public InspectionsController(InspectionService service)
        {
            _service = service;
        }

        // GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // GET BY ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateInspectionDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return Ok(id);
        }

        // UPDATE
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, CreateInspectionDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // PATCH STATUS
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateInspectionStatusDto dto)
        {
            var success = await _service.UpdateStatusAsync(id, dto.Status);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // GET STATUSES
        [HttpGet("statuses")]
        public IActionResult GetStatuses()
        {
            return Ok(_service.GetAllStatuses());
        }

        // DELETE
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}