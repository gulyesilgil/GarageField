using GarageField.Enums;

namespace GarageField.Entities
{
    public class Inspection : BaseEntity
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string InspectorName { get; set; }
        public InspectionStatus Status { get; set; }

        public List<InspectionFile> InspectionFiles { get; set; } = new();
   
    }
}
