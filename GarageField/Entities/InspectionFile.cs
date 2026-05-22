namespace GarageField.Entities
{
    public class InspectionFile : BaseEntity
    {
        public string FileName { get; set; }
        public String StoredFileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string BucketName { get; set; } = default!;


        public Guid InspectionId { get; set; }
        public Inspection Inspection { get; set; }
       
    }
}
