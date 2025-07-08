namespace HOMEOWNER.Models

{
    public class Facility
    {
        public int FacilityID { get; set; }
        public string? FacilityName { get; set; }
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public string? AvailabilityStatus { get; set; }
        public string? ImageUrl { get; set; } // Ensure this property exists
        public int? Rating { get; set; } = 0;

    }
}