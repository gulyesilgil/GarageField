using System.Collections.Generic;

namespace GarageField.DTOs.Inspection;

public class BulkCreateInspectionDto
{
    public List<CreateInspectionDto> Inspections { get; set; } = new();
}