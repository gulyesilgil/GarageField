namespace GarageField.DTOs.Inspection
{
    public class UpdateInspectionDto
    {
        public string ProductName { get; set; } = default!;

        public string Description { get; set; } = default!;

        public string InspectorName { get; set; } = default!;

        public string Status { get; set; } = default!;
    }
}