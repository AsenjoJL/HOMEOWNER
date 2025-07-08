namespace HOMEOWNER.Models
{
    public class EventModel
    {
        public int EventID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }

        public int CreatedBy { get; set; } // AdminID or StaffID
    }
}
