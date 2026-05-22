using GarageField.DTOs.Inspection;
using GarageField.Services.InspectionServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GarageField.Controllers;

[ApiController]
[Route("api/inspections")]
public class InspectionsController : ControllerBase
{
    private readonly InspectionService _service;

    public InspectionsController(InspectionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInspectionDto dto)
    {
        var result = await _service.CreateInspectionAsync(dto);
        return Ok(result);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] BulkCreateInspectionDto bulkDto)
    {
        if (bulkDto == null || !bulkDto.Inspections.Any())
            return BadRequest("Toplu kayıt verisi boş olamaz.");

        var result = await _service.CreateBulkInspectionsAsync(bulkDto);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllInspectionsAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetInspectionByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateInspectionDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateInspectionStatusDto dto)
    {
        var success = await _service.UpdateStatusAsync(id, dto.Status);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpGet("statuses")]
    public IActionResult GetAllStatuses()
    {
        var statuses = _service.GetAllStatuses();
        return Ok(statuses);
    }
}