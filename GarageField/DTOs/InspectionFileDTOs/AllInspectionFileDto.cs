namespace GarageField.DTOs.InspectionFile
{
    public class AllInspectionFileDto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
