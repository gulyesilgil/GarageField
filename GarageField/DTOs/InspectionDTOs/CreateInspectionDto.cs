namespace GarageField.DTOs.Inspection
{
    public class CreateInspectionDto
    {
        public string ProductName { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string InspectorName { get; set; } = default!;
    }
}